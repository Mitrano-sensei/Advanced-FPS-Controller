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
        [SerializeField] private float groundDrag = 6f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float coyoteeTime = .15f;

        private bool _jumpKeyPressed;  // True the frame the jump key is pressed
        private bool _jumpKeyHeld;     // True while the jump key is held
        private bool _jumpKeyReleased; // True the frame the jump key is released
        private bool _jumpKeyIsLocked; // To prevent multiple jumps same frame

        [Header("Sliding")]
        [SerializeField] private float slideBoost = 3f;
        [SerializeField] private float crouchSpeedRatio = .7f;
        [SerializeField] private float slideDragRatio = .2f;
        [SerializeField] private float minimumSlideVelocity = 1f;

        private bool _isCrouchingKeyPressed;
        private bool _isCrouchingKeyHeld;
        private bool _isCrouchingKeyReleased;

        private bool _isExitingCrouch;


        private StateMachine _stateMachine;
        private Vector2 _moveInput;
        private Vector3 _currentSlopeNormal;

        private Vector3 _movementInputLastFrame;
        private Vector3 _velocityLastFrame;

        [Header("Debug")]
        [SerializeField] private bool debugMovement = true;

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

            // Debug
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("Drag : " + _rb.drag);
            }
        }

        void FixedUpdate()
        {
            _stateMachine.FixedUpdate();

            CalculateSlope();

            HandleGravity();
            AdjustToGround();

            CalculateVelocity();
            HandleSpeedLimit();
            HandleDrag();

            ResetJumpKeys();
            ResetCrouchKeys();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _coyoteeTimer != null && coyoteeTime != _coyoteeTimer.GetInitialTime())
                UpdateTimers();
        }

        private void OnDrawGizmos()
        {
            if (debugMovement)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, _currentSlopeNormal * 3f);

                var forward = Vector3.ProjectOnPlane(orientation.forward, _currentSlopeNormal).normalized;
                var right = Vector3.ProjectOnPlane(orientation.right, _currentSlopeNormal).normalized;

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, forward * 3f);
                Gizmos.DrawRay(transform.position, right * 3f);

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + Vector3.up, forward * _moveInput.y + right * _moveInput.x);
            }
        }

        #endregion

        #region StateMachine
        private IState CurrentState => _stateMachine.CurrentState;
        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine();

            var groundedState = new GroundedState(this);
            var jumpingState = new JumpingState(this);
            var fallingState = new FallingState(this);
            var risingState = new RisingState(this);
            var crouchingState = new CrouchingState(this);
            var slidingState = new SlidingState(this);

            At(fallingState, groundedState, () => playerMover.IsGrounded());
            At(groundedState, fallingState, () => !playerMover.IsGrounded() && _rb.velocity.y <= 0f);
            At(groundedState, risingState, () => !playerMover.IsGrounded() && _rb.velocity.y > 0f);
            At(risingState, fallingState, () => !playerMover.IsGrounded() && _rb.velocity.y <= 0f);
            At(risingState, groundedState, () => playerMover.IsGrounded());

            At(groundedState, jumpingState, IsEnteringJump);
            At(fallingState, jumpingState, IsEnteringJump);
            At(jumpingState, fallingState, () => _rb.velocity.y < 0f);
            At(jumpingState, risingState, () => _rb.velocity.y > 0f && _jumpKeyReleased);

            At(groundedState, slidingState, () => IsSliding());
            At(slidingState, crouchingState, () => !IsSliding() && IsCrouching());
            At(slidingState, groundedState, () => !IsSliding() && playerMover.IsGrounded() && !IsCrouching());
            At(slidingState, jumpingState, IsEnteringJump);
            At(slidingState, fallingState, () => !playerMover.IsGrounded() && _rb.velocity.y <= 0f);
            At(slidingState, risingState, () => !playerMover.IsGrounded() && _rb.velocity.y > 0f);

            At(groundedState, crouchingState, IsCrouching);
            At(crouchingState, groundedState, () => _isCrouchingKeyReleased);
            At(crouchingState, fallingState, () => !playerMover.IsGrounded() && _rb.velocity.y <= 0f);
            At(crouchingState, risingState, () => !playerMover.IsGrounded() && _rb.velocity.y > 0f);
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

            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        internal void OnJumpExit()
        {
            if (_rb.velocity.y > 0f)
                _rb.ApplyVerticalVelocity(_rb.velocity.y * .5f);

            _isExitingCrouch = false;
        }

        private bool IsEnteringJump()
        {
            bool defaultJump = _jumpKeyPressed && !_jumpKeyIsLocked && playerMover.IsGrounded();
            bool coyoteeJump = _coyoteeTimer.IsRunning && _jumpKeyPressed && !_jumpKeyIsLocked && CurrentState is FallingState;

            return (defaultJump || coyoteeJump) && !_isExitingCrouch;
        }

        #endregion

        #region Crouch & Slide        

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
            playerMover.SetIsCrouching(true);
            _rb.AddForce(-Vector3.up * 5f, ForceMode.Impulse);
        }

        internal void OnCrouchExit()
        {
            playerMover.SetIsCrouching(false);
            _isExitingCrouch = true;
        }

        internal void OnSlideEnter()
        {
            var forward = CalculateMovementVelocity().normalized;
            _rb.AddForce(forward * slideBoost, ForceMode.Impulse);
            
            playerMover.SetIsCrouching(true);
            _rb.AddForce(-Vector3.up * 5f, ForceMode.Impulse);
        }

        internal void OnSlideExit()
        {
            playerMover.SetIsCrouching(false);
            _isExitingCrouch = true;
        }

        internal void OnSlideFixedUpdate()
        {
            SlideOnSlope();
        }

        private void SlideOnSlope()
        {
            if (_currentSlopeNormal == Vector3.up)
                return;

            var horizontalDirection = Vector3.Cross(_currentSlopeNormal, Vector3.up);       // "Right" direction on the slope, horizontal part of the slope
            var slopeDirection = Vector3.Cross(_currentSlopeNormal, horizontalDirection);   // Direction of the slope (going down)

            var slopeAngle = Vector3.Angle(Vector3.up, _currentSlopeNormal);
            var speedRatio = slopeAngle / 20f;

            _rb.AddForce(slopeDirection * movementSpeed * speedRatio, ForceMode.Impulse);
        }

        private bool IsSliding()
        {
            return (_isCrouchingKeyPressed || _isCrouchingKeyHeld) && (playerMover.IsGrounded() && (_rb.velocity.magnitude > minimumSlideVelocity));
        }


        #endregion

        #region Movement Control
        private void CalculateVelocity()
        {
            if (CurrentState is SlidingState)
                return;

            var movementInput = CalculateMovementVelocity();
            Vector3 velocity = movementInput * GetMovementControlFromState(); // If is not grounded, will be handled by momentum
            _rb.AddForce(velocity, ForceMode.Impulse);

            _movementInputLastFrame = movementInput;
            _velocityLastFrame = velocity;
        }

        private void HandleDrag()
        {
            switch (CurrentState)
            {
                case GroundedState:
                    _rb.drag = groundDrag;
                    break;
                case CrouchingState:
                    _rb.drag = groundDrag;
                    break;
                case SlidingState:
                    _rb.drag = groundDrag * slideDragRatio;
                    break;
                default:
                    _rb.drag = 0f;
                    break;
            }
        }

        private void HandleSpeedLimit()
        {
            if (CurrentState is SlidingState)
                return;

            Vector3 velocity = _rb.velocity;
            Vector3 flatVelocity = velocity.RemoveDotVector(_currentSlopeNormal);
            float verticalVelocity = Vector3.Dot(velocity, _currentSlopeNormal.normalized);

            var maxSpeed = movementSpeed * (CurrentState is CrouchingState ? crouchSpeedRatio : 1f);

            if (flatVelocity.magnitude > maxSpeed) // FIXME : _rb.velocity should be replaced by "flat velocity" accounting slopes
            {
                _rb.ApplyVelocity(flatVelocity.normalized * maxSpeed + verticalVelocity * _currentSlopeNormal.normalized);
            }
        }

        private void AdjustToGround()
        {
            if (CurrentState is not (GroundedState or CrouchingState or SlidingState))
                return;

            _rb.ApplyVelocity(_rb.velocity + CalculateGroundAdjustmentVelocity());
        }

        /**
         * When in the air, the player has less control over its movement.
         */
        private float GetMovementControlFromState()
        {
            var currentFpsState = CurrentState as IFPSState;

            if (currentFpsState == null)
            {
                Debug.LogError("Current State is not an IFPSState");
                return 1f;
            }

            return currentFpsState.GetAirControlRatio();
        }

        private Vector3 CalculateGroundAdjustmentVelocity()
        {
            if (CurrentState is not GroundedState)
            {
                return Vector3.zero;
            }

            return playerMover.GetGroundAdjustmentVelocity();
        }

        private Vector3 CalculateMovementVelocity()
        {
            var forward = Vector3.ProjectOnPlane(orientation.forward, _currentSlopeNormal).normalized;
            var right = Vector3.ProjectOnPlane(orientation.right, _currentSlopeNormal).normalized;

            return forward * _moveInput.y * movementSpeed + right * _moveInput.x * movementSpeed;
        }

        private void CalculateSlope()
        {
            _currentSlopeNormal = playerMover.GetSlopeNormal();
        }

        private void HandleGravity()
        {
            var crouchingState = (CurrentState is CrouchingState && !_isCrouchingKeyPressed); // To let gravity act on the player for the first frame after crouching, to avoid falling
            if (CurrentState is GroundedState || crouchingState)
                return;

            _rb.AddForce(Vector3.down * gravityScale, ForceMode.Impulse);
        }
        #endregion

        #region Setup
        private void SetupRigidbody()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.freezeRotation = true;
        }

        private void TickTimers()
        {
            _coyoteeTimer.Tick(Time.deltaTime);
        }

        private void UpdateTimers()
        {
            _coyoteeTimer = new CountdownTimer(coyoteeTime);
        }
        #endregion
        
        #region Debug
        public void PrintState()
        {
            Debug.Log("Current State : " + CurrentState.Name);
        }
        #endregion
    }
}
