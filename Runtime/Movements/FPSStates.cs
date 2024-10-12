using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    public interface IFPSState
    {
        float GetAirControlRatio();

    }

    public class GroundedState : IState, IFPSState
    {
        public string Name => "Grounded State";
        private PlayerController _playerController;

        public GroundedState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetAirControlRatio() => 1f; // TODO : Add to constructor so it can be changed with SerializeField

        public void FixedUpdate()
        {
            
        }

        public void OnEnter()
        {
            Debug.Log("Grounded State On Enter");
            _playerController.OnGroundEnter();
        }

        public void OnExit()
        {
            Debug.Log("Grounded State On Exit");
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

        public float GetAirControlRatio() => .8f;


        public void FixedUpdate()
        {
        }

        public void OnEnter()
        {
            Debug.Log("Jumping State On Enter");
            _playerController.OnJumpEnter();
        }

        public void OnExit()
        {
            Debug.Log("Jumping State On Exit");
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

        public float GetAirControlRatio() => .8f;

        public void FixedUpdate()
        {
        }

        public void OnEnter()
        {
            Debug.Log("Falling State On Enter");
        }

        public void OnExit()
        {
            Debug.Log("Falling State On Exit");
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

        public float GetAirControlRatio() => .8f;

        public void FixedUpdate()
        {
        }

        public void OnEnter()
        {
            Debug.Log("Rising State On Enter");
        }

        public void OnExit()
        {
            Debug.Log("Rising State On Exit");
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

        public float GetAirControlRatio() => 1f;

        public void FixedUpdate()
        {
        }

        public void OnEnter()
        {
            Debug.Log("Crouching State On Enter");
            _playerController.OnCrouchEnter();
        }

        public void OnExit()
        {
            Debug.Log("Crouching State On Exit");
            _playerController.OnCrouchExit();
        }

        public void Update()
        {
        }

    }

    public class SlidingState : IState, IFPSState
    {
        public string Name => "Sliding State";
        private PlayerController _playerController;

        public SlidingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public float GetAirControlRatio() => .3f;

        public void FixedUpdate()
        {
            _playerController.OnSlideFixedUpdate();
        }

        public void OnEnter()
        {
            Debug.Log("Sliding State On Enter");
            _playerController.OnSlideEnter();
        }

        public void OnExit()
        {
            Debug.Log("Sliding State On Exit");
            _playerController.OnSlideExit();
        }

        public void Update()
        {
        }
    }
}
