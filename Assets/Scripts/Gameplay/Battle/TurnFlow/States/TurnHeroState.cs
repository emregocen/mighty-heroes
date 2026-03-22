using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Types;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Battle.TurnFlow.States
{
    /// <summary>
    /// Represents the hero action phase of the turn flow.
    /// During this state, puzzle input is enabled and puzzle results are interpreted as hero actions.
    /// </summary>
    public sealed class TurnHeroState : TurnBaseState
    {
        // Properties

        public override TurnStateType StateType => TurnStateType.TurnHero;

        // Constructor

        /// <summary>
        /// Creates a new instance of the hero turn state.
        /// </summary>
        /// <param name="stateMachine">Owning turn state machine.</param>
        public TurnHeroState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // Public methods

        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.TurnSystem,
                "TurnHeroState entered. Enabling puzzle input and listening for puzzle events."
            );

            RegisterEvents();
        }

        public override void Exit()
        {
            UnregisterEvents();

            GameLogger.Log(
                LogSourceType.TurnSystem,
                "TurnHeroState exited. Disabling puzzle input and unsubscribing from puzzle events."
            );
        }

        // Private methods

        private void RegisterEvents()
        {
            StateMachine.EventHub.HeroEvents.HeroTurnEndedEvent.Register(OnHeroTurnEnded);
        }

        private void UnregisterEvents()
        {
            StateMachine.EventHub.HeroEvents.HeroTurnEndedEvent.Unregister(OnHeroTurnEnded);
        }

        // Event handlers

        private void OnHeroTurnEnded(HeroTurnEndedPayload payload)
        {
            GameLogger.Log(LogSourceType.TurnSystem, "HeroTurnEnded received. Switching to companion phase.");
            StateMachine.SwitchToCompanionState();
        }
    }
}