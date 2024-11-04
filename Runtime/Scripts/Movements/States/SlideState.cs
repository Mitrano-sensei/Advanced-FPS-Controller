using UnityEngine;
using Utilities;

namespace FPSController
{
    public class SlideState : MonoBehaviour, IFPSState
    {
        [Header("References")]
        private PlayerController _playerController;
        private Rigidbody _rb;
        private PlayerBody _body;
        private FPSInputReader _inputReader;

        [Header("Slide Settings")] 
        [SerializeField] private float slideDrag = 1f;
        [SerializeField] private float minimumSlideSpeed = 1f;
        [Space]
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float slopeSpeed = 20f;
        
        public string Name => "Slide State";
        
        #region State
        public void OnEnter()
        {
            _body.SetCrouchMode(true);

            _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse); 
            
            _body.UseExtendedGroundCheck = true;
            
            Debug.Log("Slide OnEnter");
        }

        public void OnStateUpdate()
        {
        }

        public void OnStateFixedUpdate()
        {
        }

        public void OnExit()
        {
            _body.SetCrouchMode(false);
            
            _body.UseExtendedGroundCheck = false;
        }

        public float GetDrag() => slideDrag;

        public void HandleGravity()
        {
            HandleGravityOnSlope();
            
            if (!_body.IsGrounded) return;
            var adjustment = _body.GetGroundAdjustment();

            _rb.ApplyVerticalVelocity(adjustment / Time.fixedDeltaTime);
        }

        #endregion

        private void HandleGravityOnSlope()
        {
            var slopeNormal = _body.CurrentSlopeNormal;
            if (slopeNormal == Vector3.up) return;
            
            var direction = _body.SlopeDirection;
            var slopeRatio = (_body.CurrentSlopeAngle / (.8f * _body.MaxSlopeAngle));
            var slopeForce = direction * (slopeRatio * slopeSpeed);
            
            _rb.AddForce(slopeForce, ForceMode.Acceleration);
        }
        
        #region Checks

        public bool IsSlideEnter()
        {
            var slideInput = _inputReader.IsCrouchKeyPressed || _inputReader.IsCrouchKeyHeld;
            var isGrounded = _body.IsGrounded;
            var minimumSpeed = _rb.velocity.magnitude >= minimumSlideSpeed;
            
            return slideInput && isGrounded && minimumSpeed;
        }

        public bool IsSlideExit()
        {
            var slideInput = _inputReader.IsCrouchKeyPressed || _inputReader.IsCrouchKeyHeld;
            var isGrounded = _body.IsGrounded;
            
            return !slideInput || !isGrounded;
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