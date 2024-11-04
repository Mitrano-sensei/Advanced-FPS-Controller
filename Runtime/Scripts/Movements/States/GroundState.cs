using FiniteStateMachine;
using UnityEngine;
using Utilities;

namespace FPSController
{
    public class GroundState : MonoBehaviour, IFPSState
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float groundDrag = 6f;

        private PlayerController _playerController;
        private Rigidbody _rb;
        private PlayerBody _body;
        private FPSInputReader _inputReader;

        public string Name => "Grounded State";


        #region State
        public void OnStateUpdate()
        {
        }

        public void OnStateFixedUpdate()
        {
        }

        public void HandleGravity()
        {
            // FIXME : Does not apply gravity, Should be handled on OnStateFixedUpdate instead ?

            // Apply ground adjustment
            if (!_body.IsGrounded) return;
            var adjustment = _body.GetGroundAdjustment();

            _rb.ApplyVerticalVelocity(adjustment / Time.fixedDeltaTime);
        }

        public void OnEnter()
        {
            Debug.Log("Grounded OnEnter");
            
            _body.UseExtendedGroundCheck = true;
        }

        public void OnExit()
        {
            _body.UseExtendedGroundCheck = false;
        }
        
        public void HandleMovementInputs()
        {
            var input = _inputReader.Direction;
            var direction = _body.Forward * input.y + _body.Right * input.x;
            var movement = direction * movementSpeed;

            _rb.AddForce(movement, ForceMode.VelocityChange);
        }

        public void HandleLimitSpeed()
        {
            if (_rb.velocity.magnitude > movementSpeed)
                _rb.velocity = _rb.velocity.normalized * movementSpeed;
        }

        public float GetDrag()
        {
            return groundDrag;
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
        
        
    }
}
