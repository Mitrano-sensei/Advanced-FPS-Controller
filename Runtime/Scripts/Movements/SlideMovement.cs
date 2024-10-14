using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class SlideMovement : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private Transform orientation;

        private PlayerController _playerController;
        private PlayerBody _playerBody;
        private Rigidbody _rb;

        [Header("Sliding")]
        [SerializeField] private float slideBoost = 3f;
        [SerializeField] private float slideDragRatio = .2f;
        [SerializeField] private float minimumSlideVelocity = 1f;

        public float SlideDragRatio { get => slideDragRatio; }

        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (_playerController == null)
                _playerController = GetComponent<PlayerController>();

            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            if (_playerBody == null)
                _playerBody = GetComponent<PlayerBody>();

            if (orientation == null )
            {
                Debug.LogError("Missing Orientation :c");
                enabled = false;
            }
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
        #endregion

        #region Slide

        internal void OnSlideEnter()
        {
            var forward = _playerController.CalculateMovementVelocity().normalized;
            _rb.AddForce(forward * slideBoost, ForceMode.Impulse);

            _playerBody.SetIsCrouching(true);
            _rb.AddForce(-Vector3.up * 5f, ForceMode.Impulse);

            _playerController.IsExitingClimb = false;
            _playerController.SetMaxSpeed(_playerController.GetFlatVelocity(_rb.velocity).magnitude);
        }

        internal void OnSlideExit()
        {
            _playerBody.SetIsCrouching(false);
            _playerController.IsExitingCrouch = true;

            _playerController.StartCoyoteeTimer();
        }

        internal void OnSlideFixedUpdate()
        {
            SlideOnSlope();
        }

        private void SlideOnSlope()
        {
            var currentSlopeNormal = _playerController.CurrentSlopeNormal;
            if (currentSlopeNormal == Vector3.up)
                return;

            var movementSpeed = _playerController.MovementSpeed; 

            var horizontalDirection = Vector3.Cross(currentSlopeNormal, Vector3.up);       // "Right" direction on the slope, horizontal part of the slope
            var slopeDirection = Vector3.Cross(currentSlopeNormal, horizontalDirection);   // Direction of the slope (going down)

            var slopeAngle = Vector3.Angle(Vector3.up, currentSlopeNormal);
            var speedRatio = slopeAngle / 20f;

            _rb.AddForce(slopeDirection * movementSpeed * speedRatio, ForceMode.Impulse);
        }

        public bool IsSliding()
        {
            return (_playerController.IsCrouchingKeyPressed || _playerController.IsCrouchingKeyHeld) && (_playerBody.IsGrounded() && (_rb.velocity.magnitude > minimumSlideVelocity));
        }

        #endregion


        public class SlidingState : IState, IFPSState
        {
            public string Name => "Sliding State";
            private PlayerController _playerController;
            private SlideMovement _slideMovement;

            public SlidingState(PlayerController playerController, SlideMovement slideMovement)
            {
                _playerController = playerController;
                _slideMovement = slideMovement;
            }

            public float GetMovementSpeedRatio() => .1f;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());

                _slideMovement.OnSlideFixedUpdate();
            }

            public void OnEnter()
            {
                _slideMovement.OnSlideEnter();
            }

            public void OnExit()
            {
                _slideMovement.OnSlideExit();
            }

            public void Update()
            {
            }

            public bool IsLimitedSpeed() => false;
        }
    }
}
