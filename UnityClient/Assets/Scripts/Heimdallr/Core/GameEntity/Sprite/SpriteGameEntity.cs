using System.Collections;
using System.Collections.Generic;
using Core.Effects;
using Core.Path;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Core;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Sprite {

    public partial class SpriteGameEntity : CoreSpriteGameEntity {
        
        private SessionManager SessionManager;
        private PathFinder PathFinder;
        private EntityManager EntityManager;
        private CustomDatabaseManager DatabaseManager;
        private GameEntityMovementController MovementController;

        [SerializeField] private SpriteViewer SpriteViewer;
        [SerializeField] private EffectRenderer EffectRenderer;
        [SerializeField] private GameEntityBaseStatus _status;
        [SerializeField] private EntityState _state;
        [SerializeField] private Direction EntityDirection;

        private SpawnPosDir _spawnPosDir;
        private GameMap _currentMap;
        
        public override Direction Direction { get; set; }
        
        public override GameMap CurrentMap => this._currentMap;
        
        public override int HeadDirection { get; }

        public override GameEntityBaseStatus Status => _status;
        
        public override EntityState State => _state;

        public override bool HasAuthority() =>
            GameManager.IsOffline || GetEntityGID() == SessionManager.CurrentSession.Entity?.GetEntityGID();

        public override int GetEntityGID() => _status.GID;

        public override void SetState(EntityState state) {
            _state = state;
        }

        public override void ManagedUpdate() {
            HandleSpawnData();
            CheckMotionQueue();
            SpriteViewer.ManagedUpdate();
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

        private class SpawnPosDir {
            public int[] posDir;
            public bool forceNorthDirection;
        }
    }
}