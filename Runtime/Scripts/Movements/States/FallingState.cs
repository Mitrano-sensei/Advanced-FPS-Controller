using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    public class FallingState : MonoBehaviour, IFPSState
    {
        [Header("Settings")]
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float drag = 0f;
        [Space]
        [SerializeField] private float airMovementSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float airControl = .8f;

        private PlayerBody _body;
        private Rigidbody _rb;
        private PlayerController _playerController;
        private FPSInputReader _inputReader;

        public string Name => "Falling State";

        public void OnStateFixedUpdate()
        {
        }

        public void HandleGravity()
        {
            _rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }

        public void OnEnter()
        {
            Debug.Log("On Falling Enter");

            _body.UseExtendedGroundCheck = false;
        }

        public void OnExit()
        {
        }

        public void OnStateUpdate()
        {
        }

        #region Setup

        public void SetPlayerBody(PlayerBody body)
        {
            _body = body;
        }

        public void SetPlayerController(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void SetRigidbody(Rigidbody rb)
        {
            _rb = rb;
        }

        public void SetInputReader(FPSInputReader inputReader)
        {
            _inputReader = inputReader;
        }
        #endregion

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
    }
}
