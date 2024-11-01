using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace FPSController
{
    public class PlayerGroundChecker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CapsuleCollider capsule;   // The player's collider
        [SerializeField] private Transform orientation;     // orientation with rotation following the camera
        [Space]

        [Header("Player Ground Checker Settings")]
        [SerializeField] private float bodyHeight = 1f;     // The height of the player's body, excluding the legs
        [SerializeField] private float legsHeight = .5f;    // The distance between the player's body and the ground
        [SerializeField] private float bodyRadius = .25f;   // The radius of the players body  
        [Space]
        [SerializeField] private LayerMask groundLayer;     // The layer that represents the ground
        [Space]
        [SerializeField] private float extendedGroundCheckRatio = 1.1f;
        [Space]
        [SerializeField] private float maxSlopeAngle = 40f;

        private RaycastHit _hit;

        public UnityAction OnGroundRetrieved = delegate { };
        public UnityAction OnGroundLost = delegate { };

        public bool IsGrounded { get; private set; }
        public bool UseExtendedGroundCheck { get; set; }
        public Vector3 CurrentSlopeNormal { get; private set; }

        public Vector3 Forward => CurrentSlopeNormal == Vector3.up ? orientation.forward : Vector3.ProjectOnPlane(orientation.forward, CurrentSlopeNormal).normalized;
        public Vector3 Right => CurrentSlopeNormal == Vector3.up ? orientation.right : Vector3.ProjectOnPlane(orientation.right, CurrentSlopeNormal).normalized;

        void FixedUpdate()
        {
            CheckGround(UseExtendedGroundCheck);
        }

        void OnValidate()
        {
            if (capsule == null)
            {
                capsule = GetComponent<CapsuleCollider>();
                Debug.LogWarning("Capsule Collider not assigned, automatically assigned to the component.");
            }

            capsule.height = bodyHeight;
            capsule.radius = bodyRadius;
            capsule.center = new Vector3(0, legsHeight + bodyHeight*.5f, 0);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.blue;
            Gizmos.DrawLine(transform.position + capsule.center, transform.position + capsule.center + Vector3.down * (legsHeight + bodyHeight*.5f));
        }

        private void CheckGround(bool useExtendedGroundCheck)
        {
            bool lastIsGrounded = IsGrounded;

            Vector3 origin = transform.position + capsule.center;
            Vector3 direction = Vector3.down;
            float distance = legsHeight + bodyHeight*.5f;

            if (useExtendedGroundCheck) distance *= extendedGroundCheckRatio;

            IsGrounded = Physics.Raycast(origin, direction, out _hit, distance, groundLayer);
            CurrentSlopeNormal = _hit.collider != null ? _hit.normal : Vector3.up;

            var currentSlopeAngle = Vector3.Angle(Vector3.up, CurrentSlopeNormal);
            if (currentSlopeAngle > maxSlopeAngle)
                CurrentSlopeNormal = Vector3.up;

            if (IsGrounded && !lastIsGrounded)
                OnGroundRetrieved?.Invoke();
            else if (!IsGrounded && lastIsGrounded)
                OnGroundLost?.Invoke();

        }

        public float GetGroundAdjustment()
        {
            CheckGround(true);

            if (_hit.collider == null)
                return 0;

            var distance = _hit.distance;
            var delta = (legsHeight + bodyHeight * .5f) - distance;

            return delta;
        }
    }
}
