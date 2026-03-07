using MightyHeroes.Core.StateMachine;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Core.TurnSystem.Types;

namespace MightyHeroes.Core.TurnSystem.States
{
    /// <summary>
    /// Base class for all turn states.
    /// Provides access to the owning turn state machine and shared turn context.
    /// </summary>
    public abstract class TurnBaseState : IState, ITurnState
    {
        // Fields

        /// <summary>
        /// Reference to the owning turn state machine.
        /// </summary>
        protected readonly TurnStateMachine StateMachine;

        // Properties

        /// <summary>
        /// Gets the type identifier of this turn state.
        /// </summary>
        public abstract TurnStateType StateType { get; }

        /// <summary>
        /// Shared turn context containing runtime turn data.
        /// </summary>
        protected TurnContext Context => StateMachine.Context;

        // Constructor

        /// <summary>
        /// Creates a new instance of a turn state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        protected TurnBaseState(TurnStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Indicates whether this state is terminal. Terminal states cannot transition to any other state.
        /// Default implementation returns false. Override in derived states to specify terminal behavior.
        /// Terminal states should prevent any state transitions from occurring once entered.  
        /// </summary>
        public virtual bool IsTerminal => false;

        /// <summary>
        /// Called when the state becomes active.
        /// </summary>
        public virtual void Enter()
        {
        }

        /// <summary>
        /// Called every tick while the state is active.
        /// </summary>
        public virtual void Tick()
        {
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary> 
        /// Handles incoming signals sent by other states or external events.
        /// Default implementation does nothing.
        /// Override in derived states to handle specific signals as needed.
        /// </summary>
        public virtual void Handle(IStateSignal signal)
        {

        }
    }
}