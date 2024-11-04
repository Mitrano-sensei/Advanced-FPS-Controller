using System;
using UnityEngine;

namespace FPSController
{
    public class WallChecker : MonoBehaviour
    {
        #region Fields

        [Header("References")] 
        [SerializeField] private Transform origin;
        [SerializeField] private Transform orientation;
        
        [Header("Settings")]
        [SerializeField] private LayerMask wallLayerMask;
        
        [Header("Front Wall Settings")]
        [SerializeField] private float frontWallCheckDistance = 1.5f;
        [SerializeField] private float frontWallCheckRadius = .3f;
        
        private bool _isWallInFront;
        private float _frontWallAngle;
        private float _frontWallDistance;
        private Vector3 _frontWallNormal;

        public bool IsWallInFront => _isWallInFront;
        public float FrontWallAngle => _frontWallAngle;
        public float FrontWallDistance => _frontWallDistance;

        public Vector3 FrontWallNormal => _frontWallNormal;

        [Header("Right/Left Wall Settings")]
        [SerializeField] private float rightWallCheckDistance = 1.5f;
        [SerializeField] private float rightWallCheckRadius = .3f;
        
        private bool _isRightWall;
        private float _rightWallAngle;      // From FORWARD to Wall
        private float _rightWallDistance;
        private Vector3 _rightWallNormal;
        
        public bool IsRightWall => _isRightWall;
        public float RightWallAngle => _rightWallAngle;
        public float RightWallDistance => _rightWallDistance;
        public Vector3 RightWallNormal => _rightWallNormal;
        
        private bool _isLeftWall;
        private float _leftWallAngle;       // From FORWARD to Wall
        private float _leftWallDistance;
        private Vector3 _leftWallNormal;
        
        public bool IsLeftWall => _isLeftWall;
        public float LeftWallAngle => _leftWallAngle;
        public float LeftWallDistance => _leftWallDistance;
        public Vector3 LeftWallNormal => _leftWallNormal;
        
        #endregion    
    
        #region MonoBehaviour

        private void FixedUpdate()
        {
            CheckFrontWall();
            
            CheckRightWall();
            CheckLeftWall();
        }

        private void OnDrawGizmosSelected()
        {
            // Front Wall
            Gizmos.color = Color.green;
            Gizmos.DrawRay(origin.position, orientation.forward * frontWallCheckDistance);
            
            // Right Wall
            Gizmos.color = Color.blue;
            
            Gizmos.DrawRay(origin.position, orientation.right * rightWallCheckDistance);
            Gizmos.DrawRay(origin.position, -orientation.right * rightWallCheckDistance);
        }

        #endregion

        #region  Utils

        public float GetFrontWallNormalDistance()
        {
            if (!IsWallInFront)
                return 0f;

            Physics.Raycast(origin.position, -_frontWallNormal, out var hit, frontWallCheckDistance, wallLayerMask);
            return hit.distance;
        }
        

        #endregion
        
        /**
         * Checks Front Wall and updates information about the detected walls
         */
        private void CheckFrontWall()
        {
            _isWallInFront = Physics.SphereCast(origin.position, frontWallCheckRadius, orientation.forward, out var hit,
                frontWallCheckDistance, wallLayerMask);

            if (!_isWallInFront)
                return;

            _frontWallAngle = Vector3.Angle(orientation.forward, -hit.normal);
            _frontWallDistance = hit.distance;
            _frontWallNormal = hit.normal;
        }

        /**
         * Checks Right Wall and updates information about the detected walls.
         */
        private void CheckRightWall()
        {
            _isRightWall = Physics.SphereCast(origin.position, rightWallCheckRadius, orientation.right, out var hit,
                rightWallCheckDistance, wallLayerMask);
            
            if (!_isRightWall)
                return;
            
            _rightWallAngle = Vector3.Angle(orientation.forward, -hit.normal); // From FORWARD to Wall
            _rightWallDistance = hit.distance;
            _rightWallNormal = hit.normal;
        }

        /**
         * Checks Left Wall and updates information about the detected wall
         */
        private void CheckLeftWall()
        {
            _isLeftWall = Physics.SphereCast(origin.position, rightWallCheckRadius, -orientation.right, out var hit, 
                rightWallCheckDistance, wallLayerMask);

            if (!_isLeftWall)
                return;
            
            _leftWallAngle = Vector3.Angle(orientation.forward, -hit.normal); // From FORWARD to Wall
            _leftWallDistance = hit.distance;
            _leftWallNormal = hit.normal;
        }

        #region Debug

        public void DrawDebugSideRays(float minAngle, float maxAngle)
        {
            // Right wall
            Gizmos.color = IsRightWall ? Color.green : Color.blue;
            var minAngleRight = Quaternion.AngleAxis(minAngle, Vector3.up) * orientation.forward;
            var maxAngleRight = Quaternion.AngleAxis(maxAngle, Vector3.up) * orientation.forward;
            
            Gizmos.DrawRay(origin.position, minAngleRight * rightWallCheckDistance);
            Gizmos.DrawRay(origin.position, maxAngleRight * rightWallCheckDistance);
            
            // Left wall
            Gizmos.color = IsLeftWall ? Color.green : Color.blue;
            var minAngleLeft = Quaternion.AngleAxis(-minAngle, Vector3.up) * orientation.forward;
            var maxAngleLeft = Quaternion.AngleAxis(-maxAngle, Vector3.up) * orientation.forward;
            
            Gizmos.DrawRay(origin.position, minAngleLeft * rightWallCheckDistance);
            Gizmos.DrawRay(origin.position, maxAngleLeft * rightWallCheckDistance);
        }

        #endregion
    }
}
