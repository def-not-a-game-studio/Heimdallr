using Heimdallr.Core.Game.Controllers;
using System;
using Heimdallr.Core.Database.Job;

namespace Heimdallr.Core.Game {
    public class MeshGameEntity : CoreMeshGameEntity {
        #region Components

        private MeshGameEntityViewer EntityViewer;
        private GameEntityMovementController MovementController;
        
        #endregion

        #region State

        private GameEntityBaseStatus _Status;
        
        #endregion

        public override void Init(GameEntityBaseStatus data) {
            _Status = data;
            gameObject.SetActive(true);

            //var job = DatabaseManager.GetJobById(data.Job) as MeshJob;
            //EntityViewer = Instantiate<MeshGameEntityViewer>(data.IsMale ? job.Male : job.Female, transform);
            //EntityViewer.SetGameEntityData(data);
        }

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == Session.CurrentSession.Entity?.GID;

        public override int GetEntityGID() => _Status.GID;
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
    }
}