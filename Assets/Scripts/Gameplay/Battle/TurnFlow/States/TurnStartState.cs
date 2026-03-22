using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Types;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Initializes the turn and advances the flow to the hero phase.
    /// </summary>
    public sealed class TurnStartState : TurnBaseState
    {
        // Properties

        public override TurnStateType StateType => TurnStateType.TurnStart;

        // Constructor

        /// <summary>
        /// Creates a new instance of the turn start state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnStartState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // Public methods

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.TurnSystem, "TurnStartState entered. Preparing turn.");
            StateMachine.SwitchToHeroState();
        }
    }
}