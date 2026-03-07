using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Reaction state entered when the hero receives damage.
    ///
    /// Responsibilities:
    /// - Play hit reaction animation via Component (Unity boundary).
    /// - Wait for HeroHitReactionEndedSignal forwarded by HeroStateMachine.
    /// - Raise HeroHitReactionEndedEvent so EnemyAttackState can advance its combo.
    /// - Transition to idle state when reaction is complete.
    ///
    /// This state does NOT apply damage. Damage is applied upstream in
    /// HeroComponent.ReceiveDamage before this state is entered.
    ///
    /// This state does NOT check IsAlive. That check is done in HeroComponent.ReceiveDamage.
    /// If the hero is dead, HeroStateMachine.SwitchToDefeatedState is called instead
    /// and this state is never entered.
    ///
    /// Event raise ownership:
    /// HeroHitReactionEndedEvent is raised here, in OnHitReactionEnded.
    /// HeroComponent.NotifyHitReactionEnded only forwards the signal to the FSM.
    /// Never raise the same event in both places.
    /// </summary>
    public sealed class HeroTakingHitState : HeroBaseState
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
        /// <param name="stateMachine">Owning hero state machine.</param>
        /// <param name="hitDirection">Direction of the incoming hit for animation.</param>
        public HeroTakingHitState(HeroStateMachine stateMachine, HitDirection hitDirection)
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
            GameLogger.Log(LogSourceType.Hero,
                $"Enter HeroTakingHitState Direction={_hitDirection}");

            StateMachine.Hero.PlayTakeHit(_hitDirection);
        }

        /// <summary>
        /// No cleanup needed. Signal arrives via HeroStateMachine.Notify,
        /// no direct EventHub subscription in this state.
        /// </summary>
        public override void Exit() { }

        /// <summary>
        /// Routes HeroHitReactionEndedSignal to trigger the post-reaction transition.
        /// Signal is forwarded by HeroStateMachine.NotifyHitReactionEnded.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            if (signal is HeroHitReactionEndedSignal)
                OnHitReactionEnded();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Called when hit reaction animation completes.
        /// Raises HeroHitReactionEndedEvent so EnemyAttackState can advance its combo step,
        /// then transitions back to idle to await the next action.
        /// </summary>
        private void OnHitReactionEnded()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"HeroTakingHitState.OnHitReactionEnded EntityId={StateMachine.Hero.EntityId}");

            StateMachine.EventHub.HeroEvents
                .HeroHitReactionEndedEvent
                .Raise(new HeroHitReactionEndedPayload(StateMachine.Hero.EntityId));

            StateMachine.SwitchToIdleState();
        }
    }
}