using FiniteStateMachine;
using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [Header("Controls")]
        [SerializeField] private InputReader inputReader;

        [Header("Movements")]
        [SerializeField] private PlayerMover playerMover;
        [SerializeField] private Transform orientation;
        [SerializeField] private float gravityScale = 1f;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float groundDragRatio = 1f;
        [SerializeField] private float airDragRatio = .9f;

        private StateMachine _stateMachine;
        private Vector2 _moveInput;

        private Vector3 _momentum = Vector3.zero;

        private Vector3 _movementInputLastFrame;
        private Vector3 _velocityLastFrame;

        // Debug
        private Rigidbody _rb;

        #endregion

        #region MonoBehaviour
        void Start()
        {
            SetupStateMachine();

            SetupRigidbody();

            UpdateTimers();

            inputReader.Move += input => _moveInput = input.magnitude > 1f ? input.normalized : input;
            inputReader.Jump += b => HandleJumpKeyInput(b);
            inputReader.Crouch += b => HandleCrouchKeyInput(b);
        }

        void Update()
        {
            _stateMachine.Update();
            TickTimers();
        }

        void FixedUpdate()
        {
            _stateMachine.FixedUpdate();

            HandleMomentum();

            CalculateVelocity();

            ResetJumpKeys();
            ResetCrouchKeys();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _coyoteeTimer != null && coyoteeTime != _coyoteeTimer.GetInitialTime())
                UpdateTimers();
        }

        #endregion

        #region StateMachine

        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine();

            var groundedState = new GroundedState(this);
            var jumpingState = new JumpingState(this);
            var fallingState = new FallingState(this);
            var risingState = new RisingState(this);
            var crouchingState = new CrouchingState(this);

            At(fallingState, groundedState, () => playerMover.IsGrounded());
            At(groundedState, fallingState, () => !playerMover.IsGrounded() && _momentum.y <= 0f);
            At(groundedState, risingState, () => !playerMover.IsGrounded() && _momentum.y > 0f);
            At(risingState, fallingState, () => !playerMover.IsGrounded() && _momentum.y <= 0f);
            At(risingState, groundedState, () => playerMover.IsGrounded());

            At(groundedState, jumpingState, IsEnteringJump);
            At(fallingState, jumpingState, IsEnteringJump);
            At(jumpingState, fallingState, () => _rb.velocity.y < 0f);
            At(jumpingState, risingState, () => _rb.velocity.y > 0f && _jumpKeyReleased);

            At(groundedState, crouchingState, IsCrouching);
            At(crouchingState, groundedState, () => _isCrouchingKeyReleased);
            At(crouchingState, fallingState, () => !playerMover.IsGrounded() && _momentum.y <= 0f);
            At(crouchingState, risingState, () => !playerMover.IsGrounded() && _momentum.y > 0f);
            At(crouchingState, jumpingState, IsEnteringJump);
        
            _stateMachine.SetState(fallingState);
        }

        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, new FuncPredicate(condition));

        #endregion

        #region Ground

        internal void OnGroundEnter()
        {
            playerMover.UseExtendedCastLength = true;
            _isExitingCrouch = false;
        }

        internal void OnGroundExit()
        {
            _coyoteeTimer.Start();
            playerMover.UseExtendedCastLength = false;
        }
        #endregion

        #region Jump

        [Header("Jump")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float coyoteeTime = .15f;

        private bool _jumpKeyPressed;  // True the frame the jump key is pressed
        private bool _jumpKeyHeld;     // True while the jump key is held
        private bool _jumpKeyReleased; // True the frame the jump key is released
        private bool _jumpKeyIsLocked; // To prevent multiple jumps same frame

        private CountdownTimer _coyoteeTimer;

        private void HandleJumpKeyInput(bool isJumpKeyPressed)
        {
            if (isJumpKeyPressed)
            {
                _jumpKeyPressed = true;
                _jumpKeyHeld = true;
            }
            else
            {
                _jumpKeyReleased = true;
                _jumpKeyHeld = false;
            }
        }

        /**
         * Called at the end of the frame, to reset jump key inputs.
         */
        private void ResetJumpKeys()
        {
            _jumpKeyPressed = false;
            _jumpKeyReleased = false;
            _jumpKeyIsLocked = false;
        }

        internal void OnJumpEnter()
        {
            if (_jumpKeyIsLocked)
                return;

            _jumpKeyIsLocked = true;

            _momentum.y += jumpForce;
        }

        internal void OnJumpExit()
        {
            if (_rb.velocity.y > 0f)
                _momentum -= Vector3.up * _rb.velocity.y * .5f;

            _isExitingCrouch = false;
        }

        private bool IsEnteringJump()
        {
            bool defaultJump = _jumpKeyPressed && !_jumpKeyIsLocked && playerMover.IsGrounded();
            bool coyoteeJump = _coyoteeTimer.IsRunning && _jumpKeyPressed && !_jumpKeyIsLocked && _stateMachine.CurrentState is FallingState;

            return (defaultJump || coyoteeJump) && !_isExitingCrouch;
        }

        #endregion

        #region Crouch & Slide
        private bool _isCrouchingKeyPressed;
        private bool _isCrouchingKeyHeld;
        private bool _isCrouchingKeyReleased;

        private bool _isExitingCrouch;

        private void HandleCrouchKeyInput(bool isCrouchKeyPressed)
        {
            if (isCrouchKeyPressed)
            {
                _isCrouchingKeyPressed = true;
                _isCrouchingKeyHeld = true;
            }
            else
            {
                _isCrouchingKeyReleased = true;
                _isCrouchingKeyHeld = false;
            }
        }

        private void ResetCrouchKeys()
        {
            _isCrouchingKeyPressed = false;
            _isCrouchingKeyReleased = false;
        }

        private bool IsCrouching()
        {
            return _isCrouchingKeyPressed || _isCrouchingKeyHeld;
        }

        internal void OnCrouchEnter()
        {
            _momentum.y -= 3f * jumpForce;
            playerMover.SetIsCrouching(true);
        }

        internal void OnCrouchExit()
        {
            playerMover.SetIsCrouching(false);
            _isExitingCrouch = true;
        }

        #endregion
        private void SetupRigidbody()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.freezeRotation = true;
        }

        private void HandleMomentum()
        {
            float verticalMomentum = _momentum.y;
            Vector2 horizontalMomentum = _momentum.WithY(0);

            HandleGravity(ref verticalMomentum);

            _momentum = new Vector3(horizontalMomentum.x, verticalMomentum, horizontalMomentum.y);
        }

        private void CalculateVelocity()
        {
            var movementInput = CalculateMovementVelocity();
            Vector3 velocity = movementInput * GetDragFromState(); // If is not grounded, will be handled by momentum

            velocity += _momentum;

            _movementInputLastFrame = movementInput;
            _velocityLastFrame = velocity;

            playerMover.SetVelocity(velocity + CalculateGroundAdjustmentVelocity());
        }

        private float GetDragFromState()
        {
            switch (_stateMachine.CurrentState)
            {
                case GroundedState:
                    return groundDragRatio;
                case RisingState:
                    return airDragRatio;
                case FallingState:
                    return airDragRatio;
                case JumpingState:
                    return airDragRatio;
                case CrouchingState:
                    return groundDragRatio;
                default:
                    Debug.LogError("Unknown state");
                    return 1f;

            }
        }

        private Vector3 CalculateGroundAdjustmentVelocity()
        {
            if (_stateMachine.CurrentState is not GroundedState)
            {
                return Vector3.zero;
            }

            return playerMover.GetGroundAdjustmentVelocity();
        }

        private Vector3 CalculateMovementVelocity()
        {
            Vector3 slopeNormal = Vector3.zero;
            if (playerMover.CheckSlope(ref slopeNormal))
            {
                var forward = Vector3.ProjectOnPlane(orientation.forward, slopeNormal).normalized;
                var right = Vector3.ProjectOnPlane(orientation.right, slopeNormal).normalized;

                return forward * _moveInput.y * movementSpeed + right * _moveInput.x * movementSpeed;
            }

            Vector3 velocity = orientation.forward * _moveInput.y * movementSpeed + orientation.right * _moveInput.x * movementSpeed;

            return velocity;
        }

        private void HandleGravity(ref float verticalVelocity)
        {
            var crouchingState = (_stateMachine.CurrentState is CrouchingState && !_isCrouchingKeyPressed); // To let gravity act on the player for the first frame after crouching, to avoid falling
            if (_stateMachine.CurrentState is GroundedState || crouchingState)
            {
                verticalVelocity = 0f;
                return;
            }

            verticalVelocity += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
        }

        private void TickTimers()
        {
            _coyoteeTimer.Tick(Time.deltaTime);
        }

        private void UpdateTimers()
        {
            _coyoteeTimer = new CountdownTimer(coyoteeTime);
        }

        #region Debug
        public void PrintState()
        {
            Debug.Log("Current State : " + _stateMachine.CurrentState.Name);
        }

        #endregion
    }
}
