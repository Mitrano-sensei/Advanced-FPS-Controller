using System;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace FPSController
{
    public class WallRunState : MonoBehaviour, IFPSState
    {
        [Header("References")]
        [SerializeField] private WallChecker wallChecker;
        
        private PlayerController _playerController;
        private PlayerBody _body;
        private FPSInputReader _inputReader;
        private Rigidbody _rb;

        [Header("Wall Run Settings")] 
        [SerializeField] private float minWallAngle = 70f;
        [SerializeField] private float maxWallAngle = 110f;
        [SerializeField] private float maxWallDistance = 1f;
        [Space]
        [SerializeField] private float wallRunSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float sideMoveRatio = .5f;      // Move Ratio when wall running and pressing right/left
        [SerializeField] private float wallRunDrag = 6f;
        [SerializeField] private float wallRunTimeInSeconds = 1f;

        [Header("Events")]
        [SerializeField] private UnityEvent<bool> onWallRunEnter; // True if right wall, False if left wall
        [SerializeField] private UnityEvent onWallRunExit;
        
        [Header("Misc")] 
        [SerializeField] private bool showDebug = true;
        
        public bool WallRunFlag => _wallRunFlag;
        
        private Vector3 _lastWallNormal;
        
        private CountdownTimer _wallRunTimer;
        private bool _hasTouchedGround = true;
        private bool _wallRunFlag;
        
        private WallRunSide _wallRunSide;
        
        public string Name => "Wall Run State";
        
        #region MonoBehaviour

        private void Start()
        {
            _body.OnGroundRetrieved += () => _hasTouchedGround = true;
            _playerController.OnStateChange += fpsState =>
            {
                if (fpsState is not (WallRunState or ClimbingState))
                {
                    _wallRunFlag = false;
                }
            };
        }

        private void OnDrawGizmos()
        {
            DrawDebugRays();
        }

        #endregion

        #region State
        
        public void OnEnter()
        {
            Debug.Log("Wall Run OnEnter");
            _hasTouchedGround = false;
            _wallRunFlag = true;
            
            _wallRunTimer ??= new CountdownTimer(wallRunTimeInSeconds);
            
            _wallRunTimer.Reset(wallRunTimeInSeconds);
            _wallRunTimer.Start();
            _lastWallNormal = GetNormal();
            
            _rb.AddForce(GetWallRunDirection() * wallRunSpeed, ForceMode.Impulse);
            onWallRunEnter?.Invoke(_wallRunSide == WallRunSide.Right);
            // Debug.Log("FLAG : " + GetWallRunDirection());
        }

        public void OnStateUpdate()
        {
            _wallRunTimer.Tick(Time.deltaTime);
        }

        public void OnStateFixedUpdate()
        {
        }

        public void OnExit()
        {
            onWallRunExit?.Invoke();
        }
        
        public void HandleGravity() { }

        public void HandleMovementInputs()
        {
            var inputDirection = _inputReader.Direction;
            var wallRunDirection = GetWallRunDirection();

            var right = _wallRunSide == WallRunSide.Right ? -GetNormal() : GetNormal();
            var movement = wallRunDirection * (inputDirection.y * wallRunSpeed) + right * (inputDirection.x * wallRunSpeed * sideMoveRatio);

            _rb.AddForce(movement, ForceMode.Acceleration);
        }

        public void HandleLimitSpeed()
        {
            _rb.velocity = _rb.velocity.ClampMagnitude(wallRunSpeed);
        }
        
        #endregion

        #region Checks

        public bool IsWallRunEnter()
        {
            if (!CanWallRun()) return false;
            _wallRunSide = IsWallOnRight() ? WallRunSide.Right : WallRunSide.Left;

            if (!_hasTouchedGround && GetNormal() == _lastWallNormal) return false; // If changes wall, can wall run
            return true;
        }

        public bool IsWallRunExit()
        {
            return !CanWallRun() || _wallRunTimer.IsFinished;
        }
        
        private bool CanWallRun()
        {
            return IsWallOnRight() || IsWallOnLeft();
        }
        
        private bool IsWallOnRight()
        {
            return wallChecker.IsRightWall && wallChecker.RightWallDistance < maxWallDistance && 
                   CheckAngle(wallChecker.RightWallAngle);
        }
        
        private bool IsWallOnLeft()
        {
            return wallChecker.IsLeftWall && wallChecker.LeftWallDistance < maxWallDistance && 
                   CheckAngle(wallChecker.LeftWallAngle);
        }


        private bool CheckAngle(float angle)
        {
            return minWallAngle < angle && angle < maxWallAngle;
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

        public float GetDrag() => wallRunDrag;

        #endregion

        #region Debug

        private void DrawDebugRays()
        {
            if (!showDebug) return;
            
            wallChecker.DrawDebugSideRays(minWallAngle, maxWallAngle);
        }

        #endregion
        
        private Vector3 GetWallRunDirection()
        {
            var normal = GetNormal();
            
            var direction = Vector3.Cross(normal * (_wallRunSide == WallRunSide.Left ? 1f : -1f), Vector3.up);
            return direction.normalized;
        }
        
        public Vector3 GetNormal()
        {
            return _wallRunSide switch
            {
                WallRunSide.Left => wallChecker.LeftWallNormal,
                WallRunSide.Right => wallChecker.RightWallNormal,
                _ => Vector3.zero
            };
        }
        
        private enum WallRunSide
        {
            Left,
            Right
        }
        
    }
}