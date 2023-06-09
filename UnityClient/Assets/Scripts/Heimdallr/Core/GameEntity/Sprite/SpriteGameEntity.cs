using System.Collections;
using Core.Path;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Core;
using UnityRO.Core.Database;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Sprite {
    public class SpriteGameEntity : CoreSpriteGameEntity {
        private const float VANISH_DESTROY_AFTER_SECONDS = 0.4f;

        private SessionManager SessionManager;
        private PathFinder PathFinder;
        private EntityManager EntityManager;
        private CustomDatabaseManager DatabaseManager;

        [SerializeField] private SpriteViewer SpriteViewer;
        private GameEntityMovementController MovementController;

        private Vector4 _pendingMove;
        private SpawnData _spawnData;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }

        [SerializeField] private GameEntityBaseStatus _Status;
        [SerializeField] private Direction EntityDirection;

        public override GameEntityBaseStatus Status => _Status;


        #region Initialization

        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
            EntityManager = FindObjectOfType<EntityManager>();
            DatabaseManager = FindObjectOfType<CustomDatabaseManager>();
        }

        private void Start() {
            MovementController = gameObject.AddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);

            if (_pendingMove != Vector4.zero) {
                StartMoving((int)_pendingMove.x, (int)_pendingMove.y, (int)_pendingMove.z, (int)_pendingMove.w);
                _pendingMove = Vector4.zero;
            }
        }

        private void HandleSpawnData() {
            if (_spawnData == null) return;

            var x = _spawnData.posDir[0];
            var y = _spawnData.posDir[1];

            var pos = new Vector3(x, PathFinder.GetCellHeight(x, y), y);
            transform.position = pos;
            if (_spawnData.posDir.Length == 3) {
                // standing/idle entry
                var npcDirection = (NpcDirection)_spawnData.posDir[2];
                Direction = _spawnData.forceNorthDirection ? Direction.North : npcDirection.ToDirection();
            } else if (_spawnData.posDir.Length == 5) {
                //moving entry
                var x1 = _spawnData.posDir[2];
                var y1 = _spawnData.posDir[3];
                var npcDirection = (NpcDirection)_spawnData.posDir[4];
                Direction = npcDirection.ToDirection();
                _pendingMove = new Vector4(x, y, x1, y1);
            }

            var body = DatabaseManager.GetJobById(_Status.Job) as SpriteJob;
            var bodySprite = (_Status.EntityType != EntityType.PC || _Status.IsMale) ? body.Male : body.Female;
            SpriteViewer.Init(bodySprite, ViewerType.Body, this);

            if (_Status.EntityType == EntityType.PC) {
                var head = DatabaseManager.GetHeadById(_Status.HairStyle);
                var headSprite = _Status.IsMale ? head.Male : head.Female;

                SpriteViewer.FindChild(ViewerType.Head)?.Init(headSprite, ViewerType.Head, this);
            }

            _spawnData = null;
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            DatabaseManager = FindObjectOfType<CustomDatabaseManager>();

            _Status = gameEntityBaseStatus;

            var body = DatabaseManager.GetJobById(gameEntityBaseStatus.Job) as SpriteJob;
            var bodySprite = (gameEntityBaseStatus.EntityType != EntityType.PC || gameEntityBaseStatus.IsMale)
                ? body.Male
                : body.Female;
            SpriteViewer.Init(bodySprite, ViewerType.Body, this);

            var head = DatabaseManager.GetHeadById(gameEntityBaseStatus.HairStyle);
            var headSprite = gameEntityBaseStatus.IsMale ? head.Male : head.Female;
            SpriteViewer.FindChild(ViewerType.Head)?.Init(headSprite, ViewerType.Head, this);

            gameObject.SetActive(true);
        }

        public override void Spawn(GameEntityBaseStatus spawnData, int[] posDir, bool forceNorthDirection) {
            _Status = spawnData;
            _spawnData = new SpawnData {
                posDir = posDir,
                forceNorthDirection = forceNorthDirection
            };
            gameObject.SetActive(true);
        }

        #endregion

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == SessionManager.CurrentSession.Entity?.GetEntityGID();

        public override int GetEntityGID() => _Status.GID;

        public override void RequestOffsetMovement(Vector2 destination) {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination) {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
        }

        public override void Vanish(VanishType vanishType) {
            MovementController.StopMoving();
            switch (vanishType) {
                case VanishType.DIED:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Dead });
                    
                    if (Status.EntityType != EntityType.PC) {
                        EntityManager.UnlinkEntity((uint)Status.AID);
                        SpriteViewer.FadeOut(5f);
                        StartCoroutine(DestroyAfterSeconds(5));
                    }

                    break;
                case VanishType.OUT_OF_SIGHT:
                    EntityManager.UnlinkEntity((uint)Status.AID);
                    StartCoroutine(HideAfterSeconds(2f));
                    break;
                case VanishType.LOGGED_OUT:
                case VanishType.TELEPORT:
                    EntityManager.DestroyEntity((uint)Status.AID);
                    break;
            }
        }

        public override void SetAttackSpeed(ushort actionRequestSourceSpeed) {
            Status.AttackSpeed = actionRequestSourceSpeed;
        }

        public override void SetAction(EntityActionRequest actionRequest, bool isSource) {
            switch (actionRequest.action) {
                case ActionRequestType.SIT:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Sit });
                    break;
                case ActionRequestType.ATTACK_MULTIPLE_NOMOTION:
                case ActionRequestType.ATTACK_MULTIPLE:
                case ActionRequestType.ATTACK_NOMOTION:
                case ActionRequestType.ATTACK_REPEAT:
                case ActionRequestType.ATTACK_CRITICAL:
                case ActionRequestType.ATTACK_LUCKY:
                case ActionRequestType.ATTACK:
                    ProcessAttack(actionRequest, isSource);
                    break;
                case ActionRequestType.ITEMPICKUP:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.PickUp });
                    break;
                case ActionRequestType.STAND:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
                    break;
                case ActionRequestType.SKILL:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Casting });
                    break;
                case ActionRequestType.SPLASH:
                    break;
                case ActionRequestType.TOUCHSKILL:
                    break;
                default:
                    break;
            }
        }

        private void ProcessAttack(EntityActionRequest actionRequest, bool isSource) {
            if (isSource) ProcessAttacker(actionRequest);
            else ProcessAttacked(actionRequest);
        }

        private void ProcessAttacked(EntityActionRequest actionRequest) {
            if (actionRequest.damage > 0 &&
                actionRequest.action is not (ActionRequestType.ATTACK_MULTIPLE_NOMOTION or ActionRequestType.ATTACK_NOMOTION)) {
                ChangeMotion(
                    new MotionRequest { Motion = SpriteMotion.Hit, forced = true },
                    new MotionRequest { Motion = SpriteMotion.Standby, delay = GameManager.Tick + actionRequest.sourceSpeed * 2 }
                );
            }
        }

        private void ProcessAttacker(EntityActionRequest actionRequest) {
            ChangeMotion(
                new MotionRequest { Motion = SpriteMotion.Attack, forced = true },
                new MotionRequest { Motion = SpriteMotion.Standby, delay = GameManager.Tick + actionRequest.sourceSpeed }
            );
        }

        public override void ChangeMotion(MotionRequest request, MotionRequest? nextRequest = null) {
            SpriteViewer.ChangeMotion(request, nextRequest);
        }

        public override void LookTo(Vector3 position) {
            var offset = new Vector2Int((int)position.x, (int)position.z) - new Vector2Int((int)transform.position.x, (int)transform.position.z);
            Direction = PathFinder.GetDirectionForOffset(offset);
            EntityDirection = Direction;
        }

        public override void ChangeDirection(Direction direction) {
            Direction = direction;
            EntityDirection = Direction;
        }

        private void StartMoving(int x, int y, int x1, int y2) {
            MovementController.StartMoving(x, y, x1, y2, GameManager.Tick);
        }

        public override void ManagedUpdate() {
            HandleSpawnData();
        }

        private IEnumerator HideAfterSeconds(float seconds) {
            yield return SpriteViewer.FadeOutRenderer(0, seconds);
            EntityManager.DestroyEntityObject(this);
        }

        private IEnumerator DestroyAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            EntityManager.DestroyEntityObject(this);
        }

        private class SpawnData {
            public int[] posDir;
            public bool forceNorthDirection;
        }
    }
}