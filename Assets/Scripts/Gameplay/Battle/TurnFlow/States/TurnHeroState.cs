// Gameplay/Battle/TurnFlow/States/TurnHeroState.cs

using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Battle.TurnFlow.Types;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Represents the hero action phase of the turn flow.
    /// During this state, puzzle input is enabled and puzzle results
    /// are interpreted as hero actions.
    ///
    /// Listens for AllEnemiesDefeatedEvent in addition to HeroTurnEndedEvent
    /// so the turn FSM can transition to victory when the last enemy dies
    /// during the hero's attack sequence, after the call stack has unwound.
    /// </summary>
    public sealed class TurnHeroState : TurnBaseState
    {
        // PROPERTIES

        public override TurnStateType StateType => TurnStateType.TurnHero;

        // CONSTRUCTOR

        /// <summary>Creates a new instance of the hero turn state.</summary>
        public TurnHeroState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.TurnSystem,
                "TurnHeroState entered. Enabling puzzle input and listening for puzzle events.");

            RegisterEvents();
        }

        public override void Exit()
        {
            UnregisterEvents();

            GameLogger.Log(LogSourceType.TurnSystem,
                "TurnHeroState exited. Disabling puzzle input and unsubscribing from puzzle events.");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            StateMachine.EventHub.HeroEvents.HeroTurnEndedEvent.Register(OnHeroTurnEnded);
            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent.Register(OnAllEnemiesDefeated);
        }

        private void UnregisterEvents()
        {
            StateMachine.EventHub.HeroEvents.HeroTurnEndedEvent.Unregister(OnHeroTurnEnded);
            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent.Unregister(OnAllEnemiesDefeated);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Hero consumed all AP without killing the last enemy.
        /// Advances to companion phase as normal.
        /// </summary>
        private void OnHeroTurnEnded(HeroTurnEndedPayload payload)
        {
            GameLogger.Log(LogSourceType.TurnSystem,
                "HeroTurnEnded received. Switching to companion phase.");

            StateMachine.SwitchToCompanionState();
        }

        /// <summary>
        /// All enemies were defeated during the hero's attack sequence.
        /// This event is raised by HeroAttackState.OnSequenceEnded after the
        /// attack call stack has fully unwound — safe to transition here.
        /// </summary>
        private void OnAllEnemiesDefeated(TurnAllEnemiesDefeatedPayload payload)
        {
            GameLogger.Log(LogSourceType.TurnSystem,
                "AllEnemiesDefeated received in TurnHeroState. Switching to victory.");

            StateMachine.SwitchToVictoryState();
        }
    }
}