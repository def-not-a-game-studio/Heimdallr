﻿using System;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Core.GameEntity;
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
            GameManager.IsOffline || GetEntityGID() == SessionManager.CurrentSession.Entity?.GetEntityGID();

        public override int GetEntityGID() => _Status.GID;
        
        public override void RequestOffsetMovement(Vector2 destination) {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination) {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
        }
        
        public override void Spawn(GameEntityBaseStatus spawnData, int[] posDir, bool forceNorthDirection) {
            throw new NotImplementedException();
        }

        public override void Vanish(VanishType vanishType) {
            throw new NotImplementedException();
        }

        public override float GetActionDelay(EntityActionRequest actionRequest) {
            throw new NotImplementedException();
        }

        public override void RequestAction(CoreGameEntity target) {
            throw new NotImplementedException();
        }
        public override void TalkToNpc(CoreSpriteGameEntity target) {
            throw new NotImplementedException();
        }
        public override void SetAttackedSpeed(ushort actionRequestTargetSpeed) {
            throw new NotImplementedException();
        }

        public override void SetAttackSpeed(ushort actionRequestSourceSpeed) {
            Status.AttackSpeed = actionRequestSourceSpeed;
        }

        public override void ShowEmotion(byte emotionType) {
            throw new NotImplementedException();
        }

        public override void ChangeLook(LookType lookType, short packetValue, short packetValue2) {
            throw new NotImplementedException();
        }

        public override void UpdateStatus(GameEntityBaseStatus getBaseStatus) {
            throw new NotImplementedException();
        }

        public override void SetAction(EntityActionRequest actionRequestAction, bool isSource, float delay = 0f) {
            throw new NotImplementedException();
        }

        public override void LookTo(Vector3 position) {
            throw new NotImplementedException();
        }

        public override GameEntityBaseStatus Status => _Status;

        private void Start() {
            MovementController = gameObject.AddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);
        }

        public string GetEntityName() => _Status.Name;
        public EntityType GetEntityType() => _Status.EntityType;

        public override void ChangeMotion(MotionRequest request, MotionRequest? nextRequest = null) {
            throw new NotImplementedException();
        }

        public override void ChangeDirection(Direction direction) {
            throw new NotImplementedException();
        }

        public override void ManagedUpdate() {
            // do nothing
        }
    }
}