using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Heimdallr.Core.Game.Controllers {
    public class GameEntityMovementController : MonoBehaviour {

        private const int MoveAnimFPS = 30 / 2;

        private LayerMask GroundMask;
        private PathFinder PathFinder;
        private MeshGameEntity meshGameEntity;
        private NetworkClient NetworkClient;

        #region Behaviour
        [SerializeField]
        private float RotateSpeed = 600f;

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
            if(meshGameEntity.HasAuthority) {
                NetworkClient.HookPacket(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            }
        }

        private void Update() {
            ProcessInput();

            if(IsWalking && Nodes.Count > 0) {
                if(NodeIndex == Nodes.Count - 1) {
                    StopMoving();
                    return;
                }

                var current = Nodes[NodeIndex];
                var next = Nodes[NodeIndex + 1];
                var direction = (next - current).normalized;

                var rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), RotateSpeed * Time.deltaTime);
                var nextPosition = (meshGameEntity.EntityData.MoveSpeed / MoveAnimFPS) * Time.deltaTime * direction;

                transform.SetPositionAndRotation(transform.position + nextPosition, rotation);

                if(transform.position.x >= next.x && transform.position.z >= next.z) {
                    NodeIndex++;
                }
            }
        }

        private void ProcessInput() {
            if(Input.GetKeyDown(KeyCode.Mouse0)) {
                IsMovementFromClick = true;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(ray, out var hit, 150, GroundMask)) {
                    RequestMovement(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z));
                }
            } else if(GetAxisDirection() != Vector3.zero) {
                // TODO: Implement free movement until we're past 1 unit over current position
                // Above comment is so we can kinda move freely within the boundaries of a cell
                var direction = GetAxisDirection();

                RequestMovement(Mathf.FloorToInt(transform.position.x + direction.x), Mathf.FloorToInt(transform.position.z + direction.z));
            } else if(GetAxisDirection() == Vector3.zero && IsWalking && !IsMovementFromClick) {
                StopMoving();
            }
        }

        private Vector3 GetAxisDirection() {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            var x = horizontal != 0 ? Mathf.Max(horizontal, 1f) * Mathf.Sign(horizontal) : 0;
            var y = vertical != 0 ? Mathf.Max(vertical, 1f) * Mathf.Sign(vertical) : 0;

            return new Vector3(x, 0, y);
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
            if(packet is ZC.NOTIFY_PLAYERMOVE pkt) {
                StartMoving(pkt.startPosition[0], pkt.startPosition[1], pkt.endPosition[0], pkt.endPosition[1]);
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
            NodeIndex = 0;
            Nodes = PathFinder
                .GetPath(startX, startY, endX, endY)
                .Select(node => new Vector3(node.x, (float) node.y, node.z))
                .ToList();

            if(Nodes.Count > 0) {
                IsWalking = true;
                meshGameEntity.SetState(GameEntityState.Walk);
            }
        }

        /// <summary>
        /// Stops moving the character.
        /// Clear the path finder nodes and set state back to Wait
        /// </summary>
        public void StopMoving() {
            IsWalking = false;
            IsMovementFromClick = false;
            Nodes.Clear();
            meshGameEntity.SetState(GameEntityState.Wait);
            return;
        }
    }
}
