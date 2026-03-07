using MightyHeroes.Core.StateMachine;
using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Base class for all enemy behavior states.
    ///
    /// Provides access to the owning enemy state machine
    /// and defines the standard state lifecycle methods.
    ///
    /// Concrete enemy states should inherit from this class
    /// and override only the methods they need.
    /// </summary>
    public abstract class EnemyBaseState : IState
    {
        /// <summary>
        /// Owning enemy state machine.
        /// Used to perform state transitions
        /// and access shared services.
        /// </summary>
        protected readonly EnemyStateMachine StateMachine;

        /// <summary>
        /// Creates a new enemy base state.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        protected EnemyBaseState(EnemyStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Indicates whether this state is terminal.
        /// Terminal states cannot transition to any other state.
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
        /// Enemy FSM logic should generally avoid heavy per-frame processing.
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