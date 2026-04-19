using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Default hero state when the hero is not actively performing an action.
    ///
    /// Responsibilities:
    /// - Dims the AP view on entry to signal the hero turn is not active.
    /// - Listens for TurnHeroPhaseStartedEvent and transitions to PreReadyState.
    ///
    /// Hit reaction is NOT handled here.
    /// HeroComponent.OnDamageIntent calls StateMachine.SwitchToTakingHitState directly,
    /// regardless of which state the hero is currently in.
    /// </summary>
    public sealed class HeroIdleState : HeroBaseState
    {
        // CONSTRUCTOR

        /// <summary>
        /// Creates a new hero idle state.
        /// </summary>
        public HeroIdleState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Dims the AP view and registers turn event listener.
        /// AP view is dimmed here because idle is the resting state between turns.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroIdleState");
            StateMachine.Hero.SetActionPointViewTurnActive(false);
            RegisterEvents();
        }

        /// <summary>
        /// Unregisters turn event listener.
        /// </summary>
        public override void Exit()
        {
            GameLogger.Log(LogSourceType.Hero, "Exit HeroIdleState");
            UnregisterEvents();
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            StateMachine.EventHub.TurnEvents
                .TurnHeroPhaseStartedEvent
                .Register(OnTurnHeroStarted);
        }

        private void UnregisterEvents()
        {
            StateMachine.EventHub.TurnEvents
                .TurnHeroPhaseStartedEvent
                .Unregister(OnTurnHeroStarted);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Called when the hero turn phase starts.
        /// Transitions to PreReadyState which owns AP reset and delay.
        /// </summary>
        private void OnTurnHeroStarted(TurnHeroPhaseStartedPayload payload)
        {
            StateMachine.SwitchToPreReadyState();
        }
    }
}