using MightyHeroes.Core.Logging;
using MightyHeroes.Core.TurnSystem.Types;

namespace MightyHeroes.Core.TurnSystem.States
{
    /// <summary>
    /// Finalizes the turn and advances the flow to the next turn start.
    /// </summary>
    public sealed class TurnEndState : TurnBaseState
    {
        // Properties

        public override TurnStateType StateType => TurnStateType.TurnEnd;

        // Constructor

        /// <summary>
        /// Creates a new instance of the turn end state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnEndState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // Public methods

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.TurnSystem, "TurnEndState entered. Finalizing turn.");

            Context.AdvanceTurn();

            if (Context.IsBattleEnded)
            {
                GameLogger.Log(LogSourceType.TurnSystem,
                    "TurnEndState: Battle ended. Halting turn loop.");
                return;
            }

            StateMachine.SwitchToStartState();
        }
    }
}