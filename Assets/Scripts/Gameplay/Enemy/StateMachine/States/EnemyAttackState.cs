using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.StateMachine.Base;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Enemy.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Active attack state for an enemy during a combo sequence.
    ///
    /// Responsibilities:
    /// - Handles incoming signals from animation behaviours and FSM-forwarded events.
    /// - Delegates combo step progression to ComboAttackState base class.
    /// - Raises EnemyAttackHit events for each hit frame so CombatSystem can resolve damage.
    /// - Decides whether to return to ReadyState (AP remaining) or finish the enemy turn.
    ///
    /// Signal routing:
    /// - EnemyAttackHitFrameSignal         : forwarded by EnemyStateMachine from AnimationEventProxy
    /// - EnemyAttackClipEndedSignal        : forwarded by EnemyStateMachine from behaviour
    /// - EnemyTargetHitReactionEndedSignal : forwarded by EnemyStateMachine from EventHub
    /// - EnemyCombatHitResolvedSignal      : forwarded by EnemyStateMachine from EventHub
    /// - EnemyAttackSequenceEndedSignal    : forwarded by EnemyStateMachine from behaviour
    /// </summary>
    public sealed class EnemyAttackState : ComboAttackState<EnemyStateMachine>
    {
        // ------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a new enemy attack state for the given combo configuration.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        /// <param name="attackVariant">Animation variant index for the attack.</param>
        /// <param name="attackIntensity">Intensity level, also defines combo step count.</param>
        public EnemyAttackState(
            EnemyStateMachine stateMachine,
            int attackVariant,
            int attackIntensity)
            : base(stateMachine, attackVariant, attackIntensity)
        {
        }

        // ------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------

        /// <summary>
        /// Routes incoming signals to the appropriate combo step handler.
        /// All signals arrive via EnemyStateMachine.Notify().
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case EnemyAttackHitFrameSignal hit:
                    HandleHitFrame(hit.HitDirection);
                    break;

                case EnemyAttackClipEndedSignal:
                    HandleAttackClipEnded();
                    break;

                case EnemyTargetHitReactionEndedSignal:
                    HandleTargetReactionEnded();
                    break;

                case EnemyCombatHitResolvedSignal resolved:
                    HandleCombatHitResolved(resolved.Payload, StateMachine.Enemy.EntityId);
                    break;

                case EnemyAttackSequenceEndedSignal:
                    OnSequenceEnded();
                    break;
            }
        }

        // ------------------------------------------------------------------
        // PROTECTED METHODS
        // ------------------------------------------------------------------

        /// <summary>
        /// Attempts to consume one action point before starting the attack sequence.
        /// Returns false if the enemy has no remaining AP, preventing the sequence.
        /// </summary>
        protected override bool TryConsumeActionPoint()
        {
            if (StateMachine.Enemy.TryConsumeActionPoints(1))
                return true;

            GameLogger.Log(LogSourceType.Enemy, "EnemyAttackState: Not enough AP.");
            return false;
        }

        /// <summary>
        /// Triggers the attack animation on the enemy animator with the configured
        /// variant and step count parameters.
        /// </summary>
        protected override void PlayAttackAnimation()
        {
            GameLogger.Log(
                LogSourceType.Enemy,
                $"EnemyAttackState: Start attack Variant={AttackVariant}, Steps={TotalSteps}");

            StateMachine.Enemy.PlayAttack(AttackVariant, TotalSteps);
        }

        /// <summary>
        /// Advances the animator to the next combo step via trigger parameter.
        /// </summary>
        /// <param name="step">Next combo step index (1-based).</param>
        protected override void AdvanceCombo(int step)
        {
            StateMachine.Enemy.AdvanceCombo(step);
        }

        /// <summary>
        /// Triggers a forced finish animation when the target dies mid-combo,
        /// allowing the enemy to recover to idle without completing all steps.
        /// </summary>
        protected override void PlayForcedFinish()
        {
            StateMachine.Enemy.PlayForcedAttackFinish();
        }

        /// <summary>
        /// Raises EnemyAttackHit event at the authored hit frame so CombatSystem
        /// can resolve damage and apply it to the hero.
        /// </summary>
        /// <param name="step">Current combo step index.</param>
        /// <param name="totalSteps">Total number of steps in this combo.</param>
        /// <param name="direction">Hit direction for knockback and animation purposes.</param>
        protected override void RaiseAttackHit(int step, int totalSteps, HitDirection direction)
        {
            StateMachine.EventHub.EnemyEvents.EnemyAttackHitEvent.Raise(
                new EnemyAttackHitPayload(
                    enemyId: StateMachine.Enemy.EntityId,
                    stepIndex: step,
                    totalSteps: totalSteps,
                    attackIntensity: totalSteps,
                    hitDirection: direction));
        }

        /// <summary>
        /// Called by base class when AP check fails at sequence start.
        /// Delegates to OnSequenceEnded to handle turn flow correctly.
        /// </summary>
        protected override void NotifySequenceEnded()
        {
            OnSequenceEnded();
        }

        // ------------------------------------------------------------------
        // PRIVATE METHODS
        // ------------------------------------------------------------------

        /// <summary>
        /// Handles the end of a full attack sequence.
        /// Returns to ReadyState if the enemy has remaining AP, otherwise
        /// raises EnemyTurnEnded to hand control to the next entity.
        /// </summary>
        private void OnSequenceEnded()
        {
            if (StateMachine.Enemy.HasRemainingActionPoints)
            {
                StateMachine.SwitchToReadyState();
                return;
            }

            StateMachine.FinishEnemyTurn();
        }
    }
}