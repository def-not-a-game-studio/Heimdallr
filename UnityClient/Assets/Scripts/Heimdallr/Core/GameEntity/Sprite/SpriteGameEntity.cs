using System.Collections;
using Core.Effects;
using Core.Effects.EffectParts;
using Core.Path;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRO.Core;
using UnityRO.Core.Database;
using UnityRO.Core.Database.Emotion;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Sprite {
    public class SpriteGameEntity : CoreSpriteGameEntity {
        private SessionManager SessionManager;
        private PathFinder PathFinder;
        private EntityManager EntityManager;
        private CustomDatabaseManager DatabaseManager;

        [SerializeField] private SpriteViewer SpriteViewer;
        private EffectRenderer EffectRenderer;
        private GameEntityMovementController MovementController;

        private Vector4 _pendingMove;
        private SpawnData _spawnData;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }

        [FormerlySerializedAs("_Status")] [SerializeField]
        private GameEntityBaseStatus _status;

        [SerializeField] private EntityState _state;

        [SerializeField] private Direction EntityDirection;

        public override GameEntityBaseStatus Status => _status;
        public override EntityState State => _state;

        #region Initialization
        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
            EntityManager = FindObjectOfType<EntityManager>();
            DatabaseManager = FindObjectOfType<CustomDatabaseManager>();
        }

        private void Start() {
            MovementController = gameObject.GetOrAddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);
            _state = EntityState.Idle;
        }

        private void HandleSpawnData() {
            if (_spawnData == null) return;

            var x = _spawnData.posDir[0];
            var y = _spawnData.posDir[1];

            var pos = new Vector3(x, PathFinder.GetCellHeight(x, y), y);
            transform.position = pos;

            if ((JobType)_status.Job == JobType.JT_WARPNPC) {
                SpriteViewer.gameObject.SetActive(false);
                EffectRenderer = new GameObject("Renderer").AddComponent<EffectRenderer>();
                EffectRenderer.gameObject.layer = LayerMask.NameToLayer("Portal");
                EffectRenderer.transform.SetParent(gameObject.transform, false);
                EffectRenderer.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
                var resourceRequest = Resources.LoadAsync<Effect>("Database/Effects/WarpZone2");
                resourceRequest.completed += (op) => {
                    EffectRenderer.Effect = resourceRequest.asset as Effect;
                    EffectRenderer.InitEffects();
                };
                _spawnData = null;
                return;
            } else {
                switch (_status.EntityType) {
                    case EntityType.MOB:
                        SpriteViewer.gameObject.layer = LayerMask.NameToLayer("Monster");
                        break;
                    case EntityType.NPC:
                        SpriteViewer.gameObject.layer = LayerMask.NameToLayer("NPC");
                        break;
                    case EntityType.PC:
                        SpriteViewer.gameObject.layer = LayerMask.NameToLayer("Player");
                        break;
                    case EntityType.ITEM:
                        SpriteViewer.gameObject.layer = LayerMask.NameToLayer("Item");
                        break;
                    default:
                        break;
                }
            }

            if (MovementController == null) {
                MovementController = gameObject.GetOrAddComponent<GameEntityMovementController>();
            }

            MovementController.SetEntity(this);

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

            var body = DatabaseManager.GetJobById(_status.Job) as SpriteJob;
            var bodySprite = (_status.EntityType != EntityType.PC || _status.IsMale) ? body.Male : body.Female;
            SpriteViewer.Init(bodySprite, ViewerType.Body, this);

            if (_status.EntityType == EntityType.PC) {
                var head = DatabaseManager.GetHeadById(_status.HairStyle);
                var headSprite = _status.IsMale ? head.Male : head.Female;

                var headViewer = SpriteViewer.FindChild(ViewerType.Head);
                if (headViewer == null) return;
                headViewer.Init(headSprite, ViewerType.Head, this);
                headViewer.gameObject.layer = LayerMask.NameToLayer("Player");
            }

            if (_pendingMove != Vector4.zero) {
                StartMoving((int)_pendingMove.x, (int)_pendingMove.y, (int)_pendingMove.z, (int)_pendingMove.w);
                _pendingMove = Vector4.zero;
            }

            _spawnData = null;
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            DatabaseManager = FindObjectOfType<CustomDatabaseManager>();

            _status = gameEntityBaseStatus;

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

        public override void Spawn(GameEntityBaseStatus status, int[] posDir, bool forceNorthDirection) {
            _status = status;
            _spawnData = new SpawnData {
                posDir = posDir,
                forceNorthDirection = forceNorthDirection
            };
            gameObject.SetActive(true);
            SpriteViewer.gameObject.SetActive(true);
        }

        public override void UpdateStatus(GameEntityBaseStatus status) {
            _status = status;
            gameObject.SetActive(true);
        }
        #endregion

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == SessionManager.CurrentSession.Entity?.GetEntityGID();

        public override int GetEntityGID() => _status.GID;

        public override void RequestOffsetMovement(Vector2 destination) {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination) {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
        }

        public override void Vanish(VanishType vanishType) {
            _state = EntityState.Vanish;
            switch (vanishType) {
                case VanishType.DIED:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Dead });

                    if (Status.EntityType != EntityType.PC) {
                        EntityManager.UnlinkEntity((uint)Status.AID);
                        SpriteViewer.FadeOut(2f, 5f);
                        StartCoroutine(DestroyAfterSeconds(5));
                    }

                    break;
                case VanishType.OUT_OF_SIGHT:
                    if (EffectRenderer != null) {
                        EffectRenderer.Vanish();
                        EffectRenderer = null;
                    }

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

        public override void SetAttackedSpeed(ushort actionRequestTargetSpeed) {
            Status.AttackedSpeed = actionRequestTargetSpeed;
        }

        public override void ShowEmotion(byte emotionType) {
            var emotionIndex = DatabaseManager.GetEmotionIndex((EmotionType)emotionType);
            var request = Resources.LoadAsync("Sprites/emotions");
            request.completed += (op) => {
                var emotionViewer = new GameObject("emotion").AddComponent<SpriteEffectViewer>();
                emotionViewer.transform.SetParent(SpriteViewer.transform, false);
                emotionViewer.transform.localPosition = new Vector3(0, 2, 0);
                emotionViewer.Init(request.asset as SpriteData, ViewerType.Emotion, this);
                emotionViewer.SetActionIndex(emotionIndex);
            };
        }

        public override void ChangeLook(LookType lookType, short packetValue, short packetValue2) {
            switch (lookType) {
                case LookType.LOOK_BASE:
                    _status.Job = packetValue;
                    var job = DatabaseManager.GetJobById(packetValue) as SpriteJob;
                    SpriteViewer.Init((_status.EntityType != EntityType.PC || _status.IsMale) ? job.Male : job.Female,
                        ViewerType.Body, this);
                    break;
                case LookType.LOOK_HAIR:
                    _status.HairStyle = packetValue;
                    var head = DatabaseManager.GetHeadById(packetValue);
                    SpriteViewer.FindChild(ViewerType.Head)
                        ?.Init((_status.EntityType != EntityType.PC || _status.IsMale) ? head.Male : head.Female,
                            ViewerType.Head, this);
                    break;
                case LookType.LOOK_CLOTHES_COLOR:
                    _status.ClothesColor = packetValue;
                    SpriteViewer.UpdatePalette();
                    break;
                case LookType.LOOK_HAIR_COLOR:
                    _status.HairColor = packetValue;
                    SpriteViewer.FindChild(ViewerType.Head)?.UpdatePalette();
                    break;
                default:
                    break;
            }
        }

        public override void SetAction(EntityActionRequest actionRequest, bool isSource, long delay = 0) {
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
                    ProcessAttack(actionRequest, isSource, delay);
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

        public override float GetActionDelay(EntityActionRequest actionRequest) {
            switch (actionRequest.action) {
                case ActionRequestType.ATTACK_MULTIPLE_NOMOTION:
                case ActionRequestType.ATTACK_MULTIPLE:
                case ActionRequestType.ATTACK_NOMOTION:
                case ActionRequestType.ATTACK_REPEAT:
                case ActionRequestType.ATTACK_CRITICAL:
                case ActionRequestType.ATTACK_LUCKY:
                case ActionRequestType.ATTACK:
                    return SpriteViewer.GetAttackDelay();
                default:
                    return SpriteViewer.GetDelay();
            }
        }

        private void ProcessAttack(EntityActionRequest actionRequest, bool isSource, long delay) {
            if (isSource) ProcessAttacker(actionRequest);
            else ProcessAttacked(actionRequest, delay);
        }

        private void ProcessAttacked(EntityActionRequest actionRequest, long delay) {
            if (actionRequest.damage > 0 &&
                actionRequest.action is not (ActionRequestType.ATTACK_MULTIPLE_NOMOTION or ActionRequestType.ATTACK_NOMOTION)) {
                ChangeMotion(
                    new MotionRequest {
                        Motion = SpriteMotion.Hit,
                        forced = true,
                        startTime = delay
                    }
                );
            }
        }

        private void ProcessAttacker(EntityActionRequest actionRequest) {
            ChangeMotion(
                new MotionRequest { Motion = SpriteMotion.Attack, forced = true }
            );
        }

        public override void ChangeMotion(MotionRequest request) {
            var state = request.Motion switch {
                            SpriteMotion.Idle => EntityState.Idle,
                            SpriteMotion.Standby => EntityState.Standby,
                            SpriteMotion.Walk => EntityState.Walk,
                            SpriteMotion.Attack => EntityState.Attack,
                            SpriteMotion.Attack1 => EntityState.Attack,
                            SpriteMotion.Attack2 => EntityState.Attack,
                            SpriteMotion.Attack3 => EntityState.Attack,
                            SpriteMotion.Dead => EntityState.Dead,
                            SpriteMotion.Hit => EntityState.Hit,
                            SpriteMotion.Casting => EntityState.Cast,
                            SpriteMotion.PickUp => EntityState.PickUp,
                            SpriteMotion.Freeze1 => EntityState.Freeze,
                            SpriteMotion.Freeze2 => EntityState.Freeze,
                            SpriteMotion.Sit => EntityState.Sit,
                            _ => EntityState.Idle
                        };

            if (state == State && !request.forced) {
                return;
            }
            _state = state;
            SpriteViewer.ChangeMotion(request);
        }

        public override void LookTo(Vector3 position) {
            var offset = new Vector2Int((int)position.x, (int)position.z) -
                         new Vector2Int((int)transform.position.x, (int)transform.position.z);
            Direction = PathFinder.GetDirectionForOffset(offset);
            EntityDirection = Direction;
        }

        public override void ChangeDirection(Direction direction) {
            Direction = direction;
            EntityDirection = Direction;
        }

        private void StartMoving(
            int x, int y, int x1,
            int y2
        ) {
            MovementController.StartMoving(x, y, x1, y2, GameManager.Tick);
        }

        public override void RequestAction(CoreGameEntity target) {
            var actionPacket = new CZ.REQUEST_ACT2 {
                TargetID = (uint)target.GetEntityAID(),
                action = EntityActionType.CONTINUOUS_ATTACK
            };
            actionPacket.Send();
        }

        public override void TalkToNpc(CoreSpriteGameEntity target) {
            new CZ.CONTACTNPC {
                NAID = (uint)target.GetEntityAID(),
                Type = 1
            }.Send();
        }

        public override void SetState(EntityState state) {
            _state = state;
        }

        public override void ManagedUpdate() {
            HandleSpawnData();
        }

        private IEnumerator HideAfterSeconds(float seconds) {
            yield return SpriteViewer.FadeOutRenderer(0, seconds);
            SpriteViewer.Teardown();
            EntityManager.RecycleEntity(this);
        }

        private IEnumerator DestroyAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            SpriteViewer.Teardown();
            EntityManager.RecycleEntity(this);
        }

        private class SpawnData {
            public int[] posDir;
            public bool forceNorthDirection;
        }
    }
}