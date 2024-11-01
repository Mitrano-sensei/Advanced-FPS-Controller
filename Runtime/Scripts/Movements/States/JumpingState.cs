using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    public class JumpingState : MonoBehaviour, IFPSState
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float drag = 0f;
        [Space]
        [SerializeField] private float coyoteeTimeInSeconds;

        [Space] 
        [SerializeField] private float airMovementSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float airControl = .8f;

        private PlayerController _playerController;
        private Rigidbody _rb;
        private PlayerGroundChecker _groundChecker;
        private FPSInputReader _inputReader;

        private CountdownTimer _coyoteeTimer;

        public string Name => "Jumping State";

        private void Start()
        {
            _coyoteeTimer = new CountdownTimer(coyoteeTimeInSeconds);

            _groundChecker.OnGroundLost += StartCoyoteeTimer;
        }

        private void OnValidate()
        {
        }

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
            _rb.ApplyVerticalVelocity(jumpForce);

            _inputReader.LockJumpKey();
            
            StopCoyoteeTimer();

            Debug.Log("Jumping OnEnter");
        }

        public void OnExit()
        {
            if (_rb.velocity.y > 0)
                _rb.ApplyVerticalVelocity(_rb.velocity.y * .5f);
        }

        #region Setup

        public void SetPlayerController(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void SetRigidbody(Rigidbody rb)
        {
            _rb = rb;
        }

        public void SetGroundChecker(PlayerGroundChecker groundChecker)
        {
            _groundChecker = groundChecker;
        }

        public void SetInputReader(FPSInputReader inputReader)
        {
            _inputReader = inputReader;
        }
        #endregion

        #region Check

        public bool IsJumpingEnter()
        {
            var classicJump = _groundChecker.IsGrounded;
            var jumpInput = (_inputReader.JumpKeyPressed || _inputReader.JumpKeyHeld) && !_inputReader.JumpKeyIsLocked;
            var coyoteeJump = _coyoteeTimer.IsRunning;

            return jumpInput && (classicJump || coyoteeJump);
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
            var direction = _groundChecker.Forward * input.y + _groundChecker.Right * input.x;
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
    }
}
