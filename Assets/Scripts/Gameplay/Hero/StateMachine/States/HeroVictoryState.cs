using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;
using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Terminal state entered when all enemies are defeated.
    ///
    /// Phase 1 — Victory sequence: plays the victory clip via Victory trigger.
    /// Phase 2 — Turn: plays the turn clip, camera closes in simultaneously.
    /// Phase 3 — Idle: plays the victory idle loop, triggers UI fade-in.
    ///
    /// Transitions are driven by AnimatorBehaviour signals, not timers.
    /// Camera transition is triggered at the start of Phase 2.
    /// UI fade-in is triggered at the start of Phase 3.
    /// </summary>
    public sealed class HeroVictoryState : HeroBaseState
    {
        // PROPERTIES

        /// <summary>
        /// Marks this state as terminal so BaseStateMachine blocks
        /// any stray SwitchState calls after victory is reached.
        /// </summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        /// <summary>Creates a new hero victory state.</summary>
        public HeroVictoryState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Plays the victory animation. Phase 1 begins here.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroVictoryState: Enter. Starting victory sequence.");
            StateMachine.Hero.PlayVictoryAnimation();
        }

        /// <summary>
        /// Routes behaviour signals into the appropriate victory phase handler.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case HeroVictorySequenceEndedSignal:
                    OnVictorySequenceEnded();
                    break;

                case HeroVictoryTurnEndedSignal:
                    OnVictoryTurnEnded();
                    break;
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Phase 2: Victory clip finished. Plays turn animation and triggers camera transition.
        /// </summary>
        private void OnVictorySequenceEnded()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroVictoryState: Phase 2 - Turn begins.");

            StateMachine.EventHub.HeroEvents.HeroVictoryTurnStartedEvent
                .Raise(new HeroVictoryTurnStartedPayload());
        }

        /// <summary>
        /// Phase 3: Turn clip finished. Plays victory idle loop and triggers UI fade-in.
        /// </summary>
        private void OnVictoryTurnEnded()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroVictoryState: Phase 3 - Victory idle begins.");

            StateMachine.EventHub.HeroEvents.HeroVictoryIdleStartedEvent
                .Raise(new HeroVictoryIdleStartedPayload());
        }
    }
}