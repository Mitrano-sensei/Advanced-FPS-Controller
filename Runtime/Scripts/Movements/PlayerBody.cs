using UnityEngine;
using UnityEngine.Events;

namespace FPSController
{
    public class PlayerBody : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CapsuleCollider capsule;   // The player's collider
        [SerializeField] private Transform orientation;     // orientation with rotation following the camera
        [Space]

        [Header("Player Ground Checker Settings")]
        [SerializeField] private float bodyHeight = 1f;         // The height of the player's body, including the legs
        [SerializeField] private float legsHeightRatio = .5f;   // The ratio of the legs height compared to the body height
        [SerializeField] private float bodyRadius = .25f;       // The radius of the players body  
        [Space]
        [SerializeField] private LayerMask groundLayer;         // The layer that represents the ground
        [Space]
        [SerializeField] private float extendedGroundCheckRatio = 1.1f;
        [Space]
        [SerializeField] private float maxSlopeAngle = 40f;
        [Space] 
        [SerializeField] private float crouchModeRatio = .5f;

        
        public float BodyHeight => IsCrouching ? bodyHeight * crouchModeRatio : bodyHeight;
        public float LegsHeight => BodyHeight * legsHeightRatio;

        public bool IsCrouching { get; private set; }

        private RaycastHit _hit;

        public UnityAction OnGroundRetrieved = delegate { };
        public UnityAction OnGroundLost = delegate { };

        public bool IsGrounded { get; private set; }
        public bool UseExtendedGroundCheck { get; set; }
        public Vector3 CurrentSlopeNormal { get; private set; }

        public Vector3 Forward => CurrentSlopeNormal == Vector3.up ? orientation.forward : Vector3.ProjectOnPlane(orientation.forward, CurrentSlopeNormal).normalized;
        public Vector3 Right => CurrentSlopeNormal == Vector3.up ? orientation.right : Vector3.ProjectOnPlane(orientation.right, CurrentSlopeNormal).normalized;
        
        public Vector3 SlopeDirection => CurrentSlopeNormal == Vector3.up ? Vector3.zero : Vector3.ProjectOnPlane(Vector3.down, CurrentSlopeNormal).normalized;
        public float CurrentSlopeAngle => Vector3.Angle(Vector3.up, CurrentSlopeNormal);
        public float MaxSlopeAngle => maxSlopeAngle;
        

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

            capsule.height = BodyHeight * (1f - legsHeightRatio);
            capsule.center = new Vector3(0, LegsHeight + capsule.height * .5f, 0);
            capsule.radius = bodyRadius;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.blue;
            Gizmos.DrawLine(transform.position + capsule.center, transform.position + capsule.center + Vector3.down * (LegsHeight + capsule.height*.5f));
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + SlopeDirection);
        }

        private void CheckGround(bool useExtendedGroundCheck)
        {
            bool lastIsGrounded = IsGrounded;

            Vector3 origin = transform.position + capsule.center;
            Vector3 direction = Vector3.down;
            float distance = LegsHeight + capsule.height*.5f;

            if (useExtendedGroundCheck) distance *= extendedGroundCheckRatio * (IsCrouching ? 2f : 1f);

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
            var rayDistance = (LegsHeight + capsule.height * .5f);
            var delta = rayDistance - distance;

            return delta;
        }

        public void SetCrouchMode(bool isCrouching)
        {
            IsCrouching = isCrouching;
            
            capsule.height = BodyHeight;
            capsule.center = new Vector3(0, LegsHeight + capsule.height * .5f, 0);
        }
    }
}
