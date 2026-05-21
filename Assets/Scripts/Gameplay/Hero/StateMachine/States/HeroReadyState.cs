using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Puzzle.Payloads;
using Unity.VisualScripting;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// State where the hero is ready to receive puzzle input.
    /// Puzzle results are resolved into hero actions.
    /// </summary>
    public sealed class HeroReadyState : HeroBaseState
    {
        /// <summary>
        /// Creates a new hero ready state.
        /// </summary>
        public HeroReadyState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Enables puzzle input and subscribes to puzzle events.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroReadyState");

            StateMachine.Hero.SetPuzzlePlayable(true);
            StateMachine.Hero.SetCanChangeTarget(true);

            StateMachine.EventHub.PuzzleEvents
                .PuzzleTilesMatchedEvent
                .Register(OnPuzzleTilesMatched);
        }

        /// <summary>
        /// Disables puzzle input and unsubscribes from puzzle events.
        /// </summary>
        public override void Exit()
        {
            GameLogger.Log(LogSourceType.Hero, "Exit HeroReadyState");

            StateMachine.EventHub.PuzzleEvents
                .PuzzleTilesMatchedEvent
                .Unregister(OnPuzzleTilesMatched);

            StateMachine.Hero.SetCanChangeTarget(false);
            StateMachine.Hero.SetPuzzlePlayable(false);
        }

        /// <summary>
        /// Resolves puzzle results into hero actions and triggers state transitions.
        /// </summary>
        private void OnPuzzleTilesMatched(PuzzleTilesMatchedPayload payload)
        {
            HeroActionExecution execution =
                StateMachine.ActionResolver.Resolve(payload);

            if (execution.ActionType != ActionType.Attack)
                return;

            StateMachine.SwitchToAttackState(execution);
        }
    }
}