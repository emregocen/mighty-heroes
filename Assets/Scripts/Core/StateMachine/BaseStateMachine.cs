using System;
using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Core.StateMachine
{
    /// <summary>
    /// Base finite state machine implementation.
    /// Manages state lifecycle and transitions.
    /// This class does not carry gameplay data.
    /// Data flow is handled externally via context objects and events.
    /// </summary>
    public abstract class BaseStateMachine
    {
        /// <summary>
        /// Currently active state instance.
        /// </summary>
        private IState _currentState;

        /// <summary>
        /// Indicates whether the state machine has been started.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Gets the currently active state.
        /// Accessible by derived state machines.
        /// </summary>
        protected IState CurrentState => _currentState;

        /// <summary>
        /// Indicates whether the current state is terminal.
        /// Terminal states cannot transition to any other state.
        /// </summary>
        public virtual bool IsTerminal => false;

        /// <summary>
        /// Starts the state machine with the given initial state.
        /// This method must be called exactly once.
        /// </summary>
        /// <param name="initialState">Initial state to enter.</param>
        public void Start(IState initialState)
        {
            if (_isActive)
                throw new InvalidOperationException("StateMachine is already started.");

            if (initialState == null)
                throw new ArgumentNullException(nameof(initialState));

            _isActive = true;
            _currentState = initialState;

            GameLogger.Log(
                LogSourceType.StateMachine,
                $"FSM started with state {initialState.GetType().Name}"
            );

            _currentState.Enter();

            // IMPORTANT:
            // This hook is invoked AFTER the state has fully entered.
            // Derived state machines can react safely here.
            OnStateEntered(_currentState);
        }

        /// <summary>
        /// Advances the currently active state.
        /// Should be called once per frame by a Unity-facing controller.
        /// </summary>
        public void Tick()
        {
            if (!_isActive)
                return;

            _currentState?.Tick();
        }

        /// <summary>
        /// Switches to the specified next state.
        /// Handles proper exit and enter lifecycle calls.
        /// </summary>
        /// <param name="nextState">Next state to activate.</param>
        protected void SwitchState(IState nextState)
        {
            // Guard against null next state.
            if (nextState == null)
                throw new ArgumentNullException(nameof(nextState));
                
            // Guard against switching from a terminal state.
            if (_currentState != null && _currentState.IsTerminal)
                return;

            _currentState?.Exit();
            _currentState = nextState;

            GameLogger.Log(
                LogSourceType.StateMachine,
                $"FSM switched to state {nextState.GetType().Name}"
            );

            _currentState.Enter();

            // IMPORTANT:
            // Ensures the new state is fully initialized
            // before any derived logic is triggered.
            OnStateEntered(_currentState);
        }

        /// <summary>
        /// Stops the state machine and exits the current state.
        /// </summary>
        public void Stop()
        {
            if (!_isActive)
                return;

            _currentState?.Exit();
            _currentState = null;
            _isActive = false;
        }

        /// <summary>
        /// Sends a signal to the currently active state for handling.
        /// Used to route messages from external events or other states.
        /// </summary>
        /// <param name="signal"></param>
        public void Notify(IStateSignal signal)
        {
            _currentState?.Handle(signal);
        }

        /// <summary>
        /// Called after a state has fully entered.
        /// Derived state machines override this to react to state changes.
        /// </summary>
        /// <param name="state">State that has just entered.</param>
        protected virtual void OnStateEntered(IState state)
        {
        }
    }
}