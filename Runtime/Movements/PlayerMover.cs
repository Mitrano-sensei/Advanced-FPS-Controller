using System;
using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("Collider Settings")]
        [Range(0f, 1f)][SerializeField] float stepHeightRatio = .1f;
        [SerializeField] float colliderHeight = 2f;
        [SerializeField] float colliderThickness = 1f;
        [SerializeField] Vector3 colliderOffset = Vector3.zero;

        Rigidbody _rb;
        CapsuleCollider _collider;
        Transform _tr;
        RaycastSensor _raycastSensor;

        bool _isGrounded;
        float _baseSensorRange;
        Vector3 _currentGroundAdjustmentVelocity;       // Velocity to adjust player position to maintain ground contact
        int currentLayer;

        [Header("Sensor Settings")]
        [SerializeField] bool isInDebugMode;
        bool isUsingExtendedSensorRange = true;         // Use extended range for smoother ground transitions


        private void Awake()
        {
            Setup();
            RecalculateColliderDimensions();
        }

        
        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                RecalculateColliderDimensions();
            }
        }

        public void CheckForGround()
        {
            if (currentLayer != gameObject.layer)
            {
                RecalculateSensorLayerMask();
            }

            _currentGroundAdjustmentVelocity = Vector3.zero;
            _raycastSensor.castLength = isUsingExtendedSensorRange 
                ? _baseSensorRange + colliderHeight * transform.lossyScale.x * stepHeightRatio 
                : _baseSensorRange;
            _raycastSensor.Cast();

            _isGrounded = _raycastSensor.HasDetectedHit();
            if (!_isGrounded) return;

            float distanceToGround = _raycastSensor.GetDistance();
            float upperLimit = colliderHeight * transform.localScale.x * (1f - stepHeightRatio) * .5f;
            float middle = upperLimit + colliderHeight * transform.localScale.x * stepHeightRatio;
            float distanceToGo = middle - distanceToGround;

            _currentGroundAdjustmentVelocity = transform.up * (distanceToGo / Time.fixedDeltaTime);
        }

        public void SetVelocity(Vector3 velocity) => _rb.velocity = velocity + _currentGroundAdjustmentVelocity;

        private void Setup()
        {
            _tr = transform;
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _rb.freezeRotation = true;
            _rb.useGravity = false;
        }

        private void RecalculateColliderDimensions()
        {
            if (_collider == null) Setup();

            _collider.height = colliderHeight * (1f-stepHeightRatio);
            _collider.radius = colliderThickness * .5f;
            _collider.center = colliderOffset * colliderHeight + new Vector3(0f, _collider.height * .5f * stepHeightRatio, 0f);

            if (_collider.height * .5f < _collider.radius)
            {
                _collider.radius = _collider.height * .5f;
            }

            RecalibrateSensor();
        }

        private void RecalibrateSensor()
        {
            _raycastSensor ??= new RaycastSensor(_tr);

            _raycastSensor.SetCastOrigin(_collider.bounds.center);
            _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Down);
            RecalculateSensorLayerMask();

            const float safetyDistanceFactor = .001f; // Small factor added to prevent clipping issues when the ensor range is calculated

            float length = colliderHeight * (1f - stepHeightRatio) * .5f + colliderThickness + safetyDistanceFactor;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * _tr.localScale.x;
            _raycastSensor.castLength = length * transform.localScale.x;

        }

        private void RecalculateSensorLayerMask()
        {
            int objectLayer = gameObject.layer;
            int layerMask = Physics.AllLayers;

            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(objectLayer, i))
                {
                    layerMask = layerMask & ~(1 << i);
                }
            }

            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);

            _raycastSensor.layerMask = layerMask;
            currentLayer = objectLayer;
        }
    }
}
