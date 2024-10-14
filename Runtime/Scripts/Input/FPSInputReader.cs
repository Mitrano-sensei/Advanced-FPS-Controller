using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static FPSInputAction;

namespace FPSController
{
    [CreateAssetMenu(menuName = "FPS Controller/Input Reader")]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { }; // bool is true if the user is using the mouse, false for controller
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Run = delegate { };
        public event UnityAction<bool> Crouch = delegate { };

        FPSInputAction inputActions;

        public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>();

        void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new FPSInputAction();
                inputActions.Player.SetCallbacks(this);

            }
            inputActions.Enable();
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
    }
}
