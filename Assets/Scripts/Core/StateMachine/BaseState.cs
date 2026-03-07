using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Core.StateMachine
{
    /// <summary>
    /// Minimal base implementation for states.
    ///
    /// Why this exists:
    /// - Prevents repeating boilerplate (Tick, IsTerminal, empty Handle)
    /// - Enables shared abstract states (e.g., ComboAttackState)
    /// - Keeps IState as the primary contract
    /// </summary>
    public abstract class BaseState : IState
    {
        /// <summary>
        /// Indicates whether this state is terminal.
        /// Override if you have a terminal/dead state.
        /// </summary>
        public virtual bool IsTerminal => false;

        /// <summary>
        /// Called when the state becomes active.
        /// Must be implemented by concrete states.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Called every tick while the state is active.
        /// Most of our states are event-driven, so default is no-op.
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Called before transitioning away from the state.
        /// Must be implemented by concrete states.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// Handles incoming signals.
        /// Default is no-op; override where needed.
        /// </summary>
        public virtual void Handle(IStateSignal signal) { }
    }
}