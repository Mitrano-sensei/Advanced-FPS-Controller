using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    public class GroundMovement : MonoBehaviour
    {
        #region Fields
        private PlayerBody _playerBody;
        private PlayerController _playerController;

        #endregion    

        #region MonoBehaviour
        void Awake()
        {
            _playerBody = GetComponent<PlayerBody>();
            _playerController = GetComponent<PlayerController>();
        }
        #endregion

        internal void OnGroundEnter()
        {
            _playerBody.UseExtendedCastLength = true;
            _playerController.IsExitingClimb = false;
            _playerController.IsExitingCrouch = false;
            _playerController.SetupLerpToDefaultMoveSpeed();
        }

        internal void OnGroundExit()
        {
            _playerController.StartCoyoteeTimer();
            _playerBody.UseExtendedCastLength = false;
        }

        public class GroundedState : IState, IFPSState
        {
            public string Name => "Grounded State";
            private PlayerController _playerController;
            private GroundMovement _groundMovement;

            public GroundedState(PlayerController playerController, GroundMovement groundMovement)
            {
                _playerController = playerController;
                _groundMovement = groundMovement;
            }

            public float GetMovementSpeedRatio() => 1f;

            public void FixedUpdate()
            {
                _playerController.CalculateVelocity(GetMovementSpeedRatio());

            }

            public void OnEnter()
            {
                _groundMovement.OnGroundEnter();
            }

            public void OnExit()
            {
                _groundMovement.OnGroundExit();
            }

            public void Update()
            {
            }

        }
    }
}
