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
        private const int VANISH_DESTROY_AFTER_SECONDS = 2;

        private SessionManager SessionManager;
        private PathFinder PathFinder;
        private EntityManager EntityManager;

        [SerializeField] private SpriteViewer SpriteViewer;
        private GameEntityMovementController MovementController;

        private Vector4 _pendingMove;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }

        private GameEntityBaseStatus _Status;

        public override GameEntityBaseStatus Status => _Status;

        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
            EntityManager = FindObjectOfType<EntityManager>();
        }

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == SessionManager.CurrentSession.Entity?.GID;

        public override int GetEntityGID() => _Status.GID;

        public override void RequestOffsetMovement(Vector2 destination) {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination) {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
        }

        public override void Vanish(VanishType vanishType) {
            switch (vanishType) {
                case VanishType.DIED:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Dead });
                    StartCoroutine(DestroyAfterSeconds(VANISH_DESTROY_AFTER_SECONDS));
                    break;
                case VanishType.LOGGED_OUT:
                    StartCoroutine(DestroyAfterSeconds(VANISH_DESTROY_AFTER_SECONDS));
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

        public override void ChangeMotion(MotionRequest request) {
            SpriteViewer.ChangeMotion(request);
        }

        public override void ChangeDirection(Direction direction) {
            Direction = direction;
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            _Status = gameEntityBaseStatus;
            gameObject.SetActive(true);
        }

        public override void Spawn(GameEntityBaseStatus spawnData, int[] posDir, bool forceNorthDirection) {
            _Status = spawnData;

            var x = posDir[0];
            var y = posDir[1];

            transform.position = new Vector3(x, PathFinder.GetCellHeight(x, y), y);
            if (posDir.Length == 3) { // standing/idle entry
                var npcDirection = (NpcDirection)posDir[2];
                Direction = forceNorthDirection ? Direction.North : npcDirection.ToDirection();
            } else if (posDir.Length == 5) { //moving entry
                var x1 = posDir[2];
                var y1 = posDir[3];
                var npcDirection = (NpcDirection)posDir[4];
                Direction = npcDirection.ToDirection();
                _pendingMove = new Vector4(x, y, x1, y1);
            }

            var body = DatabaseManager.GetJobById(spawnData.Job) as SpriteJob;
            var bodySprite = (spawnData.EntityType != EntityType.PC || spawnData.IsMale) ? body.Male : body.Female;

            var bodyGO = new GameObject("Body");
            bodyGO.SetActive(false);

            var spriteViewer = bodyGO.AddComponent<SpriteViewer>();
            SpriteViewer = spriteViewer;

            bodyGO.transform.localPosition = new Vector3(0.5f, 0.25f, 0.5f);
            bodyGO.transform.SetParent(transform, false);
            spriteViewer.Init(bodySprite, ViewerType.Body, this);

            if (spawnData.EntityType == EntityType.PC) {
                var head = DatabaseManager.GetHeadById(spawnData.HairStyle);
                var headSprite = spawnData.IsMale ? head.Male : head.Female;
                var headGO = new GameObject("Head");
                headGO.SetActive(false);

                var headViewer = headGO.AddComponent<SpriteViewer>();
                headGO.transform.SetParent(bodyGO.transform, false);
                headViewer.Init(headSprite, ViewerType.Head, this);
                headViewer.SetParent(spriteViewer);

                spriteViewer.AddChildren(headViewer);
            }

            bodyGO.SetActiveRecursively(true);
        }

        private void StartMoving(int x, int y, int x1, int y2) {
            MovementController.StartMoving(x, y, x1, y2, GameManager.Tick);
        }

        public override void ManagedUpdate() {
            // do nothing
        }

        private IEnumerator DestroyAfterSeconds(int seconds) {
            yield return new WaitForSeconds(seconds);
            EntityManager.RemoveEntity((uint)Status.AID);
        }
    }
}