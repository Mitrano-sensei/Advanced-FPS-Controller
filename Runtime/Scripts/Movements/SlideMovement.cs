using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class SlideMovement : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerBody playerMover;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform orientation;

        [Header("Sliding")]
        [SerializeField] private float slideBoost = 3f;
        [SerializeField] private float slideDragRatio = .2f;
        [SerializeField] private float minimumSlideVelocity = 1f;

        public float SlideDragRatio { get => slideDragRatio; }

        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (playerMover == null)
                playerMover = GetComponent<PlayerBody>();

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
            var forward = playerController.CalculateMovementVelocity().normalized;
            rb.AddForce(forward * slideBoost, ForceMode.Impulse);

            playerMover.SetIsCrouching(true);
            rb.AddForce(-Vector3.up * 5f, ForceMode.Impulse);

            playerController.SetMaxSpeed(playerController.GetFlatVelocity(rb.velocity).magnitude);
        }

        internal void OnSlideExit()
        {
            playerMover.SetIsCrouching(false);
            playerController.SetIsExitingCrouch(true);

            playerController.StartCoyoteeTimer();
        }

        internal void OnSlideFixedUpdate()
        {
            SlideOnSlope();
        }

        private void SlideOnSlope()
        {
            var currentSlopeNormal = playerController.CurrentSlopeNormal;
            if (currentSlopeNormal == Vector3.up)
                return;

            var movementSpeed = playerController.MovementSpeed; 

            var horizontalDirection = Vector3.Cross(currentSlopeNormal, Vector3.up);       // "Right" direction on the slope, horizontal part of the slope
            var slopeDirection = Vector3.Cross(currentSlopeNormal, horizontalDirection);   // Direction of the slope (going down)

            var slopeAngle = Vector3.Angle(Vector3.up, currentSlopeNormal);
            var speedRatio = slopeAngle / 20f;

            rb.AddForce(slopeDirection * movementSpeed * speedRatio, ForceMode.Impulse);
        }

        public bool IsSliding()
        {
            return (playerController.IsCrouchingKeyPressed || playerController.IsCrouchingKeyHeld) && (playerMover.IsGrounded() && (rb.velocity.magnitude > minimumSlideVelocity));
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
