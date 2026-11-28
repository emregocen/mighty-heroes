using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// State entered after every attack sequence completes.
    /// Waits for HeroRetreatEndedSignal, which is routed through
    /// HeroComponent → HeroStateMachine from the controller's RetreatCompleted
    /// event. This state has no direct knowledge of the movement controller.
    /// Victory is not handled here; HeroStateMachine switches to victory on
    /// TurnBattleVictoryEvent and cancels the retreat itself.
    /// </summary>
    public sealed class HeroRetreatState : HeroBaseState
    {
        // CONSTRUCTOR

        /// <summary>Creates a new hero retreat state.</summary>
        public HeroRetreatState(HeroStateMachine stateMachine) : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Disables puzzle input and target selection for the retreat duration.
        /// Movement is already running; the controller started it before the FSM
        /// transitioned here.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroRetreatState: Enter");

            StateMachine.Hero.SetPuzzlePlayable(false);
            StateMachine.Hero.SetCanChangeTarget(false);
        }

        /// <summary>
        /// Routes HeroRetreatEndedSignal into the next transition.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            if (signal is HeroRetreatEndedSignal)
                OnRetreatEnded();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Transitions to ReadyState if AP remains, otherwise ends the hero turn.
        /// </summary>
        private void OnRetreatEnded()
        {
            if (!StateMachine.Hero.HasRemainingActionPoints)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroRetreatState: No AP remaining, finishing turn.");
                StateMachine.SwitchToIdleState();
                StateMachine.FinishHeroTurn();
                return;
            }

            GameLogger.Log(LogSourceType.Hero, "HeroRetreatState: Returning to ready.");
            StateMachine.SwitchToReadyState();
        }
    }
}
