using FiniteStateMachine;
using UnityEngine;
using Utilities;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class JumpMovement : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float jumpForce = 20f;

        private Rigidbody _rb;
        private PlayerController _playerController;
        private PlayerBody _playerBody;
        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerController = GetComponent<PlayerController>();
            _playerBody = GetComponent<PlayerBody>();
        }

        #endregion

        internal void OnJumpEnter()
        {
            if (_playerController.JumpKeyIsLocked)
                return;

            _playerController.JumpKeyIsLocked = true;

            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        internal void OnJumpExit()
        {
            if (_rb.velocity.y > 0f)
                _rb.ApplyVerticalVelocity(_rb.velocity.y * .5f);

            _playerController.IsExitingCrouch = false;
        }

        internal bool IsEnteringJump()
        {
            bool defaultJump = _playerController.JumpKeyPressed && !_playerController.JumpKeyIsLocked && _playerBody.IsGrounded();
            bool coyoteeJump = _playerController.IsCoyoteeJumpAllowed();

            return (defaultJump || coyoteeJump) && !_playerController.IsExitingCrouch;
        }


        public class JumpingState : IState, IFPSState
        {
            public string Name => "Jumping State";
            private PlayerController _playerController;
            private JumpMovement _jumpMovement;

            public JumpingState(PlayerController playerController, JumpMovement jumpMovement)
            {
                _playerController = playerController;
                _jumpMovement = jumpMovement;
            }

            public float GetMovementSpeedRatio() => .8f;


            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());
            }

            public void OnEnter()
            {
                _jumpMovement.OnJumpEnter();
            }

            public void OnExit()
            {
                _jumpMovement.OnJumpExit();
            }

            public void Update()
            {
            }
        }

        public class FallingState : IState, IFPSState
        {
            public string Name => "Falling State";
            private PlayerController _playerController;

            public FallingState(PlayerController playerController)
            {
                _playerController = playerController;
            }

            public float GetMovementSpeedRatio() => .8f;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());
            }

            public void OnEnter()
            {
            }

            public void OnExit()
            {
            }

            public void Update()
            {
            }
        }

        public class RisingState : IState, IFPSState
        {
            public string Name => "Rising State";
            private PlayerController _playerController;

            public RisingState(PlayerController playerController)
            {
                _playerController = playerController;
            }
            public float GetMovementSpeedRatio() => .8f;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());
            }

            public void OnEnter()
            {
            }

            public void OnExit()
            {
            }

            public void Update()
            {
            }
        }
    }
}
