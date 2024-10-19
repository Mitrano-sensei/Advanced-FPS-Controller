using FiniteStateMachine;
using UnityEngine;
using Utilities;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class ClimbMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orientation;

        private Rigidbody _rb;
        private PlayerController _playerController;

        [Header("Climb Settings")]
        [SerializeField] private float climbTimeInSeconds = 1f;
        [SerializeField] private LayerMask whatIsWall;

        private CountdownTimer _climbTimer;

        [Header("Detection")]
        [SerializeField] private float detectionDistance = 0.55f;
        [SerializeField] private float sphereCastRadius = 0.5f;
        [SerializeField] private float maxWallLookAngle = 30f;

        private RaycastHit _wallHit;
        private float _wallLookAngle;

        private bool _isWallInFront;
        private Vector3 _wallNormal;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody>();
        }

        #region Check
        public void WallCheck(bool useExtended = false, bool useLastWall = false)
        {
            float extensionRatio = useExtended ? 1.4f : 1f;
            Vector3 direction = useLastWall ? -_playerController.LastWallNormal : orientation.forward;

            _isWallInFront = Physics.SphereCast(transform.position, sphereCastRadius, direction, out _wallHit, detectionDistance * extensionRatio, whatIsWall);
            _wallLookAngle = Vector3.Angle(orientation.forward, -_wallHit.normal);
            _wallNormal = _wallHit.normal;
        }

        public bool IsClimbingEnter()
        {
            bool isWallValid = _isWallInFront && _wallLookAngle < maxWallLookAngle;
            bool isRequestedClimb = (_playerController.JumpKeyPressed || _playerController.JumpKeyHeld);
            bool isClimbAllowed = (!_playerController.IsExitingClimb || _wallNormal != _playerController.LastWallNormal);

            return isWallValid && isRequestedClimb && isClimbAllowed;
        }

        public bool IsClimbingExit()
        {
            WallCheck(true, true);
            bool isClimbingExit = _playerController.JumpKeyReleased || _climbTimer.IsFinished || !_isWallInFront;

            return isClimbingExit;
        }

        #endregion

        public void OnClimbEnter()
        {
            if (_climbTimer == null) _climbTimer = new CountdownTimer(climbTimeInSeconds);

            _climbTimer.Reset(climbTimeInSeconds);
            _climbTimer.Start();

            _playerController.LastWallNormal = _wallNormal;
        }

        public void OnClimbUpdate()
        {
            _climbTimer.Tick(Time.deltaTime);
        }

        public void OnClimbFixedUpdate()
        {
            _rb.ApplyVerticalVelocity(_playerController.GetCurrentMaxSpeed());
        }

        public void OnClimbExit()
        {
            _playerController.IsExitingClimb = true;
        }

        public class ClimbingState : IState, IFPSState
        {
            public string Name => "Climbing State";
            private ClimbMovement _climbMovement;
            private PlayerController _playerController;

            public ClimbingState(PlayerController playerController, ClimbMovement climbMovement)
            {
                _playerController = playerController;
                _climbMovement = climbMovement;
            }

            public float GetMovementSpeedRatio() => .6f;
            public bool IsLimitedSpeed() => false;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());
                _climbMovement.OnClimbFixedUpdate();
            }

            public void OnEnter() => _climbMovement.OnClimbEnter();

            public void OnExit() => _climbMovement.OnClimbExit();

            public void Update() => _climbMovement.OnClimbUpdate();
        }
    }


    

}
