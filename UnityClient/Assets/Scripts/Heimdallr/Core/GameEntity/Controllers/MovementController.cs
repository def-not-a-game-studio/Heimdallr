using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Heimdallr.Core.Game.Controllers {
    public class GameEntityMovementController : MonoBehaviour {
        private const int MoveAnimFPS = 30 / 2;

        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private MeshGameEntity meshGameEntity;
        private NetworkClient NetworkClient;
        private long _tick;

        #region Behaviour

        [SerializeField] private float RotateSpeed = 600f;

        private bool IsMovementFromClick;
        private bool IsWalking;
        private int NodeIndex;
        private List<Vector3> Nodes;

        #endregion

        private void Awake() {
            GroundMask = LayerMask.GetMask("Ground");
            PathFinder = FindObjectOfType<PathFinder>();
            NetworkClient = FindObjectOfType<NetworkClient>();
            meshGameEntity = GetComponent<MeshGameEntity>();
        }

        private void Start() {
            if (meshGameEntity.HasAuthority) {
                NetworkClient.HookPacket(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            }
        }

        private long lastSpeed = 150;
        private Vector3 lastPosition;

        private void Update() {
            ProcessInputAsync();
            var shouldEnd = false;

            if (IsWalking && Nodes.Count > 0) {
                while (_tick <= GameManager.Tick) {
                    if (NodeIndex == Nodes.Count - 1) {
                        shouldEnd = true;
                        break;
                    }
                    
                    var current = Nodes[NodeIndex];
                    var next = Nodes[NodeIndex + 1];
                    var isDiagonal = PathFinder.IsDiagonal(next, current);
                    lastSpeed = (ushort) (isDiagonal ? meshGameEntity.EntityData.MoveSpeed * 14 / 10 : meshGameEntity.EntityData.MoveSpeed); //Diagonal walking is slower
                    _tick += lastSpeed;
                    
                    //_tick += meshGameEntity.EntityData.MoveSpeed;
                    NodeIndex++;
                }

                var deltaTime = 1f - Mathf.Max(_tick - GameManager.Tick, 0f) / lastSpeed;

                var currentNode = NodeIndex == 0 ? lastPosition : Nodes[NodeIndex - 1];
                var nextNode = Nodes[NodeIndex];
                var direction = nextNode - currentNode;
                var rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up), RotateSpeed * Time.deltaTime);

                transform.position = currentNode + direction * deltaTime;
                transform.rotation = rotation;

                if (shouldEnd) {
                    StopMoving();
                }
            }
        }

        private void ProcessInput() {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                IsMovementFromClick = true;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 500, GroundMask)) {
                    RequestMovement(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z));
                }
            } else if (GetAxisDirection() == Vector3.zero && IsWalking && !IsMovementFromClick) {
                StopMoving();
            }
        }

        private bool IsAwaitingResponse = false;

        private void ProcessInputAsync() {
            var direction = GetAxisDirection();
            
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                IsMovementFromClick = true;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 500, GroundMask)) {
                    RequestMovement(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z));
                }
            } else if (direction != Vector3.zero && !IsAwaitingResponse) {
                Debug.Log($"Direction {direction}");
                IsAwaitingResponse = true;

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

        /// <summary>
        /// Request the server to move to X,Y (Z in WorldSpace)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RequestMovement(int x, int y) {
            new CZ.REQUEST_MOVE2(x, y, 0).Send();
            //StartMoving((int) transform.position.x, (int) transform.position.z, x, y);
        }

        private void OnPlayerMovement(ushort cmd, int size, InPacket packet) {
            if (packet is not ZC.NOTIFY_PLAYERMOVE pkt) return;
            Debug.Log(
                $"We're at {transform.position}\nServer answered from {new Vector2(pkt.startPosition[0], pkt.startPosition[1])} to {new Vector2(pkt.endPosition[0], pkt.endPosition[1])}");
            IsAwaitingResponse = false;
            StartMoving(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z), pkt.endPosition[0], pkt.endPosition[1]);
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
            _tick = GameManager.Tick;
            NodeIndex = 0;
            Nodes = PathFinder
                .GetPath(startX, startY, endX, endY)
                .Select(node => new Vector3(node.x, (float)node.y, node.z))
                .ToList();

            if (Nodes.Count > 0) {
                IsWalking = true;
                meshGameEntity.SetState(GameEntityState.Walk);
            }

            lastPosition = transform.position;
        }

        private void StartMoving(Vector4 data) {
            StartMoving(Mathf.FloorToInt(data.x), Mathf.FloorToInt(data.y), Mathf.FloorToInt(data.z),
                Mathf.FloorToInt(data.w));
        }

        /// <summary>
        /// Stops moving the character.
        /// Clear the path finder nodes and set state back to Wait
        /// </summary>
        public void StopMoving() {
            IsWalking = false;
            IsMovementFromClick = false;
            Nodes?.Clear();
            meshGameEntity.SetState(GameEntityState.Wait);
            return;
        }
    }
}