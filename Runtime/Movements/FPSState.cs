using FiniteStateMachine;

namespace FPSController
{
    public class GroundState : IState
    {
        protected PlayerController _controller;

        public string Name => "Grounded State";

        public GroundState(PlayerController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnGroundedEnter();
        }

        public void OnExit()
        {
            _controller.OnGroundedExit();
        }

        public void FixedUpdate()
        {
            _controller.OnGroundedFixedUpdate();
        }

        public void Update()
        {
            _controller.OnGroundedUpdate();
        }
    }

    public class AirState : IState
    {
        protected PlayerController _controller;

        public string Name => "Air State";

        public AirState(PlayerController controller) 
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnAirEnter();
        }

        public void OnExit()
        {
            _controller.OnAirExit();
        }

        public void FixedUpdate()
        {
            _controller.OnAirFixedUpdate();
        }

        public void Update()
        {
            _controller.OnAirUpdate();
        }
    }

    public class JumpingState : IState
    {
        protected PlayerController _controller;

        public string Name => "Jumping State";

        public JumpingState(PlayerController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnJumpingEnter();
        }

        public void OnExit()
        {
            _controller.OnJumpingExit();
        }

        public void FixedUpdate()
        {
            _controller.OnJumpingFixedUpdate();
        }

        public void Update()
        {
            _controller.OnJumpingUpdate();
        }
    }
}
