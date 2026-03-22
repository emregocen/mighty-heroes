using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Types;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Handles companion actions during the turn if companions are present.
    /// Waits for companion actions to complete before advancing the turn.
    /// </summary>
    public sealed class TurnCompanionState : TurnBaseState
    {
        // Properties

        public override TurnStateType StateType => TurnStateType.TurnCompanion;

        // Constructor

        /// <summary>
        /// Creates a new instance of the companion turn state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnCompanionState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // Public methods

        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.TurnSystem,
                $"TurnCompanionState entered. HasCompanions={Context.HasCompanions}"
            );

            if (!Context.HasCompanions)
            {
                StateMachine.SwitchToEnemyState();
                return;
            }

            // Companion exists:
            // - Enable companion AI
            // - Subscribe to companion action completed event
        }

        public override void Exit()
        {
            GameLogger.Log(LogSourceType.TurnSystem, "TurnCompanionState exited.");

            // Example responsibilities:
            // - Disable companion AI
            // - Unsubscribe from companion events
        }
    }
}