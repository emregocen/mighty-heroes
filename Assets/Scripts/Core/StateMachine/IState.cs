using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Core.StateMachine
{
    /// <summary>
    /// Contract for all FSM states.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state becomes active.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every tick while the state is active.
        /// </summary>
        void Tick();

        /// <summary>
        /// Called before transitioning away from the state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Handles incoming signals sent by other states or external events.
        /// </summary>
        void Handle(IStateSignal signal);

        /// <summary>
        /// Indicates whether this state is terminal.
        /// A terminal state cannot transition to any other state.
        /// </summary>
        bool IsTerminal { get; }
    }
}