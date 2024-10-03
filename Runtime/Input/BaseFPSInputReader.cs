using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static BaseFPSInputActions;

namespace FPSController
{
    public class BaseFPSInputReader : ScriptableObject, IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { }; // bool is true if the user is using the mouse, false for controller
        public event UnityAction Jump = delegate { };

        BaseFPSInputActions inputActions;

        public Vector3 Direction => inputActions.Player.Move.ReadValue<Vector2>();

        void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new BaseFPSInputActions();
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
            Jump.Invoke();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";


    }
}
