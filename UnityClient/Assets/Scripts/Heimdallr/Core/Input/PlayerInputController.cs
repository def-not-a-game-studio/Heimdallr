using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityRO.Core.Camera;
using UnityRO.Core.GameEntity;

namespace Core.Input {
    public class PlayerInputController : MonoBehaviour {
        [SerializeField] private CoreGameEntity Entity;

        private PlayerInputActions InputActions;
        private Camera MainCamera;
        private CharacterCamera CharacterCamera;

        private LayerMask EntityMask;
        private LayerMask GroundMask;

        private void OnEnable() {
            InputActions = new PlayerInputActions();
            InputActions.Player.Enable();
            MainCamera = Camera.main;
            CharacterCamera = FindObjectOfType<CharacterCamera>();

            EntityMask = LayerMask.GetMask("NPC", "Monster", "Item");
            GroundMask = LayerMask.GetMask("Ground");

            InputActions.Player.SelectConfirm.performed += OnSelectConfirm;
            InputActions.Player.RightClick.canceled += OnRightClick;
        }

        private void OnDisable() {
            InputActions.Player.Disable();
        }

        private void FixedUpdate() {
            CheckCameraZoom();
            CheckRightClick();
            CheckPlayerMove();
        }

        private void CheckPlayerMove() {
            var value = InputActions.Player.Move.ReadValue<Vector2>();
            if (value == Vector2.zero) return;
            Entity.RequestOffsetMovement(value);
        }

        private void OnSelectConfirm(InputAction.CallbackContext context) {
            var mousePosition = Mouse.current.position.ReadValue();
            var ray = MainCamera.ScreenPointToRay(mousePosition);

            var didHitAnything = Physics.Raycast(ray, out var hit, 300, GroundMask | EntityMask);

            if (!didHitAnything) return;

            var target = hit.collider.gameObject;
            
            if (target.layer == LayerMask.NameToLayer("Ground")) {
                Entity.RequestMovement(new Vector2(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z)));
            } else if (target.layer == LayerMask.NameToLayer("NPC")) {
                // TODO
            } else if (target.layer == LayerMask.NameToLayer("Monster")) {
                // TODO
            } else if (target.layer == LayerMask.NameToLayer("Item")) {
                // TODO
            }
        }

        private void CheckCameraZoom() {
            CharacterCamera.OnZoom(Mouse.current.scroll.y.ReadValue(), InputActions.Player.Shift.IsPressed());
        }
        
        private void CheckRightClick() {
            var action = InputActions.Player.RightClick;
                                     
            if (action.IsPressed()) {
                var mousePosition = Mouse.current.delta.ReadValue();
                CharacterCamera.OnRotatePressed(mousePosition, Mouse.current.scroll.y.ReadValue(), InputActions.Player.Shift.IsPressed());    
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