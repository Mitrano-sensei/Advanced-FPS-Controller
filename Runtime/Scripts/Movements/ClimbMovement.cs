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
        [SerializeField] private Rigidbody rb;
        [SerializeField] private LayerMask whatIsWall;

        private PlayerController _playerController;

        [Header("Climb Settings")]
        [SerializeField] private float climbTimeInSeconds = 1f;

        private CountdownTimer _climbTimer;

        [Header("Detection")]
        [SerializeField] private float detectionDistance = 0.55f;
        [SerializeField] private float sphereCastRadius = 0.5f;
        [SerializeField] private float maxWallLookAngle = 30f;

        private RaycastHit _wallHit;
        private float _wallLookAngle;

        private bool _isWallInFront;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        #region Check
        public void WallCheck()
        {
            _isWallInFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out _wallHit, detectionDistance, whatIsWall);
            _wallLookAngle = Vector3.Angle(orientation.forward, -_wallHit.normal);
        }

        public bool IsClimbingEnter()
        {
            return _isWallInFront && _wallLookAngle < maxWallLookAngle && (_playerController.JumpKeyPressed || _playerController.JumpKeyHeld);
        }

        public bool IsClimbingExit()
        {
            return _playerController.JumpKeyReleased || _climbTimer.IsFinished;
        }

        #endregion

        public void OnClimbEnter()
        {
            if (_climbTimer == null) _climbTimer = new CountdownTimer(climbTimeInSeconds);

            _climbTimer.Reset(climbTimeInSeconds);
            _climbTimer.Start();
        }

        public void OnClimbUpdate()
        {
            _climbTimer.Tick(Time.deltaTime);
        }

        public void OnClimbFixedUpdate()
        {
            rb.ApplyVerticalVelocity(_playerController.GetCurrentMaxSpeed());
        }

        public void OnClimbExit()
        {
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
