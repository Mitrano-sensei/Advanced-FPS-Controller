using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    public class PlayerMover : MonoBehaviour
    {
        #region Fields
        [Header("Player Collider Info")]
        [SerializeField] private float playerTotalHeight = 1.6f;
        [SerializeField] private float playerRadius = 0.7f;
        [SerializeField] private float playerLegsHeight = 0.5f;
        
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayers;

        private Vector3 _groundSensorOriginOffset = Vector3.zero;
        private float _groundSensorCastLength = 1.1f;

        [Header("Components")]
        [SerializeField] private CapsuleCollider playerCollider;
        [SerializeField] private Rigidbody rb;

        private RaycastSensor _groundSensor;
        private bool _isGrounded;

        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            CheckReferences();
        }

        void Start()
        {
            ReCalculatePlayerCollider();
            CalibrateGroundSensor();
        }

        private void FixedUpdate()
        {
            CheckForGround();
        }

        void OnValidate()
        {
            ReCalculatePlayerCollider();

            if (_groundSensor != null && (_groundSensorCastLength != _groundSensor.GetCastLength() || _groundSensorOriginOffset != _groundSensor.GetRawCastOriginOffset() || _groundSensor.GetLayerMasks() != groundLayers))
            {
                CalibrateGroundSensor();
            }
        }

        private void OnDrawGizmosSelected()
        {
            _groundSensor?.DrawDebug();

            DrawLegsDebug();
        }
        #endregion

        #region Ground Detection

        public bool IsGrounded() => _isGrounded;

        private void CalibrateGroundSensor()
        {
            if (_groundSensor == null)
                _groundSensor = new RaycastSensor(transform);

            _groundSensor.SetCastDirection(RaycastSensor.CastDirection.Down);
            _groundSensor.SetCastOrigin(playerCollider.bounds.center + _groundSensorOriginOffset);
            _groundSensor.SetCastLength(_groundSensorCastLength);
            _groundSensor.SetLayerMask(groundLayers);
        }

        private void CheckForGround()
        {
            _groundSensor.Cast();

            _isGrounded = _groundSensor.HasDetectedHit();
        }

        #endregion

        #region Movements
        public void SetVelocity(Vector3 velocity) => rb.ApplyVelocity(velocity);

        /**
         * A positive value means the player is above the ground, a negative value means the player is below the ground.
         */
        public Vector3 GetGroundAdjustmentVelocity()
        {
            if (!_groundSensor.HasDetectedHit())
                return Vector3.zero;

            float distance = _groundSensor.GetDistance();
            var distanceToGo = (_groundSensorCastLength) - distance;

            return Vector3.up * distanceToGo * 10f; // <= Tbh it's a magic number, but it works
        }

        #endregion

        private void ReCalculatePlayerCollider()
        {
            var capsuleHeight = playerTotalHeight - playerLegsHeight;

            playerCollider.height = capsuleHeight;
            playerCollider.radius = playerRadius;
            playerCollider.center = new Vector3(0f, capsuleHeight * .5f + playerLegsHeight, 0f);

            _groundSensorCastLength = capsuleHeight * .5f + playerLegsHeight;
        }

        private void CheckReferences()
        {
            if (playerCollider == null || rb == null)
            {
                Debug.LogError("Missing components");
                enabled = false;
            }
        }

        private void DrawLegsDebug()
        {
            Gizmos.color = Color.yellow;
            var bodyHeight = playerTotalHeight - playerLegsHeight;
            Gizmos.DrawRay(playerCollider.bounds.center, Vector3.down * (playerLegsHeight + bodyHeight * .5f));
        }
    }
}
