using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Utilities;
using static FPSInputAction;

namespace FPSController
{
    [CreateAssetMenu(menuName = "FPS Controller/FPS Input Reader")]
    public class FPSInputReader : ScriptableObject, IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { }; // bool is true if the user is using the mouse, false for controller
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Run = delegate { };
        public event UnityAction<bool> Crouch = delegate { };
        public event UnityAction<bool> SpecialAction = delegate { };
        public event UnityAction<bool> Interact = delegate { };

        FPSInputAction inputActions;

        public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>().ClampMagnitude(1f);

        void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new FPSInputAction();
                inputActions.Player.SetCallbacks(this);

            }
            inputActions.Enable();

            Jump += HandleJumpInputs;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    Run.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Run.Invoke(false);
                    break;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

        public void OnCrouch(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    Crouch.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Crouch.Invoke(false);
                    break;
            }
        }

        public void OnSpecialAction(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    SpecialAction.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    SpecialAction.Invoke(false);
                    break;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    Interact.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Interact.Invoke(false);
                    break;
            }
        }


        #region Jump Inputs

        public bool JumpKeyPressed { get; private set; }
        public bool JumpKeyReleased { get; private set; }
        public bool JumpKeyHeld { get; private set; }
        public bool JumpKeyIsLocked { get; private set; }

        private void HandleJumpInputs(bool jumpKeyPressed)
        {
            if (jumpKeyPressed)
            {
                JumpKeyPressed = true;
                JumpKeyHeld = true;
            }
            else
            {
                JumpKeyReleased = true;
                JumpKeyHeld = false;
                JumpKeyIsLocked = false;
            }
        }

        public void UpdateJumpInputs()
        {
            JumpKeyPressed = false;
            JumpKeyReleased = false;
        }

        public void LockJumpKey(bool doLock = true)
        {
            JumpKeyIsLocked = doLock;
        }

        #endregion
    }
}
