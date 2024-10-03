using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovements : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private InputReader inputReader;

        [Header("Movement")]
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private Transform orientation;
        [SerializeField] private float groundDrag = 5f;

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

        StopwatchTimer _groundedTimer = new StopwatchTimer();
        SimpleRaycastSensor _groundSensor;
        bool _isGrounded;

        #endregion

        #region MonoBehaviour 
        void Start()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.freezeRotation = true;
        
            inputReader.Move += HandleMovementInput;
            inputReader.Jump += HandleJump; // <== FIXME : Should be HandleJumpInput ? 

            CalibrateRaycastSensor();

            _jumpTimer = new CountdownTimer(jumpCooldown);
        }

        private void Update()
        {
            _jumpTimer.Tick(Time.deltaTime);
            _groundedTimer.Tick(Time.deltaTime);
        }

        void FixedUpdate()
        {
            CheckGround();

            MovePlayer();
            HandleDrag();

            HandleSpeedLimit();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (_jumpTimer != null && jumpCooldown != _jumpTimer.GetTime())
            {
                _jumpTimer = new CountdownTimer(jumpCooldown);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundSensor == null) return;
            _groundSensor.DrawDebug();
        }
        #endregion

        #region Public Methods


        #endregion

        #region Private Methods
        private void HandleMovementInput(Vector2 movement)
        {
            _horizontalInput = movement.x;
            _verticalInput = movement.y;
        }

        /**
         * <param name="isJumpPressed"> : True if the jump button is pressed, false if released </param>
         */
        private void HandleJump(bool isJumpPressed)
        {
            bool jumpTimerFinished = (_jumpTimer.IsFinished || !_jumpTimer.IsRunning);
            bool groundedOrCoyotee = (_isGrounded || _groundedTimer.GetTime() < coyoteeTime);
            bool canJump = isJumpPressed && groundedOrCoyotee && jumpTimerFinished;

            if (canJump)
            {
                _rb.velocity = _rb.velocity.WithY(0);

                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _jumpTimer.Start();
            }

            // If key is released
            if (!isJumpPressed)
            {
                if (_rb.velocity.y > 0f)
                    _rb.velocity = _rb.velocity.WithY(_rb.velocity.y * .5f);
            }
        }

        private void MovePlayer()
        {
            _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

            var airFactor = _isGrounded ? 1f : airMultiplier;
            _rb.AddForce(_moveDirection.normalized * movementSpeed * 30f * airFactor, ForceMode.Force);
        }

        private void HandleDrag()
        {
            _rb.drag = _isGrounded ? groundDrag : 0f;
        }

        private void CalibrateRaycastSensor()
        {
            if (_groundSensor == null) _groundSensor = new SimpleRaycastSensor(transform);

            _groundSensor.SetCastDirection(SimpleRaycastSensor.CastDirection.Down);
            _groundSensor.SetCastOrigin(origin.localPosition);
            _groundSensor.castLength = groundSensorLength;
            _groundSensor.layermask = groundLayers;

        }
        private void CheckGround()
        {
            if (_groundSensor == null) CalibrateRaycastSensor();

            _groundSensor.Cast();
            _isGrounded = _groundSensor.HasDetectedHit();

            // For Coyotee time
            if (_isGrounded)
                _groundedTimer.Reset();
        }

        private void HandleSpeedLimit()
        {
            Vector3 flatVelocity = _rb.velocity.WithY(0);

            if (flatVelocity.magnitude > movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
                _rb.velocity = limitedVelocity.WithY(_rb.velocity.y);
            }
        }
        #endregion
    }
}
