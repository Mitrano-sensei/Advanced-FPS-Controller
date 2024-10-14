using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class CrouchMovement : MonoBehaviour
    {
        #region Fields

        private Rigidbody _rb;
        private PlayerController _playerController;
        private PlayerBody _playerBody;
        #endregion    
    
        #region MonoBehaviour
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerController = GetComponent<PlayerController>();
            _playerBody = GetComponent<PlayerBody>();
        }

        #endregion

        internal bool IsCrouching()
        {
            return _playerController.IsCrouchingKeyPressed || _playerController.IsCrouchingKeyHeld;
        }

        internal void OnCrouchEnter()
        {
            _playerBody.SetIsCrouching(true);
            _rb.AddForce(-Vector3.up * 5f, ForceMode.Impulse);

            _playerController.SetupLerpToDefaultMoveSpeed();
        }

        internal void OnCrouchExit()
        {
            _playerBody.SetIsCrouching(false);
            _playerController.IsExitingCrouch = true;
        }

        public class CrouchingState : IState, IFPSState
        {
            public string Name => "Crouching State";
            private PlayerController _playerController;
            private CrouchMovement _crouchMovement;

            public CrouchingState(PlayerController playerController, CrouchMovement crouchMovement)
            {
                _playerController = playerController;
                _crouchMovement = crouchMovement;
            }

            public float GetMovementSpeedRatio() => .5f;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());
            }

            public void OnEnter()
            {
                _crouchMovement.OnCrouchEnter();
            }

            public void OnExit()
            {
                _crouchMovement.OnCrouchExit();
            }

            public void Update()
            {
            }

        }
    }
}
