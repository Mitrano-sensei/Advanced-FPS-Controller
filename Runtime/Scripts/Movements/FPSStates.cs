using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    public interface IFPSState
    {
        float GetMovementSpeedRatio();

        bool IsLimitedSpeed() => true; // Returns false if the state shouldn't be limited in speed
    }

    public class GroundedState : IState, IFPSState
    {
        public string Name => "Grounded State";
        private PlayerController _playerController;

        public GroundedState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetMovementSpeedRatio() => 1f;

        public void FixedUpdate()
        {
            _playerController.CalculateVelocity(GetMovementSpeedRatio());

        }

        public void OnEnter()
        {
            _playerController.OnGroundEnter();
        }

        public void OnExit()
        {
            _playerController.OnGroundExit();
        }

        public void Update()
        {
        }

    }

    public class JumpingState : IState, IFPSState
    {
        public string Name => "Jumping State";
        private PlayerController _playerController;

        public JumpingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetMovementSpeedRatio() => .8f;


        public void FixedUpdate()
        {
            _playerController.CalculateVelocity(GetMovementSpeedRatio());
        }

        public void OnEnter()
        {
            _playerController.OnJumpEnter();
        }

        public void OnExit()
        {
            _playerController.OnJumpExit();
        }

        public void Update()
        {
        }
    }

    public class  FallingState : IState, IFPSState
    {
        public string Name => "Falling State";
        private PlayerController _playerController;

        public FallingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetMovementSpeedRatio() => .8f;

        public void FixedUpdate()
        {
            _playerController.CalculateVelocity(GetMovementSpeedRatio());
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Update()
        {
        }
    }

    public class RisingState : IState, IFPSState
    {
        public string Name => "Rising State";
        private PlayerController _playerController;

        public RisingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetMovementSpeedRatio() => .8f;

        public void FixedUpdate()
        {
            _playerController.CalculateVelocity(GetMovementSpeedRatio());
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Update()
        {
        }
    }

    public class CrouchingState : IState, IFPSState
    {
        public string Name => "Crouching State";
        private PlayerController _playerController;

        public CrouchingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetMovementSpeedRatio() => .5f;

        public void FixedUpdate()
        {
            _playerController.CalculateVelocity(GetMovementSpeedRatio());
        }

        public void OnEnter()
        {
            _playerController.OnCrouchEnter();
        }

        public void OnExit()
        {
            _playerController.OnCrouchExit();
        }

        public void Update()
        {
        }

    }
}
