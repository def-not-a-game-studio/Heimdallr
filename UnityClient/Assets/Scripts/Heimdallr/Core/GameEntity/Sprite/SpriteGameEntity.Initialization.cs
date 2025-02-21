using Core.Effects;
using Core.Effects.EffectParts;
using Core.Path;
using Heimdallr.Core.Game.Controllers;
using UnityEngine;
using UnityRO.Core;
using UnityRO.Core.Database;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;
using Cysharp.Threading.Tasks;

namespace Heimdallr.Core.Game.Sprite
{
    public partial class SpriteGameEntity
    {
        private void Awake()
        {
            SessionManager = FindObjectOfType<SessionManager>();
            PathFinder = FindObjectOfType<PathFinder>();
            EntityManager = FindObjectOfType<EntityManager>();
            DatabaseManager = FindObjectOfType<CustomDatabaseManager>();

            SessionManager.OnSessionMapChanged += this.OnMapChanged;
        }

        private void OnDestroy()
        {
            SessionManager.OnSessionMapChanged -= this.OnMapChanged;
        }

        private void OnMapChanged(GameMap map)
        {
            this._currentMap = map;
        }

        private void Start()
        {
            MovementController = gameObject.GetOrAddComponent<GameEntityMovementController>();
            MovementController.SetEntity(this);
            _state = EntityState.Idle;
        }

        private async void HandleSpawnData()
        {
            if (_spawnPosDir == null) return;

            var x = _spawnPosDir.posDir[0];
            var y = _spawnPosDir.posDir[1];

            var pos = new Vector3(x, PathFinder.GetCellHeight(x, y), y);
            transform.position = pos;

            EffectRenderer = new GameObject("Renderer").AddComponent<EffectRenderer>();
            EffectRenderer.gameObject.layer = LayerMask.NameToLayer("Portal");
            EffectRenderer.transform.SetParent(gameObject.transform, false);
            EffectRenderer.transform.localPosition = new Vector3(0.5f, 0, 0.5f);

            if ((JobType)_status.Job == JobType.JT_WARPNPC)
            {
                SpriteViewer.gameObject.SetActive(false);
                var effect = await Resources.LoadAsync("Database/Effects/WarpZone2") as Effect;
                EffectRenderer.InitEffects(effect);
                _spawnPosDir = null;
                return;
            }
            else
            {
                switch (_status.EntityType)
                {
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

            if (MovementController is null)
            {
                MovementController = gameObject.GetOrAddComponent<GameEntityMovementController>();
            }

            MovementController.SetEntity(this);

            if (_spawnPosDir.posDir.Length == 3)
            {
                // standing/idle entry
                var npcDirection = (NpcDirection)_spawnPosDir.posDir[2];
                Direction = _spawnPosDir.forceNorthDirection ? Direction.North : npcDirection.ToDirection();
            }

            var body = DatabaseManager.GetJobById(_status.Job) as SpriteJob;
            var bodySprite = (_status.EntityType != EntityType.PC || _status.IsMale) ? body.Male : body.Female;
            SpriteViewer.Init(bodySprite, ViewerType.Body, this);

            if (_status.EntityType == EntityType.PC)
            {
                var head = DatabaseManager.GetHeadById(_status.HairStyle);
                var headSprite = _status.IsMale ? head.Male : head.Female;

                var headViewer = SpriteViewer.FindChild(ViewerType.Head);
                if (headViewer == null) return;
                headViewer.Init(headSprite, ViewerType.Head, this);
                headViewer.gameObject.layer = LayerMask.NameToLayer("Player");
            }

            // handle moving entry after initializing sprite viewer
            if (_spawnPosDir.posDir.Length == 5)
            {
                ProcessMovingEntry(_spawnPosDir.posDir);
            }

            _spawnPosDir = null;
        }

        private void ProcessMovingEntry(int[] posDir)
        {
            var x = posDir[0];
            var y = posDir[1];
            var x1 = posDir[2];
            var y1 = posDir[3];
            var npcDirection = (NpcDirection)posDir[4];
            Direction = npcDirection.ToDirection();
            var move = new Vector4(x, y, x1, y1);
            StartMoving((int)move.x, (int)move.y, (int)move.z, (int)move.w);
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus)
        {
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

        public override void Spawn(GameEntityBaseStatus status, int[] posDir, bool forceNorthDirection)
        {
            _status = status;
            _spawnPosDir = new SpawnPosDir
            {
                posDir = posDir,
                forceNorthDirection = forceNorthDirection
            };
            gameObject.SetActive(true);
            SpriteViewer.gameObject.SetActive(true);
            StartCoroutine(SpriteViewer.FadeInRenderer(0, .5f));
        }

        public override void UpdateStatus(GameEntityBaseStatus status, int[] posDir, bool forceNorthDirection)
        {
            _status = status;
            if (posDir.Length == 5)
            {
                ProcessMovingEntry(posDir);
            }

            gameObject.SetActive(true);
        }
    }
}