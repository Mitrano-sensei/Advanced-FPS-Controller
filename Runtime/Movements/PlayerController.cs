using System;
using UnityEngine;
using Utilities;

namespace FPSController
{
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [SerializeField] BaseFPSInputReader input;

        Transform _tr;
        PlayerMover _mover;
        // CeilingDetector _ceilingDetector;

        bool _jumpInputIsLocked, _jumpKeyWasPressed, _jumpKeyWasReleased, _jumpKeyIsPressed;

        [SerializeField] float movementSpeed = 7f;
        [SerializeField] float airControlRate = 2f;
        [SerializeField] float jumpSpeed = 10f;
        [SerializeField] float jumpDuration = .2f;
        [SerializeField] float airFriction = .5f;
        [SerializeField] float groundFriction = 100f;
        [SerializeField] float gravity = 30f;
        [SerializeField] float slideGravity = 5f;
        [SerializeField] float slopeLimit = 30f;
        [SerializeField] bool useLocalMomentum;

        FiniteStateMachine.StateMachine _stateMachine;
        CountdownTimer jumpTimer;

        [SerializeField] Transform cameraTransform;

        Vector3 _momentum, _savedVelocity, _savecMovementVelocity;

        public event Action<Vector3> OnJump = delegate { };
        public event Action<Vector3> OnLand = delegate { };
        #endregion

        private void Awake()
        {
            _tr = transform;
            _mover = GetComponent<PlayerMover>();
            // _ceilingDetector = GetComponent<CeilingDetector>();

            jumpTimer = new CountdownTimer(jumpDuration);
            // SetupStateMachine();
        }

        public Vector3 GetMomentum() => useLocalMomentum ? _tr.localToWorldMatrix * _momentum : _momentum;

        private void FixedUpdate()
        {
            _mover.CheckForGround();
            HandleMomentum();
        }

        void HandleMomentum()
        {
            _momentum = GetMomentum();

            Vector3 verticalMomentum = VectorHelpers.ExtractDotVector(_momentum, _tr.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;

            verticalMomentum -= _tr.up * gravity * Time.deltaTime;
            if (_stateMachine.CurrentState is GroundState && VectorHelpers.GetDotProduct(verticalMomentum, _tr.up) < 0f)
            {
                verticalMomentum = Vector3.zero;
            }

            if (!IsGrounded())
            {
                Vector3 movementVelocity = CalculateMovementVelocity();
            }

        }

        bool IsGrounded() => _stateMachine.CurrentState is GroundState or SlidingState;
        Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        Vector3 CalculateMovementDirection()
        {
            Vector3 direction = _tr.right * input.Direction.x + _tr.forward * input.Direction.y;
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
    }
}
