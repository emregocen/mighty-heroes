using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Enemy.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Reactive state entered when the enemy receives damage.
    ///
    /// Responsibilities:
    /// - Play hit reaction animation via Component.
    /// - Wait for EnemyHitReactionEndedSignal from animation behaviour.
    /// - Raise EnemyHitReactionEndedEvent so HeroAttackState can advance combo.
    /// - Transition to idle when reaction completes.
    ///
    /// This state does NOT apply damage — done in EnemyComponent.ReceiveDamage.
    /// This state does NOT check IsAlive — done in EnemyComponent.ReceiveDamage.
    /// If enemy is dead, SwitchToDefeatedState is called before this state is entered.
    ///
    /// Event raise ownership:
    /// EnemyHitReactionEndedEvent is raised here only.
    /// EnemyComponent.NotifyHitReactionEnded only forwards the signal to the FSM.
    /// </summary>
    public sealed class EnemyTakingHitState : EnemyBaseState
    {
        // FIELDS

        /// <summary>
        /// Direction from which the hit was received.
        /// Passed to the animator to select the correct reaction clip.
        /// </summary>
        private readonly HitDirection _hitDirection;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new taking-hit state.
        /// Damage must already be applied before entering this state.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        /// <param name="hitDirection">Direction of the incoming hit for animation.</param>
        public EnemyTakingHitState(EnemyStateMachine stateMachine, HitDirection hitDirection)
            : base(stateMachine)
        {
            _hitDirection = hitDirection;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Plays hit reaction animation.
        /// Damage is already applied; this state only drives the visual response.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Enemy,
                $"Enter EnemyTakingHitState Direction={_hitDirection}");

            StateMachine.Enemy.PlayTakeHit(_hitDirection);
        }

        /// <summary>
        /// No cleanup needed. Signal arrives via EnemyStateMachine.Notify.
        /// </summary>
        public override void Exit() { }

        /// <summary>
        /// Routes EnemyHitReactionEndedSignal to trigger the post-reaction transition.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            if (signal is EnemyHitReactionEndedSignal)
                OnHitReactionEnded();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Called when hit reaction animation completes.
        /// Raises EnemyHitReactionEndedEvent so HeroAttackState can advance its combo step,
        /// then transitions back to idle.
        /// </summary>
        private void OnHitReactionEnded()
        {
            GameLogger.Log(LogSourceType.Enemy,
                $"EnemyTakingHitState.OnHitReactionEnded EntityId={StateMachine.Enemy.EntityId}");

            StateMachine.EventHub.EnemyEvents
                .EnemyHitReactionEndedEvent
                .Raise(new EnemyHitReactionEndedPayload(StateMachine.Enemy.EntityId));

            StateMachine.SwitchToIdleState();
        }
    }
}