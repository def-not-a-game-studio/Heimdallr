using Heimdallr.Core.Game.Controllers;
using System;
using System.Collections;
using UnityEngine;

namespace Heimdallr.Core.Game {
    public class GameEntity : CoreGameEntity, INetworkEntity {

        #region Components
        private GameEntityViewer EntityViewer;
        #endregion

        #region State
        public GameEntityState EntityState { get; private set; }
        public GameEntityData EntityData { get; private set; }
        #endregion

        #region Properties
        public bool HasAuthority => GetEntityGID() == Session.CurrentSession.Entity?.GetEntityGID();
        #endregion

        public void Init(GameEntityData data) {
            EntityData = data;

            var job = DatabaseManager.GetJobById(data.Job);
            EntityViewer = Instantiate<GameEntityViewer>(data.IsMale ? job.Male : job.Female, transform);
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

        public override int Direction { get; }
        public override int CameraDirection { get; }
        public override int HeadDir { get; }
        public override bool IsMonster { get; }
        public override GameEntityBaseStatus Status { get; }

        public override void ChangeMotion(MotionRequest request) {
            throw new NotImplementedException();
        }

        public override void StopCoroutine(Coroutine coroutine) {
            throw new NotImplementedException();
        }

        public override Coroutine StartCoroutine(IEnumerator coroutine) {
            throw new NotImplementedException();
        }
    }
}
