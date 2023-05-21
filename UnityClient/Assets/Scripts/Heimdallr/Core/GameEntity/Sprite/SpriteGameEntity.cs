using Core.Path;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Core.Database;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Sprite {
    public class SpriteGameEntity : CoreSpriteGameEntity {
        private SessionManager SessionManager;
        private PathFinder PathFinder;

        [SerializeField] private SpriteViewer SpriteViewer;
        private GameEntityMovementController MovementController;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }

        private GameEntityBaseStatus _Status;

        public override GameEntityBaseStatus Status => _Status;

        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
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
                    break;
            }
        }

        private void Start() {
            MovementController = gameObject.AddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);
        }

        public override void ChangeMotion(MotionRequest request) {
            SpriteViewer.ChangeMotion(request);
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            _Status = gameEntityBaseStatus;
            gameObject.SetActive(true);
        }

        public override void Spawn(GameEntityBaseStatus spawnData, Vector2 pos, Direction direction) {
            _Status = spawnData;
            transform.position = new Vector3(pos.x, PathFinder.GetCellHeight((int)pos.x, (int)pos.y), pos.y);
            Direction = direction;

            var body = DatabaseManager.GetJobById(spawnData.Job) as SpriteJob;
            var bodySprite = (spawnData.EntityType != EntityType.PC || spawnData.IsMale) ? body.Male : body.Female;

            var bodyGO = new GameObject("Body");
            bodyGO.SetActive(false);

            var spriteViewer = bodyGO.AddComponent<SpriteViewer>();
            SpriteViewer = spriteViewer;

            bodyGO.transform.localPosition = new Vector3(0, 0.25f, 0);
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

        public override void ManagedUpdate() {
            // do nothing
        }
    }
}