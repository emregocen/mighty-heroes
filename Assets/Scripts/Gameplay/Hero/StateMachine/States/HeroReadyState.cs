using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Puzzle.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// State where the hero is ready to receive puzzle input.
    /// Puzzle results are resolved into hero actions and routed to the
    /// matching state. Unmapped ActionTypes resolve to None and are ignored —
    /// the hero simply stays in this state waiting for another match.
    /// </summary>
    public sealed class HeroReadyState : HeroBaseState
    {
        public HeroReadyState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroReadyState");

            StateMachine.Hero.SetPuzzlePlayable(true);
            StateMachine.Hero.SetCanChangeTarget(true);

            StateMachine.EventHub.PuzzleEvents
                .PuzzleTilesMatchedEvent
                .Register(OnPuzzleTilesMatched);
        }

        public override void Exit()
        {
            GameLogger.Log(LogSourceType.Hero, "Exit HeroReadyState");

            StateMachine.EventHub.PuzzleEvents
                .PuzzleTilesMatchedEvent
                .Unregister(OnPuzzleTilesMatched);

            StateMachine.Hero.SetCanChangeTarget(false);
            StateMachine.Hero.SetPuzzlePlayable(false);
        }

        private void OnPuzzleTilesMatched(PuzzleTilesMatchedPayload payload)
        {
            HeroActionExecution execution = StateMachine.ActionResolver.Resolve(payload);

            switch (execution.ActionType)
            {
                case ActionType.Attack:
                    StateMachine.SwitchToAttackState(execution);
                    break;

                case ActionType.Heal:
                    StateMachine.SwitchToSupportState(execution);
                    break;

                default:
                    // None / unmapped — no AP spent, stay in ready state.
                    break;
            }
        }
    }
}