using System;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;

namespace Heimdallr.Core.Game.Sprite {
    public class SpriteGameEntity : CoreSpriteGameEntity {
        [SerializeField] private SpriteViewer SpriteViewer;

        private GameEntityMovementController MovementController;

        public override Direction Direction { get; set; }

        public override int HeadDirection { get; }
        public override bool IsMonster { get; }

        private GameEntityBaseStatus _Status;

        public override GameEntityBaseStatus Status => _Status;

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == Session.CurrentSession.Entity?.GID;

        public override int GetEntityGID() => _Status.GID;
        
        public override void RequestOffsetMovement(Vector2 destination) {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination) {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
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

        public override void ManagedUpdate() {
            // do nothing
        }
    }
}