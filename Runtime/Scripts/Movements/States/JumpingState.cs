using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    public class JumpingState : MonoBehaviour, IFPSState
    {
        [Header("References")] [SerializeField]
        private WallRunState wallRunState;

        private PlayerController _playerController;
        private Rigidbody _rb;
        private PlayerBody _body;
        private FPSInputReader _inputReader;

        [Header("Jump Settings")] [SerializeField]
        private float jumpForce = 5f;

        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float drag = 0f;
        [Space] 
        [SerializeField] private float coyoteeTimeInSeconds;

        [Space] 
        [SerializeField] private float airMovementSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float airControl = .8f;

        private CountdownTimer _coyoteeTimer;

        public string Name => "Jumping State";

        #region MonoBehaviour

        private void Start()
        {
            _coyoteeTimer = new CountdownTimer(coyoteeTimeInSeconds);

            _body.OnGroundLost += StartCoyoteeTimer;
            
            if (wallRunState == null)
                Debug.Log("INFO : Wall Run State is not assigned in JumpingState");
        }

        #endregion

        #region State

        public void OnStateFixedUpdate()
        {
        }

        public void OnStateUpdate()
        {
            _coyoteeTimer.Tick(Time.deltaTime);
        }

        public void HandleGravity()
        {
            _rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }

        public void OnEnter()
        {
            _inputReader.LockJumpKey();
            StopCoyoteeTimer();

            if (wallRunState == null || !wallRunState.WallRunFlag)
                HandleDefaultJump();
            else
                HandleWallJump();


            Debug.Log("Jumping OnEnter");
        }

        public void OnExit()
        {
            if (_rb.velocity.y > 0)
                _rb.ApplyVerticalVelocity(_rb.velocity.y * .5f);
        }

        #endregion



        #region Setup

        public void SetPlayerController(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void SetRigidbody(Rigidbody rb)
        {
            _rb = rb;
        }

        public void SetPlayerBody(PlayerBody body)
        {
            _body = body;
        }

        public void SetInputReader(FPSInputReader inputReader)
        {
            _inputReader = inputReader;
        }

        #endregion

        #region Check

        public bool IsJumpingEnter()
        {
            var classicJump = _body.IsGrounded;
            var jumpInput = (_inputReader.JumpKeyPressed || _inputReader.JumpKeyHeld) && !_inputReader.JumpKeyIsLocked;
            var coyoteeJump = _coyoteeTimer.IsRunning;
            var wallJump = wallRunState != null && wallRunState.WallRunFlag;

            return jumpInput && (classicJump || coyoteeJump || wallJump);
        }

        public bool IsJumpingExit()
        {
            return _inputReader.JumpKeyReleased;
        }

        #endregion

        private void StartCoyoteeTimer()
        {
            _coyoteeTimer.Reset(coyoteeTimeInSeconds);
            _coyoteeTimer.Start();
        }

        private void StopCoyoteeTimer()
        {
            _coyoteeTimer.Stop();
        }

        public void HandleMovementInputs()
        {
            var input = _inputReader.Direction;
            var direction = _body.Forward * input.y + _body.Right * input.x;
            var movement = direction * (airControl * airMovementSpeed);

            _rb.AddForce(movement, ForceMode.VelocityChange);
        }

        public void HandleLimitSpeed()
        {
            Vector3 flatVelocity = _rb.velocity.WithY(0);
            float flatSpeed = flatVelocity.magnitude;

            if (flatSpeed > airMovementSpeed)
                _rb.velocity = flatVelocity.normalized * airMovementSpeed + Vector3.up * _rb.velocity.y;
        }

        public float GetDrag()
        {
            return drag;
        }

        private void HandleDefaultJump()
        {
            _rb.ApplyVerticalVelocity(jumpForce);
        }

        private void HandleWallJump()
        {
            _rb.ApplyVerticalVelocity(jumpForce);
            _rb.AddForce(wallRunState.GetNormal() * jumpForce, ForceMode.Impulse);
        }
    }
}
