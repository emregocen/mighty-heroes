using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Default hero state when the hero is not actively performing an action.
    ///
    /// Responsibilities:
    /// - Listen for TurnHeroPhaseStartedEvent and transition to ReadyState.
    ///
    /// Hit reaction is NOT handled here.
    /// HeroComponent.ReceiveDamage calls StateMachine.SwitchToTakingHitState directly,
    /// regardless of which state the hero is currently in.
    /// Listening to HeroDamagedEvent here would duplicate that transition.
    /// </summary>
    public sealed class HeroIdleState : HeroBaseState
    {
        // CONSTRUCTOR

        public HeroIdleState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroIdleState");

            RegisterEvents();
        }

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
        /// Resets action points and transitions to ReadyState to enable puzzle input.
        /// </summary>
        private void OnTurnHeroStarted(TurnHeroPhaseStartedPayload payload)
        {
            StateMachine.Hero.ResetActionPoints();
            StateMachine.SwitchToReadyState();
        }
    }
}