using System.Linq;
using Core.Path;
using UnityEngine;
using UnityRO.Core;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Controllers
{
    public partial class GameEntityMovementController : ManagedMonoBehaviour
    {
        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private NetworkClient NetworkClient;
        private GameManager GameManager;

        #region Behaviour

        [SerializeField] private CoreGameEntity Entity;
        [SerializeField] private float RotateSpeed = 600f;

        #endregion

        private void Awake()
        {
            GroundMask = LayerMask.GetMask("Ground");
            PathFinder = FindObjectOfType<PathFinder>();
            NetworkClient = FindObjectOfType<NetworkClient>();
            Entity = GetComponent<MeshGameEntity>();
            GameManager = FindObjectOfType<GameManager>();
        }

        private void Start()
        {
            HookPackets();
            pathInfo ??= new CPathInfo();
        }

        private void OnDestroy()
        {
            UnhookPackets();
        }

        public override void ManagedUpdate()
        {
            ProcessState();
        }

        public void SetEntity(CoreGameEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// Request the server to move to X,Y (Z in WorldSpace)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RequestMovement(int x, int y)
        {
            if (GameManager.IsOffline)
            {
                StartMoving(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z), x, y,
                    GameManager.Tick);
            }
            else
            {
                new CZ.REQUEST_MOVE2(x, y, 0).Send();
            }
        }

        /// <summary>
        /// Server has acknowledged our request and we're good to go.
        /// Use to bypass server request (ie: Offline mode)
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        public void StartMoving(int startX, int startY, int endX, int endY, long tick)
        {
            //Debug.Log($"Moving\n Start:{startX},{startY}\nDest:{endX},{endY}");

            pathInfo ??= new CPathInfo();
            GameManager.SetServerTick(tick);
            var hasValidPath = FindPath(startX, startY, endX, endY, tick);

            if (hasValidPath)
            {
                // Debug.Log($"[{GameManager.Tick}] ({startX},0,{startY}) -> {transform.position}");
                // MoveStartPosition = new Vector3(startX, PathFinder.GetCellHeight(startX, startY), startY);
                MoveStartPosition = transform.position;
                pathStartCellIndex = 0;
                Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Walk });
            }
        }

        /// <summary>
        /// Stops moving the character.
        /// Clear the path finder nodes and set state back to Wait
        /// </summary>
        public void StopMoving()
        {
            m_isNeverAnimation = true;
            Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
        }

        public void DelayMovement(long delay)
        {
            float xPos = 0f, yPos = 0f;
            int dir = 0;
            var index = pathInfo.GetPos(GameManager.Tick, ref xPos, ref yPos, ref dir);
            var remainingCells = pathInfo.PathData.Skip(index + 1).Select(it =>
            {
                it.Time += delay;
                return it;
            }).ToArray();
            pathInfo.SetNewPathInfo(remainingCells, remainingCells.Length);
        }
    }
}