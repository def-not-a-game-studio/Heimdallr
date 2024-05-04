using Core.Path;
using UnityEngine;
using UnityRO.Core;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;

namespace Heimdallr.Core.Game.Controllers {
    public class GameEntityMovementController : ManagedMonoBehaviour {
        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private NetworkClient NetworkClient;
        private GameManager GameManager;

        #region Behaviour

        [SerializeField] private CoreGameEntity Entity;
        [SerializeField] private float RotateSpeed = 600f;
        

        #endregion

        #region PathFinding

        private CPathInfo pathInfo;
        private int pathStartCellIndex;
        private Direction direction;
        private long m_lastProcessStateTime;
        private long m_lastServerTime;
        private bool m_isNeverAnimation;
        private Vector3 MoveStartPosition;

        #endregion

        private void Awake() {
            GroundMask = LayerMask.GetMask("Ground");
            PathFinder = FindObjectOfType<PathFinder>();
            NetworkClient = FindObjectOfType<NetworkClient>();
            Entity = GetComponent<MeshGameEntity>();
            GameManager = GetComponent<GameManager>();
        }

        private void Start() {
            if (Entity.HasAuthority()) {
                NetworkClient.HookPacket<ZC.NOTIFY_PLAYERMOVE>(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            } else {
                NetworkClient.HookPacket<ZC.NOTIFY_MOVE>(ZC.NOTIFY_MOVE.HEADER, OnEntityMovement);
            }
            NetworkClient.HookPacket<ZC.STOPMOVE>(ZC.STOPMOVE.HEADER, OnEntityStop);
            NetworkClient.HookPacket<ZC.NPCACK_MAPMOVE>(ZC.NPCACK_MAPMOVE.HEADER, OnEntityMoved);

            pathInfo ??= new CPathInfo();
        }

        private void OnDestroy() {
            if (Entity.HasAuthority()) {
                NetworkClient.UnhookPacket<ZC.NOTIFY_PLAYERMOVE>(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            } else {
                NetworkClient.UnhookPacket<ZC.NOTIFY_MOVE>(ZC.NOTIFY_MOVE.HEADER, OnEntityMovement);
            }
            NetworkClient.UnhookPacket<ZC.STOPMOVE>(ZC.STOPMOVE.HEADER, OnEntityStop);
        }

        public override void ManagedUpdate() {
            ProcessState();
        }

        private void ProcessState() {
            var serverTime = GameManager.Tick;

            if (Entity.State == EntityState.Walk) {
                var serverDirection = 0;
                var nextCellPosition = Vector3.zero;
                var nextCellTime = serverTime;

                var previousServerDirection = 0;
                var previousCellPosition = Vector3.zero;
                var prevTime = 0L;

                pathStartCellIndex = pathInfo.GetNextCellInfo(
                    serverTime,
                    ref nextCellTime,
                    ref nextCellPosition,
                    ref serverDirection,
                    PathFinder.GetCellHeight
                );

                var pathPreviousCellIndex = pathInfo.GetPrevCellInfo(
                    serverTime,
                    ref prevTime,
                    ref previousCellPosition,
                    ref previousServerDirection,
                    PathFinder.GetCellHeight
                );

                var passedTime = serverTime - prevTime;
                var cellTime = nextCellTime - prevTime;

                if (pathPreviousCellIndex == 0) {
                    previousCellPosition = MoveStartPosition;
                }

                if (passedTime > cellTime) {
                    passedTime = cellTime;
                }

                if (passedTime >= 0 && cellTime > 0) {
                    var distance = nextCellPosition - previousCellPosition;
                    var position = previousCellPosition + distance * passedTime / cellTime;
                    CheckDirection(nextCellPosition, previousCellPosition);

                    transform.position = position;
                }

                if (pathStartCellIndex == -1 && serverTime >= nextCellTime) {
                    transform.position = nextCellPosition;

                    StopMoving();
                }
            }

            m_lastProcessStateTime = GameManager.Tick;
            m_lastServerTime = serverTime;
        }

        private void CheckDirection(Vector3 position, Vector3 prevPos) {
            var direction = PathFinder.GetDirectionForOffset(position, prevPos);
            if (this.direction != direction) {
                this.direction = direction;
                Entity.ChangeDirection(direction);
            }
        }

        public void SetEntity(CoreGameEntity entity) {
            Entity = entity;
        }

        /// <summary>
        /// Request the server to move to X,Y (Z in WorldSpace)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RequestMovement(int x, int y) {
            if (GameManager.IsOffline) {
                StartMoving(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z), x, y, GameManager.Tick);
            } else {
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
        public void StartMoving(int startX, int startY, int endX, int endY, long tick) {
            //Debug.Log($"Moving\n Start:{startX},{startY}\nDest:{endX},{endY}");

            pathInfo ??= new CPathInfo();
            var hasValidPath = FindPath(startX, startY, endX, endY, tick);

            if (hasValidPath) {
                MoveStartPosition = new Vector3(startX, PathFinder.GetCellHeight(startX, startY), startY);
                pathStartCellIndex = 0;
                Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Walk });
            }
        }

        private bool FindPath(int startX, int startY, int endX, int endY, long tick) {
            if (PathFinder == null) {
                PathFinder = FindObjectOfType<PathFinder>();
            }
            
            if (PathFinder is null) return false;

            return PathFinder.FindPath(
                tick,
                startX, startY,
                endX, endY,
                Entity.Status.MoveSpeed,
                pathInfo
            );
        }

        /// <summary>
        /// Stops moving the character.
        /// Clear the path finder nodes and set state back to Wait
        /// </summary>
        public void StopMoving() {
            m_isNeverAnimation = true;
            Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
        }

        private void OnEntityMovement(ushort cmd, int size, ZC.NOTIFY_MOVE packet) {
            if (packet.AID != Entity.Status.AID) return;
            StartMoving(packet.StartPosition[0], packet.StartPosition[1], packet.EndPosition[0], packet.EndPosition[1], GameManager.Tick);
        }

        private void OnPlayerMovement(ushort cmd, int size, ZC.NOTIFY_PLAYERMOVE packet) {
            StartMoving(packet.StartPosition[0], packet.StartPosition[1], packet.EndPosition[0], packet.EndPosition[1], GameManager.Tick);
        }
        
        private void OnEntityStop(ushort cmd, int size, ZC.STOPMOVE packet) {
            if (packet.AID != Entity.Status.AID || packet.AID != Entity.Status.GID) return;
            StartMoving((int)transform.position.x, (int)transform.position.z, packet.PosX, packet.PosY, GameManager.Tick);
        }
        
        private void OnEntityMoved(ushort cmd, int size, ZC.NPCACK_MAPMOVE packet) {
            StopMoving();

            PathFinder ??= FindObjectOfType<PathFinder>();
            
            var position = new Vector3(packet.PosX, PathFinder.GetCellHeight(packet.PosX, packet.PosY), packet.PosY);
            transform.position = position;
            
            new CZ.NOTIFY_ACTORINIT().Send();
        }
    }
}