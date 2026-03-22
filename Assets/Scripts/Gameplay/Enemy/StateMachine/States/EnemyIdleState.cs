using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Payloads;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Passive idle state of the enemy.
    ///
    /// Enemy FSMs tick every frame but must not start turn-owned behavior
    /// unless TurnEnemyStartedEvent announces that THIS enemy is the active one.
    ///
    /// Responsibilities:
    /// - Listen for TurnEnemyStartedEvent and transition to ReadyState only
    ///   if the announced EnemyId matches this enemy.
    ///
    /// Hit reaction is NOT handled here.
    /// EnemyComponent.ReceiveDamage calls StateMachine.SwitchToTakingHitState directly,
    /// regardless of which state the enemy is currently in.
    /// Listening to EnemyDamagedEvent here would duplicate that transition.
    /// </summary>
    public sealed class EnemyIdleState : EnemyBaseState
    {
        // CONSTRUCTOR

        public EnemyIdleState(EnemyStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            RegisterEvents();
        }

        public override void Exit()
        {
            UnregisterEvents();
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            StateMachine.EventHub.TurnEvents
                .TurnEnemyStartedEvent
                .Register(OnTurnEnemyStarted);
        }

        private void UnregisterEvents()
        {
            StateMachine.EventHub.TurnEvents
                .TurnEnemyStartedEvent
                .Unregister(OnTurnEnemyStarted);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Received when TurnSystem announces which enemy should start acting.
        /// Transitions to ReadyState only if this enemy is the announced one.
        /// </summary>
        private void OnTurnEnemyStarted(TurnEnemyStartedPayload payload)
        {
            GameLogger.Log(LogSourceType.Enemy,
                $"EnemyIdleState.OnTurnEnemyStarted EntityId={StateMachine.Enemy.EntityId} PayloadId={payload.EnemyId}");

            if (payload.EnemyId != StateMachine.Enemy.EntityId)
                return;

            StateMachine.SwitchToReadyState();
        }
    }
}