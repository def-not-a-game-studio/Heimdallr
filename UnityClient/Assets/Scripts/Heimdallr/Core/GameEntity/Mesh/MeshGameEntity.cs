using Heimdallr.Core.Game.Controllers;
using System;
using Heimdallr.Core.Database.Job;

namespace Heimdallr.Core.Game {
    public class MeshGameEntity : CoreMeshGameEntity {

        #region Components
        private MeshGameEntityViewer EntityViewer;
        #endregion

        #region State
        public GameEntityState EntityState { get; private set; }
        public GameEntityBaseStatus EntityData { get; private set; }
        #endregion

        #region Properties
        public bool HasAuthority => GetEntityGID() == Session.CurrentSession.Entity?.GID;
        #endregion

        public override void Init(GameEntityBaseStatus data) {
            EntityData = data;

            var job = DatabaseManager.GetJobById(data.Job) as MeshJob;
            EntityViewer = Instantiate<MeshGameEntityViewer>(data.IsMale ? job.Male : job.Female, transform);
            EntityViewer.SetGameEntityData(data);

            gameObject.AddComponent<GameEntityMovementController>();
        }

        public void SetState(GameEntityState state) {
            EntityState = state;
        }

        public void UpdateSprites() {
            throw new NotImplementedException();
        }

        public string GetEntityName() {
            return EntityData.Name;
        }

        public int GetEntityGID() {
            return EntityData.GID;
        }

        public EntityType GetEntityType() {
            return EntityData.EntityType;
        }

        public override GameEntityBaseStatus Status { get; }

        public override void ChangeMotion(MotionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
