using FiniteStateMachine;
using System;
using UnityEngine;
using Utilities;
using static FPSController.ClimbMovement;

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
        [SerializeField] private float timeToChangeSpeedInSeconds = 1f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float coyoteeTime = .15f;

        private bool _jumpKeyPressed;  // True the frame the jump key is pressed
        private bool _jumpKeyHeld;     // True while the jump key is held
        private bool _jumpKeyReleased; // True the frame the jump key is released
        private bool _jumpKeyIsLocked; // To prevent multiple jumps same frame

        public bool JumpKeyPressed { get => _jumpKeyPressed; private set => _jumpKeyPressed = value; }
        public bool JumpKeyHeld { get => _jumpKeyHeld; private set => _jumpKeyHeld = value; }
        public bool JumpKeyReleased { get => _jumpKeyReleased; private set => _jumpKeyReleased = value; }

        [Header("Crouching & Sliding")]
        [SerializeField] private float slideBoost = 3f;
        [SerializeField] private float slideDragRatio = .2f;
        [SerializeField] private float minimumSlideVelocity = 1f;

        [Header("Climb")]
        [SerializeField] private ClimbMovement climbMovement;

        public Vector3 CurrentSlopeNormal => _currentSlopeNormal;
        public IState CurrentState => _stateMachine.CurrentState;

        internal void SetMaxSpeed(float v) => _currentMaxSpeed = v;


        private bool _isCrouchingKeyPressed;
        private bool _isCrouchingKeyHeld;
        private bool _isCrouchingKeyReleased;

        private bool _isExitingCrouch;

        private StateMachine _stateMachine;
        private Vector2 _moveInput;
        private Vector3 _currentSlopeNormal;

        internal float _currentMaxSpeed;
        internal float _speedWhenChanged;
        internal StopwatchTimer _changeStateTimer = new();

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

            climbMovement.WallCheck();

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
        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine();

            var groundedState = new GroundedState(this);
            var jumpingState = new JumpingState(this);
            var fallingState = new FallingState(this);
            var risingState = new RisingState(this);
            var crouchingState = new CrouchingState(this);
            var slidingState = new SlidingState(this);
            var climbingState = new ClimbingState(this, climbMovement);

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

            At(groundedState, crouchingState, () => IsCrouching() && _rb.velocity.magnitude < minimumSlideVelocity);
            At(crouchingState, groundedState, () => _isCrouchingKeyReleased);
            At(crouchingState, fallingState, () => !playerMover.IsGrounded() && _rb.velocity.y <= 0f);
            At(crouchingState, risingState, () => !playerMover.IsGrounded() && _rb.velocity.y > 0f);
            At(crouchingState, jumpingState, IsEnteringJump);

            At(jumpingState, climbingState, climbMovement.IsClimbingEnter);
            At(risingState, climbingState, climbMovement.IsClimbingEnter);
            At(fallingState, climbingState, climbMovement.IsClimbingEnter);
            At(climbingState, fallingState, climbMovement.IsClimbingExit);
            
            _stateMachine.SetState(fallingState);
        }

        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, new FuncPredicate(condition));

        #endregion

        #region Ground

        internal void OnGroundEnter()
        {
            playerMover.UseExtendedCastLength = true;
            _isExitingCrouch = false;
            SetupLerpToDefaultMoveSpeed();
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

            SetupLerpToDefaultMoveSpeed();
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

            SetMaxSpeed(GetFlatVelocity(_rb.velocity).magnitude);
        }

        internal void OnSlideExit()
        {
            playerMover.SetIsCrouching(false);
            _isExitingCrouch = true;

            _coyoteeTimer.Start();
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
        public float GetCurrentMaxSpeed() => _currentMaxSpeed;

        internal void CalculateVelocity(float ratio = 1f)
        {
            var movementInput = CalculateMovementVelocity();
            Vector3 velocity = movementInput * ratio;
            _rb.AddForce(velocity, ForceMode.Impulse);

            _movementInputLastFrame = movementInput;
            _velocityLastFrame = velocity;
        }

        private void HandleDrag()
        {
            if (!playerMover.IsGrounded())
            {
                _rb.drag = 0;
                return;
            }

            if (CurrentState is SlidingState)
                _rb.drag = slideDragRatio;
            else
                _rb.drag = groundDrag;
        }

        private void HandleSpeedLimit()
        {
            var state = (CurrentState as IFPSState);

            if (state == null) Debug.LogError("Current State " + CurrentState.Name + " is not an IFPSState");
            if (!state.IsLimitedSpeed())
                return;

            Vector3 velocity = _rb.velocity;
            Vector3 flatVelocity = GetFlatVelocity(velocity);
            float verticalVelocity = Vector3.Dot(velocity, _currentSlopeNormal.normalized);

            if (flatVelocity.magnitude > _currentMaxSpeed)
            {
                var currentSpeed = Mathf.Lerp(_speedWhenChanged, _currentMaxSpeed, _changeStateTimer.GetCurrentTime() / (timeToChangeSpeedInSeconds * (_moveInput.magnitude + .05f)));
                if (Mathf.Abs(currentSpeed - _currentMaxSpeed) < .05f)
                    currentSpeed = _currentMaxSpeed;

                _rb.ApplyVelocity(flatVelocity.normalized * currentSpeed + verticalVelocity * _currentSlopeNormal.normalized);
            }
        }

        /**
         * Gets Velocity, not taking account vertical velocity, and taking slopes into accounts
         */
        private Vector3 GetFlatVelocity(Vector3 velocity)
        {
            Vector3 flatVelocity = velocity.RemoveDotVector(_currentSlopeNormal);

            return flatVelocity;
        }

        private void AdjustToGround()
        {
            if (!playerMover.IsGrounded())
                return;

            _rb.ApplyVelocity(_rb.velocity + CalculateGroundAdjustmentVelocity());
        }

        /**
         * When in the air, the player has less control over its movement.
         */
        private float GetMovementSpeedFromState()
        {
            var currentFpsState = CurrentState as IFPSState;

            if (currentFpsState == null)
            {
                Debug.LogError("Current State is not an IFPSState");
                return 1f;
            }

            return currentFpsState.GetMovementSpeedRatio();
        }

        private Vector3 CalculateGroundAdjustmentVelocity()
        {
            if (CurrentState is not GroundedState)
            {
                return Vector3.zero;
            }

            return playerMover.GetGroundAdjustmentVelocity();
        }

        public Vector3 CalculateMovementVelocity()
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
            if (playerMover.IsGrounded())
                return;

            _rb.AddForce(Vector3.down * gravityScale, ForceMode.Impulse);
        }

        internal void SetupLerpToDefaultMoveSpeed()
        {
            // Handle Speed On Change State
            _changeStateTimer.Reset();
            _changeStateTimer.Start();
            _speedWhenChanged = GetFlatVelocity(_rb.velocity).magnitude;

            _currentMaxSpeed = movementSpeed;
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
            _changeStateTimer.Tick(Time.deltaTime);
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
