using System;
using System.Collections.Generic;
using Core.Path;
using Heimdallr.Core.Game.Sprite;
using UnityEngine;

namespace Heimdallr.Core.Game.Controllers {
    public class GameEntityMovementController : MonoBehaviour {
        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private NetworkClient NetworkClient;
        private GameManager GameManager;

        #region Behaviour

        [SerializeField] private CoreGameEntity Entity;
        [SerializeField] private float RotateSpeed = 600f;

        private bool IsMovementFromClick;
        private bool IsWalking;

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
            }

            pathInfo = new CPathInfo();
        }

        private void LateUpdate() {
            ProcessInputAsync();
            ProcessState();
        }

        private void ProcessState() {
            var serverTime = GameManager.Tick;

            if (IsWalking) {
                var prevPos = transform.position;

                var serverDirection = 0;
                var nextCellPosition = Vector3.zero;
                var nextCellTime = serverTime;

                var previousServerDirection = 0;
                var previousCellPosition = Vector3.zero;
                var prevTime = 0L;

                pathStartCellIndex = pathInfo.GetNextCellInfo(serverTime, ref nextCellTime, ref nextCellPosition,
                    ref serverDirection, PathFinder.GetCellHeight);

                var pathPreviousCellIndex = pathInfo.GetPrevCellInfo(serverTime, ref prevTime, ref previousCellPosition,
                    ref previousServerDirection, PathFinder.GetCellHeight);

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
                if (Entity is SpriteGameEntity spriteGameEntity) {
                    spriteGameEntity.Direction = direction;
                }
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
                MoveStartPosition = new Vector3(startX, PathFinder.GetCellHeight(startX, startY), startY);
                pathStartCellIndex = 0;
                IsWalking = true;
                Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Walk });
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
            m_isNeverAnimation = true;
            Entity.ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
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