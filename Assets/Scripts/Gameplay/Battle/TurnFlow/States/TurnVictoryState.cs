using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Types;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Terminal state entered when all enemies are defeated.
    /// Raises BattleVictoryEvent through TurnStateMachine.OnStateEntered.
    /// Does not transition to any further state.
    /// The state machine halts here for the remainder of the battle lifetime.
    /// </summary>
    public sealed class TurnVictoryState : TurnBaseState
    {
        // PROPERTIES

        public override TurnStateType StateType => TurnStateType.BattleVictory;

        /// <summary>
        /// Marks this state as terminal so BaseStateMachine blocks
        /// any further SwitchState calls originating from stray events.
        /// </summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new instance of the turn victory state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnVictoryState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.TurnSystem,
                $"TurnVictoryState entered. FinalTurnIndex={Context.TurnIndex}. Battle halted."
            );

            // Context is marked ended so TurnEndState's loop-back guard
            // also catches this path if a race condition occurs.
            Context.EndBattle();
        }
    }
}