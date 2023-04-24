using System.Collections.Generic;
using Core.Path;
using UnityEngine;

namespace Heimdallr.Core.Game.Controllers {
    public class GameEntityMovementController : MonoBehaviour {
        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private MeshGameEntity Entity;
        private NetworkClient NetworkClient;
        private GameManager GameManager;

        #region Behaviour

        [SerializeField] private float RotateSpeed = 600f;

        private bool IsMovementFromClick;
        private bool IsWalking;

        #endregion

        #region PathFinding

        private CPathInfo pathInfo;
        private int pathStartCell;
        private Direction direction;
        private long m_lastProcessStateTime;
        private long m_lastServerTime;
        private bool m_isNeverAnimation;
        private float m_MoveStartClientX;
        private float m_MoveStartClientY;

        #endregion


        private void Awake() {
            GroundMask = LayerMask.GetMask("Ground");
            PathFinder = FindObjectOfType<PathFinder>();
            NetworkClient = FindObjectOfType<NetworkClient>();
            Entity = GetComponent<MeshGameEntity>();
            GameManager = GetComponent<GameManager>();
        }

        private void Start() {
            if (Entity.HasAuthority) {
                NetworkClient.HookPacket(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            }

            pathInfo = new CPathInfo();
        }

        private void LateUpdate() {
            ProcessInputAsync();
            ProcessState();
        }

        private void ProcessState() {
            var serverTime = GameManager.Tick;
            var posChanged = false;

            if (IsWalking) {
                var prevPos = transform.position;

                int serverDirection = 0;
                int serverDirection2 = 0;
                float nextCellX = 0, nextCellY = 0;

                var nextCellTime = serverTime;
                pathStartCell = pathInfo.GetNextCellInfo(serverTime, ref nextCellTime, ref nextCellX, ref nextCellY,
                    ref serverDirection);

                float previousCellX = 0, previousCellY = 0;
                long PrevTime = 0;
                int r1 = pathInfo.GetPrevCellInfo(serverTime, ref PrevTime, ref previousCellX, ref previousCellY, ref serverDirection2);

                float nBX = nextCellX, nBY = nextCellY;
                float nBX2 = previousCellX, nBY2 = previousCellY;

                var passedTime = serverTime - PrevTime;
                var cellTime = nextCellTime - PrevTime;

                if (r1 == 0) {
                    nBX2 = (m_MoveStartClientX);
                    nBY2 = (m_MoveStartClientY);
                }

                if (passedTime > cellTime) {
                    passedTime = cellTime;
                }

                if (passedTime >= 0 && cellTime > 0) {
                    var dx = nBX - nBX2;
                    var dy = nBY - nBY2;

                    var x = nBX2 + dx * passedTime / cellTime;
                    var z = nBY2 + dy * passedTime / cellTime;

                    var nextCellHeight = PathFinder.GetCellHeight((int)nBX, (int)nBY);
                    var previousCellHeight = PathFinder.GetCellHeight((int)nBX2, (int)nBY2);
                    var diffHeight = nextCellHeight - previousCellHeight;
                    var y = previousCellHeight + diffHeight * passedTime / cellTime;

                    //Debug.Log($"{dHeight}");

                    transform.position = new Vector3(x, y, z);
                }

                if (!m_isNeverAnimation) {
                    var dir = (Vector2)(transform.position - prevPos).normalized;

                    if (dir != Vector2.zero) {
                        direction = PathFinder.GetDirectionForOffset(dir);
                        //SpriteAnimator.Direction = PathFinder.GetDirectionForOffset(dir);
                    }
                }

                // if (!m_isNeverAnimation) {
                //     SpriteAnimator.Direction = direction;
                // }

                if (pathStartCell == -1 && serverTime >= nextCellTime) {
                    transform.position = new Vector3(nBX, transform.position.y, nBY);

                    StopMoving();
                }
            }

            m_lastProcessStateTime = GameManager.Tick;
            m_lastServerTime = serverTime;
        }

        /// <summary>
        /// Request the server to move to X,Y (Z in WorldSpace)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RequestMovement(int x, int y) {
            if (GameManager.IsOffline) {
                StartMoving(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z), x, y);
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
        public void StartMoving(int startX, int startY, int endX, int endY) {
            //Debug.Log($"Moving\n Start:{startX},{startY}\nDest:{endX},{endY}");

            var hasValidPath = FindPath(startX, startY, endX, endY);

            if (hasValidPath) {
                m_MoveStartClientX = startX;
                m_MoveStartClientY = startY;
                pathStartCell = 0;
                IsWalking = true;
                Entity.SetState(GameEntityState.Walk);
            }
        }

        private bool FindPath(int startX, int startY, int endX, int endY) {
            return PathFinder.FindPath(
                GameManager.Tick,
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
            IsWalking = false;
            IsMovementFromClick = false;
            Entity.SetState(GameEntityState.Wait);
        }

        private void ProcessInputAsync() {
            var direction = GetAxisDirection();

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                IsMovementFromClick = true;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 500, GroundMask)) {
                    RequestMovement(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z));
                }
            } else if (direction != Vector3.zero) {
                var currentX = direction.x < 0
                    ? Mathf.FloorToInt(transform.position.x + direction.x)
                    : Mathf.CeilToInt(transform.position.x + direction.x);
                var currentZ = direction.z < 0
                    ? Mathf.FloorToInt(transform.position.z + direction.z)
                    : Mathf.CeilToInt(transform.position.z + direction.z);

                var destX = currentX;
                var destY = currentZ;
                RequestMovement(destX, destY);
            }
        }

        private Vector3Int GetAxisDirection() {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            var x = horizontal != 0 ? Mathf.Max(horizontal, 2f) * Mathf.Sign(horizontal) : 0;
            var y = vertical != 0 ? Mathf.Max(vertical, 2f) * Mathf.Sign(vertical) : 0;

            return new Vector3Int((int)x, 0, (int)y);
        }

        private void OnPlayerMovement(ushort cmd, int size, InPacket packet) {
            if (packet is not ZC.NOTIFY_PLAYERMOVE pkt) return;

            // Debug.Log(
            //     $"We're at {transform.position}\n" +
            //     $"Server answered from {new Vector2(pkt.startPosition[0], pkt.startPosition[1])} to {new Vector2(pkt.endPosition[0], pkt.endPosition[1])}"
            // );

            StartMoving(pkt.startPosition[0], pkt.startPosition[1], pkt.endPosition[0], pkt.endPosition[1]);
        }
    }
}