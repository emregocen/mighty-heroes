using MightyHeroes.Battle.TurnFlow.Types;
using MightyHeroes.Core.Logging;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// First state entered when the turn state machine starts.
    ///
    /// Purely a wait state — it does not know about BattleIntroController or
    /// any intro sequencing. BattleBootstrapper owns both the intro controller
    /// and the turn state machine directly, so it calls SwitchToStartState
    /// on the turn state machine once the intro completes, without this state
    /// needing to be aware of what triggered the transition.
    ///
    /// Entered exactly once per battle. Never re-entered.
    /// </summary>
    public sealed class TurnIntroState : TurnBaseState
    {
        // PROPERTIES

        public override TurnStateType StateType => TurnStateType.TurnIntro;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new intro wait state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnIntroState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                "TurnIntroState: Enter — waiting for BattleBootstrapper to advance the turn flow.");
        }

        public override void Exit()
        {
        }
    }
}