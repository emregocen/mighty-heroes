using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Battle.TurnFlow.Types;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Terminal state entered when the hero dies.
    /// Raises TurnBattleDefeatedEvent through TurnStateMachine.OnStateEntered.
    /// Does not transition to any further state.
    /// </summary>
    public sealed class TurnDefeatedState : TurnBaseState
    {
        // FIELDS
        private readonly int _remainingEnemyCount;

        // PROPERTIES

        public override TurnStateType StateType => TurnStateType.BattleDefeated;

        /// <summary>
        /// Marks this state as terminal so BaseStateMachine blocks
        /// any stray SwitchState calls after defeat is reached.
        /// </summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        public TurnDefeatedState(TurnStateMachine stateMachine, int remainingEnemyCount)
            : base(stateMachine)
        {
            _remainingEnemyCount = remainingEnemyCount;
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.TurnSystem,
                $"TurnDefeatedState entered. RemainingEnemies={_remainingEnemyCount}. Battle halted.");

            Context.EndBattle();
        }

        /// <summary>Exposes remaining enemy count for payload construction in OnStateEntered.</summary>
        public int RemainingEnemyCount => _remainingEnemyCount;
    }
}