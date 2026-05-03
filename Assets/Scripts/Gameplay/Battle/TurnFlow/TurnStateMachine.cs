using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.StateMachine;
using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Battle.TurnFlow.States;
using MightyHeroes.Battle.TurnFlow.Types;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Core.Logging;

namespace MightyHeroes.Battle.TurnFlow
{
    /// <summary>
    /// State machine responsible for controlling the turn flow.
    /// Acts as the single authority for switching between turn phases.
    ///
    /// Enemy ordering is delegated to IFormationQuery so formation layout
    /// determines turn sequence — front line acts before back line.
    /// </summary>
    public sealed class TurnStateMachine : BaseStateMachine
    {
        // PROPERTIES

        /// <summary>Shared turn context containing runtime turn data.</summary>
        public TurnContext Context { get; }

        /// <summary>Shared event hub used for publishing turn-related events.</summary>
        public EventHub EventHub { get; }

        /// <summary>
        /// Gets the type of the currently active turn state.
        /// Returns None if no turn state is active.
        /// </summary>
        public TurnStateType CurrentStateType =>
            (CurrentState as ITurnState)?.StateType ?? TurnStateType.None;

        /// <summary>
        /// Formation line structure used by TurnEnemyState
        /// to build the front-to-back ordered enemy action sequence.
        /// </summary>
        public IFormation Formation { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new instance of the turn state machine.
        /// </summary>
        /// <param name="eventHub">Shared event hub instance.</param>
        /// <param name="context">Shared turn context.</param>
        /// <param name="formation">Formation query for ordered enemy turn sequencing.</param>
        public TurnStateMachine(EventHub eventHub, TurnContext context, IFormation formation)
        {
            EventHub = eventHub;
            Context = context;
            Formation = formation;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Starts the turn state machine.
        /// Must be called exactly once at battle start.
        /// </summary>
        public void Start()
        {
            Start(new TurnStartState(this));
        }

        /// <summary>Switches the flow to the beginning of a new turn.</summary>
        public void SwitchToStartState()
        {
            SwitchState(new TurnStartState(this));
        }

        /// <summary>Switches the flow to the hero action phase.</summary>
        public void SwitchToHeroState()
        {
            SwitchState(new TurnHeroState(this));
        }

        /// <summary>Switches the flow to the companion action phase.</summary>
        public void SwitchToCompanionState()
        {
            SwitchState(new TurnCompanionState(this));
        }

        /// <summary>Switches the flow to the enemy action phase.</summary>
        public void SwitchToEnemyState()
        {
            SwitchState(new TurnEnemyState(this));
        }

        /// <summary>Switches the flow to the turn end phase.</summary>
        public void SwitchToTurnEndState()
        {
            SwitchState(new TurnEndState(this));
        }

        /// <summary>Switches the flow to the victory state.</summary>
        public void SwitchToVictoryState()
        {
            SwitchState(new TurnVictoryState(this));
        }

        /// <summary>
        /// Switches the flow to the battle defeated terminal state.
        /// Called when the hero dies.
        /// </summary>
        public void SwitchToDefeatedState(int remainingEnemyCount)
        {
            SwitchState(new TurnDefeatedState(this, remainingEnemyCount));
        }

        // PROTECTED METHODS

        /// <summary>
        /// Called after a turn state has fully entered.
        /// Publishes semantic turn events for external systems.
        /// </summary>
        /// <param name="state">State that has just entered.</param>
        protected override void OnStateEntered(IState state)
        {
            if (state is not ITurnState turnState)
                return;

            switch (turnState.StateType)
            {
                case TurnStateType.TurnStart:
                    EventHub.TurnEvents.TurnStartedEvent.Raise(new TurnStartedPayload(Context.TurnIndex, Context.IsFirstTurn));
                    break;

                case TurnStateType.TurnHero:
                    EventHub.TurnEvents.TurnHeroPhaseStartedEvent.Raise(new TurnHeroPhaseStartedPayload());
                    break;

                case TurnStateType.TurnCompanion:
                    EventHub.TurnEvents.TurnCompanionPhaseStartedEvent.Raise(new TurnCompanionPhaseStartedPayload());
                    break;

                case TurnStateType.TurnEnemy:
                    EventHub.TurnEvents.TurnEnemyPhaseStartedEvent.Raise(new TurnEnemyPhaseStartedPayload());
                    break;

                case TurnStateType.TurnEnd:
                    EventHub.TurnEvents.TurnEndedEvent.Raise(
                        new TurnEndedPayload(Context.TurnIndex, Context.IsBattleEnded));
                    break;

                case TurnStateType.BattleVictory:
                    EventHub.TurnEvents.TurnBattleVictoryEvent.Raise(
                        new TurnBattleVictoryPayload(Context.TurnIndex));
                    break;

                case TurnStateType.BattleDefeated:
                    var defeatedState = state as TurnDefeatedState;
                    EventHub.TurnEvents.TurnBattleDefeatedEvent.Raise(
                        new TurnBattleDefeatedPayload(defeatedState?.RemainingEnemyCount ?? 0));
                    break;
            }
        }
    }
}