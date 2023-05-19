using System;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Net;

namespace Heimdallr.Core.Game {
    public class MeshGameEntity : CoreMeshGameEntity {
        #region Components

        private MeshGameEntityViewer EntityViewer;
        private GameEntityMovementController MovementController;
        private SessionManager SessionManager;
        
        #endregion

        #region State

        private GameEntityBaseStatus _Status;
        
        #endregion

        private void Awake() {
            SessionManager = FindObjectOfType<SessionManager>();
        }

        public override void Init(GameEntityBaseStatus data) {
            _Status = data;
            gameObject.SetActive(true);

            //var job = DatabaseManager.GetJobById(data.Job) as MeshJob;
            //EntityViewer = Instantiate<MeshGameEntityViewer>(data.IsMale ? job.Male : job.Female, transform);
            //EntityViewer.SetGameEntityData(data);
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
        
        public override void Spawn(GameEntityBaseStatus spawnData, Vector2 pos, Direction direction) {
            throw new NotImplementedException();
        }

        public override void Vanish(ZC.NOTIFY_VANISH.VanishType vanishType) {
            throw new NotImplementedException();
        }

        public override GameEntityBaseStatus Status => _Status;


        private void Start() {
            MovementController = gameObject.AddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);
        }

        public string GetEntityName() => _Status.Name;
        public EntityType GetEntityType() => _Status.EntityType;

        public override void ChangeMotion(MotionRequest request) {
            throw new NotImplementedException();
        }

        public override void ManagedUpdate() {
            // do nothing
        }
    }
}