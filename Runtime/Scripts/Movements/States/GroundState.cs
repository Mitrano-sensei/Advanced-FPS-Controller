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
        private PlayerGroundChecker _groundChecker;
        private FPSInputReader _inputReader;

        public string Name => "Grounded State";


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
            if (!_groundChecker.IsGrounded) return;
            var adjustment = _groundChecker.GetGroundAdjustment();

            _rb.ApplyVerticalVelocity(adjustment / Time.fixedDeltaTime);
        }

        public void OnEnter()
        {
            Debug.Log("Grounded OnEnter");
            
            _groundChecker.UseExtendedGroundCheck = true;
        }

        public void OnExit()
        {
            _groundChecker.UseExtendedGroundCheck = false;
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
        public void HandleMovementInputs()
        {
            var input = _inputReader.Direction;
            var direction = _groundChecker.Forward * input.y + _groundChecker.Right * input.x;
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
    }
}
