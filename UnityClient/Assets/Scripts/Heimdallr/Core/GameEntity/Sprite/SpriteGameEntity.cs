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
        private DatabaseManager DatabaseManager;

        [SerializeField] private SpriteViewer SpriteViewer;
        private GameEntityMovementController MovementController;

        private Vector4 _pendingMove;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }

        [SerializeField] private GameEntityBaseStatus _Status;
        [SerializeField] private Direction EntityDirection;

        public override GameEntityBaseStatus Status => _Status;

        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
            EntityManager = FindObjectOfType<EntityManager>();
            DatabaseManager = FindObjectOfType<DatabaseManager>();
        }

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
                        SpriteViewer.FadeOut();
                    }

                    StartCoroutine(DestroyAfterSeconds(5));
                    break;
                case VanishType.OUT_OF_SIGHT:
                    EntityManager.HideEntity((uint)Status.AID);
                    break;
                case VanishType.LOGGED_OUT:
                case VanishType.TELEPORT:
                    StartCoroutine(DestroyAfterSeconds(VANISH_DESTROY_AFTER_SECONDS));
                    break;
            }
        }

        public override void SetAttackSpeed(ushort actionRequestSourceSpeed) {
            Status.AttackSpeed = actionRequestSourceSpeed;
        }

        public override void SetAction(ActionRequestType actionRequestAction) {
            switch (actionRequestAction) {
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
                    
                    ChangeMotion(
                        new MotionRequest { Motion = SpriteMotion.Attack2, forced = true },
                        new MotionRequest { Motion = SpriteMotion.Standby }
                    );
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

        private void Start() {
            MovementController = gameObject.AddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);

            if (_pendingMove != Vector4.zero) {
                StartMoving((int)_pendingMove.x, (int)_pendingMove.y, (int)_pendingMove.z, (int)_pendingMove.w);
            }
        }

        public override void ChangeMotion(MotionRequest request, MotionRequest? nextRequest = null) {
            SpriteViewer.ChangeMotion(request, nextRequest);
        }

        public override void ChangeDirection(Direction direction) {
            Direction = direction;
            EntityDirection = Direction;
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            DatabaseManager = FindObjectOfType<DatabaseManager>();

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
            PathFinder = FindObjectOfType<PathFinder>();
            DatabaseManager = FindObjectOfType<DatabaseManager>();

            _Status = spawnData;

            var x = posDir[0];
            var y = posDir[1];

            var pos = new Vector3(x, PathFinder.GetCellHeight(x, y), y);
            transform.position = pos;
            if (posDir.Length == 3) {
                // standing/idle entry
                var npcDirection = (NpcDirection)posDir[2];
                Direction = forceNorthDirection ? Direction.North : npcDirection.ToDirection();
            } else if (posDir.Length == 5) {
                //moving entry
                var x1 = posDir[2];
                var y1 = posDir[3];
                var npcDirection = (NpcDirection)posDir[4];
                Direction = npcDirection.ToDirection();
                _pendingMove = new Vector4(x, y, x1, y1);
            }

            var body = DatabaseManager.GetJobById(spawnData.Job) as SpriteJob;
            var bodySprite = (spawnData.EntityType != EntityType.PC || spawnData.IsMale) ? body.Male : body.Female;

            var bodyGO = new GameObject("Body");
            var spriteViewer = bodyGO.AddComponent<SpriteViewer>();
            SpriteViewer = spriteViewer;

            bodyGO.transform.localPosition = new Vector3(0, 0.25f, 0);
            bodyGO.transform.SetParent(transform, false);
            spriteViewer.Init(bodySprite, ViewerType.Body, this);

            if (spawnData.EntityType == EntityType.PC) {
                var head = DatabaseManager.GetHeadById(spawnData.HairStyle);
                var headSprite = spawnData.IsMale ? head.Male : head.Female;
                var headGO = new GameObject("Head");

                var headViewer = headGO.AddComponent<SpriteViewer>();
                headGO.transform.SetParent(bodyGO.transform, false);
                headViewer.Init(headSprite, ViewerType.Head, this);
                headViewer.SetParent(spriteViewer);

                spriteViewer.AddChildren(headViewer);
            }

            gameObject.SetActive(true);
        }

        private void StartMoving(
            int x,
            int y,
            int x1,
            int y2
        ) {
            MovementController.StartMoving(x, y, x1, y2, GameManager.Tick);
        }

        public override void ManagedUpdate() { }

        private IEnumerator DestroyAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            EntityManager.HideEntity((uint)Status.AID);
        }
    }
}