using FiniteStateMachine;
using UnityEngine;

namespace FPSController
{
    public class GroundedState : IState
    {
        public string Name => "Grounded State";
        private PlayerController _playerController;

        public GroundedState(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void FixedUpdate()
        {
            
        }

        public void OnEnter()
        {
            Debug.Log("Grounded State On Enter");
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

    public class JumpingState : IState
    {
        public string Name => "Jumping State";
        private PlayerController _playerController;

        public JumpingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

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

    public class  FallingState : IState
    {
        public string Name => "Falling State";
        private PlayerController _playerController;

        public FallingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

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

    public class RisingState : IState
    {
        public string Name => "Rising State";
        private PlayerController _playerController;

        public RisingState(PlayerController playerController)
        {
            _playerController = playerController;
        }

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
}
