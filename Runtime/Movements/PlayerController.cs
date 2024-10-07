using FiniteStateMachine;
using System;
using UnityEngine;
using Utilities;
using StateMachine = FiniteStateMachine.StateMachine;

namespace FPSController
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private InputReader inputReader;

        [Header("Movement")]
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private Transform orientation;
        [SerializeField] private float groundDrag = 5f;
        [SerializeField] private float gravityMultiplier = 2f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpCooldown = .25f;
        [SerializeField] private float airMultiplier;

        [SerializeField] private float coyoteeTime = .2f;

        CountdownTimer _jumpTimer;

        [Header("Ground Sensor")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundSensorLength = 1.1f;
        [SerializeField] private Transform origin;

        float _horizontalInput, _verticalInput;
        Vector3 _moveDirection;

        Rigidbody _rb;

        StopwatchTimer _groundedTimer;
        SimpleRaycastSensor _groundSensor;
        bool _isGrounded;

        private StateMachine _stateMachine;

        #endregion

        #region MonoBehaviour 
        void Start()
        {
            // Rigidbody
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;


            // Inputs
            inputReader.Move += HandleMovementInput;
            inputReader.Jump += HandleJumpKeyInput;

            // Initialization
            CalibrateRaycastSensor();
            InitializeTimers();
            SetupStateMachine();
        }

        private void Update()
        {
            UpdateTimers();

            _stateMachine.Update();
        }

        void FixedUpdate()
        {
            _stateMachine.FixedUpdate();

            HandleGravity();

            ResetJumpKeys();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (_jumpTimer != null && jumpCooldown != _jumpTimer.GetTime())
            {
                _jumpTimer = new CountdownTimer(jumpCooldown);
            }

            if (_groundSensor != null && _groundSensor.castLength != groundSensorLength)
            {
                CalibrateRaycastSensor();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundSensor == null) return;
            _groundSensor.DrawDebug();
        }
        #endregion


        #region Private Methods
        private void InitializeTimers()
        {
            _jumpTimer = new CountdownTimer(jumpCooldown);
            _groundedTimer = new StopwatchTimer();
        }

        private void UpdateTimers()
        {
            _jumpTimer.Tick(Time.deltaTime);
            _groundedTimer.Tick(Time.deltaTime);
        }

        #region StateMachine
        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine();

            var groundState = new GroundState(this);
            var airState = new AirState(this);
            var jumpState = new JumpingState(this);

            At(airState, groundState, () => _isGrounded);
            At(airState, jumpState, isCoyoteeJumping);
            At(groundState, airState, () => !_isGrounded);
            At(groundState, jumpState, IsJumping);
            At(jumpState, airState, HasFinishedJumping);

            _stateMachine.SetState(airState);
        }

        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, new FuncPredicate(condition));
        void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, new FuncPredicate(condition));
        #endregion

        #region Jump
        bool jumpKeyIsPressed;    // Tracks whether the jump key is currently being held down by the player
        bool jumpKeyWasPressed;   // Indicates if the jump key was pressed since the last reset, used to detect jump initiation
        bool jumpKeyWasLetGo;     // Indicates if the jump key was released since it was last pressed, used to detect when to stop jumping
        bool jumpInputIsLocked;   // Prevents jump initiation when true, used to ensure only one jump action per press


        void HandleJumpKeyInput(bool isJumpKeyPressed)
        {
            if (!jumpKeyIsPressed && isJumpKeyPressed)
            {
                jumpKeyWasPressed = true;
            }

            if (jumpKeyIsPressed && !isJumpKeyPressed)
            {
                jumpKeyWasLetGo = true;
                jumpInputIsLocked = false;
            }

            jumpKeyIsPressed = isJumpKeyPressed;
        }

        void ResetJumpKeys()
        {
            jumpKeyWasLetGo = false;
            jumpKeyWasPressed = false;
        }

        private bool IsJumping()
        {
            bool jumpInput = (jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked;

            bool canJump = jumpInput && _isGrounded;

            if (canJump)
            {
                Debug.Log($"can jump dump : Jump Input : {jumpInput}, jumpKeyIsPressed : {jumpKeyIsPressed}, jumpKeyWasPressed : {jumpKeyWasPressed}, jumpInputIsLocked : {jumpInputIsLocked}");
            }

            return canJump;
        }

        private bool isCoyoteeJumping()
        {
            bool isCoyoteeJumping = jumpKeyWasPressed && _groundedTimer.GetTime() <= coyoteeTime && !jumpInputIsLocked;

            if (isCoyoteeJumping) Debug.LogWarning("Coyotee Jumping !!");
            return isCoyoteeJumping;
        }

        private bool HasFinishedJumping()
        {
            return jumpKeyWasLetGo || _jumpTimer.IsFinished;
        }

        #endregion

        #region Movements
        private void HandleMovementInput(Vector2 movement)
        {
            _horizontalInput = movement.x;
            _verticalInput = movement.y;
        }

        private void MovePlayer(float airDragFactor = 1f)
        {
            _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

            _rb.AddForce(_moveDirection.normalized * movementSpeed * 30f * airDragFactor, ForceMode.Force);
        }

        private void HandleDrag()
        {
            // FIXME : The check is already made in groundstate, should be removed from here ?
            _rb.drag = _isGrounded ? groundDrag : 0f;
        }

        private void HandleSpeedLimit(float limitMultiplier = 1f)
        {
            Vector3 flatVelocity = _rb.velocity.WithY(0);
            var limit = limitMultiplier * movementSpeed;

            if (flatVelocity.magnitude > limit)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * limit;
                _rb.ApplyVelocity(limitedVelocity.WithY(_rb.velocity.y));
            }
        }


        private void HandleGravity()
        {
            if (_stateMachine.CurrentState is GroundState) return;

            _rb.AddForce(Vector3.up * Physics.gravity.y * 2f * gravityMultiplier, ForceMode.Acceleration);
        }
        #endregion

        #region Ground Sensor
        private void CalibrateRaycastSensor()
        {
            if (_groundSensor == null) _groundSensor = new SimpleRaycastSensor(transform);

            _groundSensor.SetCastDirection(SimpleRaycastSensor.CastDirection.Down);
            _groundSensor.SetCastOrigin(origin.position);
            _groundSensor.castLength = groundSensorLength;
            _groundSensor.layermask = groundLayers;

        }
        private void CheckGround()
        {
            if (_groundSensor == null) CalibrateRaycastSensor();

            _groundSensor.Cast();
            _isGrounded = _groundSensor.HasDetectedHit();
        }

        #endregion

        #endregion

        #region State Methods

        internal void OnGroundedEnter()
        {
            Debug.Log("On Grounded Enter Hehe");
            _rb.RemoveVerticalVelocity();
        }

        internal void OnGroundedExit()
        {
            // For Coyotee Time
            _groundedTimer.Start();
        }

        internal void OnGroundedFixedUpdate()
        {
            CheckGround();

            MovePlayer();
            HandleDrag();

            HandleSpeedLimit();
        }

        internal void OnGroundedUpdate()
        {
        }

        internal void OnAirEnter()
        {
            Debug.Log("On Air Enter Ma doude");
        }

        internal void OnAirExit()
        {
        }

        internal void OnAirFixedUpdate()
        {
            CheckGround();

            MovePlayer(airMultiplier);

            HandleSpeedLimit(1.5f);
        }

        internal void OnAirUpdate()
        {
        }

        internal void OnJumpingEnter()
        {
            // TODO : Should check if the player was sliding (on walls or slopes) before jumping

            Debug.Log("On Jump Enter Mother Fucker");

            _rb.ApplyVerticalVelocity(jumpForce);
            _jumpTimer.Start();

            jumpInputIsLocked = true;
        }

        internal void OnJumpingExit()
        {
            if (_rb.velocity.y > 0f)
                _rb.ApplyVerticalVelocity(_rb.velocity.y * .5f);
        }

        internal void OnJumpingFixedUpdate()
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);

            CheckGround();
            MovePlayer(airMultiplier);

            HandleSpeedLimit(1.5f);
        }

        internal void OnJumpingUpdate()
        {
            
        }

        #endregion

        #region Debug
        public void PrintState()
        {
            Debug.Log("Current State : " + _stateMachine.CurrentState.Name);
        }

        #endregion
    }
}