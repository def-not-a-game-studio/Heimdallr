using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityRO.Core.Camera;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;

namespace Core.Input {
    public class PlayerInputController : MonoBehaviour {
        [SerializeField] private CoreGameEntity Entity;

        private PlayerInputActions InputActions;
        private Camera MainCamera;
        private CharacterCamera CharacterCamera;

        private LayerMask EntityMask;
        private LayerMask GroundMask;

        public PlayerInputActions.UIActions UIActions { get; private set; }
        public PlayerInputActions.PlayerActions PlayerActions { get; private set; }
        
        private void OnEnable() {
            InputActions = new PlayerInputActions();
            InputActions.Player.Enable();
            MainCamera = Camera.main;
            CharacterCamera = FindObjectOfType<CharacterCamera>();

            EntityMask = LayerMask.GetMask("NPC", "Monster", "Item");
            GroundMask = LayerMask.GetMask("Ground");

            UIActions = InputActions.UI;
            PlayerActions = InputActions.Player;
            
            InputActions.Player.RightClick.canceled += OnRightClick;
        }

        private void OnDisable() {
            InputActions.Player.Disable();
        }

        private void Update() {
            CheckCameraZoom();
            CheckRightClick();
            CheckPlayerMove();
            CheckPlayerAction();
        }

        private void CheckPlayerMove() {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            var value = InputActions.Player.Move.ReadValue<Vector2>();
            if (value == Vector2.zero) return;
            Entity.RequestOffsetMovement(value);
        }

        private void CheckPlayerAction() {
            if (EventSystem.current.IsPointerOverGameObject() || !PlayerActions.SelectConfirm.WasPerformedThisFrame()) return;
            var mousePosition = Mouse.current.position.ReadValue();

            var ray = MainCamera.ScreenPointToRay(mousePosition);

            var didHitGround = Physics.Raycast(ray, out var groundHit, 300, GroundMask);
            var didHitAnything = Physics.Raycast(ray, out var entityHit, 300, EntityMask);

            if (didHitAnything) {
                var target = entityHit.collider.gameObject;
                var isTargetEntity = target.TryGetComponent<SpriteViewer>(out var targetEntity);

                if (target.layer == LayerMask.NameToLayer("Monster") && isTargetEntity) {
                    Entity.RequestAction(targetEntity.Entity);
                } else if (target.layer == LayerMask.NameToLayer("NPC")&& isTargetEntity) {
                    Entity.TalkToNpc(targetEntity.Entity);
                } else if (target.layer == LayerMask.NameToLayer("Item")) {
                    // TODO
                }
            } else if (didHitGround) {
                Entity.RequestMovement(new Vector2(Mathf.FloorToInt(groundHit.point.x), Mathf.FloorToInt(groundHit.point.z)));
            }
        }

        private void CheckCameraZoom() {
            CharacterCamera.OnZoom(Mouse.current.scroll.y.ReadValue(), InputActions.Player.Shift.IsPressed());
        }

        private void CheckRightClick() {
            var action = InputActions.Player.RightClick;

            if (action.IsPressed()) {
                var mousePosition = Mouse.current.delta.ReadValue();
                CharacterCamera.OnRotatePressed(mousePosition, Mouse.current.scroll.y.ReadValue(),
                                                InputActions.Player.Shift.IsPressed());
            } else if (action.WasPerformedThisFrame()) {
                CharacterCamera.OnRotateReleased(InputActions.Player.Shift.IsPressed());
            }
        }

        private void OnRightClick(InputAction.CallbackContext context) {
            if (context is { canceled: true, interaction: HoldInteraction }) {
                CharacterCamera.OnRotateReleased(InputActions.Player.Shift.IsPressed());
            }
        }
    }
}