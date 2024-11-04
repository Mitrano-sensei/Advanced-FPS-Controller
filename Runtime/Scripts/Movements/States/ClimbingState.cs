using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace FPSController
{
    public class ClimbingState : MonoBehaviour, IFPSState
    {
        [Header("References")]
        [SerializeField] private WallChecker wallChecker;
        [SerializeField] private CapsuleCollider capsuleCollider;

        private PlayerController _playerController;
        private PlayerBody _body;
        private FPSInputReader _inputReader;
        private Rigidbody _rb;
        
        [Header("Climbing State Settings")]
        [SerializeField] private float climbingSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float horizontalClimbingSpeedRatio = .3f;
        [SerializeField] private float climbingDrag = 6f;
        [Space] 
        [SerializeField] private float maxDistanceToStartClimbing = 1f;
        [SerializeField] private float maxWallAngle = 40f;
        [Space] 
        [SerializeField] private float distanceToWall = .1f;
        [Space] 
        [SerializeField] private float climbTimeInSeconds = 1f;
        
        private CountdownTimer _climbTimer;
        private bool _hasTouchedGround = true;
        
        public string Name => "Climbing State";

        private void Start()
        {
            if (wallChecker == null) Debug.LogError("WallChecker is not assigned in ClimbingState");
            if (capsuleCollider == null) Debug.LogError("CapsuleCollider is not assigned in ClimbingState");
            
            _body.OnGroundRetrieved += () => _hasTouchedGround = true;
        }

        public void OnEnter()
        {
            Debug.Log("Climbing OnEnter");

            _hasTouchedGround = false;
            
            _climbTimer ??= new(climbTimeInSeconds);
            
            _climbTimer.Reset(climbTimeInSeconds);
            _climbTimer.Start();
        }
        
        public void OnStateUpdate()
        {
            _climbTimer.Tick(Time.deltaTime);
        }

        public void OnStateFixedUpdate()
        {
        }

        public void OnExit()
        {
        }
        
        #region Checks

        public bool IsClimbingEnter()
        {
            return IsClimbAllowed() && _hasTouchedGround; 
        }
        
        public bool IsClimbingExit()
        {
            return !IsClimbAllowed() || IsClimbingTimeFinished();
        }

        private bool IsClimbAllowed()
        {
            bool isWallValid = wallChecker.IsWallInFront && wallChecker.FrontWallDistance < maxDistanceToStartClimbing;
            bool isAngleValid = wallChecker.FrontWallAngle < maxWallAngle;

            return isWallValid && isAngleValid;
        }
        
        private bool IsClimbingTimeFinished() => _climbTimer.IsFinished;
        
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

        public float GetDrag()
        {
            return climbingDrag;
        }

        public void HandleGravity()
        {
            _rb.ApplyVerticalVelocity(climbingSpeed);

            var currentDistanceToWall = wallChecker.GetFrontWallNormalDistance();
            if (!Mathf.Approximately(currentDistanceToWall, capsuleCollider.radius + distanceToWall))
                FixDistanceToWall(currentDistanceToWall);
        }

        private void FixDistanceToWall(float currentDistanceToWall)
        {
            float delta = currentDistanceToWall - (capsuleCollider.radius + distanceToWall);
            _rb.AddForce(-wallChecker.FrontWallNormal * delta, ForceMode.VelocityChange);
        }
        
        public void HandleMovementInputs()
        {
            var input = _inputReader.Direction;

            var right = Vector3.ProjectOnPlane(_body.Right, wallChecker.FrontWallNormal).normalized;

            var steerDirection = right * _inputReader.Direction.x; 
            var movement = steerDirection * (climbingSpeed * horizontalClimbingSpeedRatio);

            _rb.AddForce(movement, ForceMode.VelocityChange);
        }
    }
}