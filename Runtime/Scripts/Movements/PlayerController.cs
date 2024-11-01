using FiniteStateMachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPSController
{
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private FPSInputReader inputReader;
        [SerializeField] private PlayerGroundChecker groundChecker;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform orientation;

        [Header("States")]
        [SerializeField] private GroundState groundState;
        [SerializeField] private JumpingState jumpingState;
        [SerializeField] private FallingState fallingState;
        [SerializeField] private RisingState risingState;
        [SerializeField] private ClimbingState climbingState;

        // State Machine
        private StateMachine _stateMachine;

        public IState CurrentState => _stateMachine.CurrentState;
        public IFPSState CurrentFPSState => (IFPSState)CurrentState;

        #endregion

        #region MonoBehaviour
        void Awake()
        {
            if (inputReader == null) Debug.LogError("InputReader is not assigned in PlayerController");
            if (groundChecker == null) Debug.LogError("GroundChecker is not assigned in PlayerController");
            if (rb == null) Debug.LogError("Rigidbody not assigned in PlayerController");
        }

        void Start()
        {
            InitStates();
            SetupStateMachine();
        }

        void Update()
        {
            _stateMachine.Update();

            inputReader.UpdateJumpInputs();
        }

        void FixedUpdate()
        {
            _stateMachine.FixedUpdate();

            HandleCurrentState();
        }

        #endregion

        #region StateMachine
        private void SetupStateMachine()
        {
            _stateMachine = new();

            At(fallingState, groundState, () => groundChecker.IsGrounded);

            At(groundState, fallingState, () => !groundChecker.IsGrounded && rb.velocity.y <= 0f);
            At(groundState, risingState, () => !groundChecker.IsGrounded && rb.velocity.y > 0f);

            At(risingState, fallingState, () => !groundChecker.IsGrounded && rb.velocity.y < 0f);
            At(risingState, groundState, () => groundChecker.IsGrounded);

            At(groundState, jumpingState, () => jumpingState.IsJumpingEnter());

            At(jumpingState, groundState, () => groundChecker.IsGrounded && !inputReader.JumpKeyIsLocked);
            At(jumpingState, risingState, () => jumpingState.IsJumpingExit() && rb.velocity.y > 0f);
            At(jumpingState, fallingState, () => rb.velocity.y < 0f);

            At(fallingState, jumpingState, () => jumpingState.IsJumpingEnter());
            
            At(jumpingState, climbingState, () => climbingState.IsClimbingEnter());
            At(risingState, climbingState, () => climbingState.IsClimbingEnter());
            At(fallingState, climbingState, () => climbingState.IsClimbingEnter());

            At(climbingState, risingState, () => climbingState.IsClimbingExit());

            _stateMachine.SetState(fallingState);
        }

        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, new FuncPredicate(condition));

        private void InitStates()
        {
            List<IFPSState> states = new List<IFPSState>() { groundState, jumpingState, risingState, fallingState, climbingState };

            foreach (var state in states)
            {
                state.SetPlayerController(this);
                state.SetRigidbody(rb);
                state.SetGroundChecker(groundChecker);
                state.SetInputReader(inputReader);
            }
        }
        #endregion

        private void HandleCurrentState()
        {
            CurrentFPSState.HandleGravity();
            rb.drag = CurrentFPSState.GetDrag();
            CurrentFPSState.HandleMovementInputs();
            CurrentFPSState.HandleLimitSpeed();
        }

        #region Debug

        public void PrintState()
        {
            Debug.Log("Current State : " + CurrentState.Name);
        }
        #endregion
    }

    public interface IFPSState : IState
    {
        void SetPlayerController(PlayerController playerController);
        void SetRigidbody(Rigidbody rb);
        void SetGroundChecker(PlayerGroundChecker groundChecker);
        void SetInputReader(FPSInputReader inputReader);

        /**
         * Gets the drag that should be set for sthe rigidbody
         */
        float GetDrag();
        
        // Movement State Loop : The following implementations of the Current State will be called in PlayerController.FixedUpdate
        
        /**
         * Handle Gravity. Base Rigidbody gravity should be turned off
         */
        void HandleGravity();
        
        /**
         * Handles the movements following the direction inputs of the player. Note that inputs corresponding to complex moves as jump impulse or wall sticking should be handled in the state
         */
        void HandleMovementInputs() {}
        
        /**
         * Handles the limitation of the player's speed.
         */
        void HandleLimitSpeed() {}
    }
}
