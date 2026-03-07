using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.StateMachine.Base;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Active attack state for the hero during a puzzle-driven combo sequence.
    ///
    /// Responsibilities:
    /// - Handles incoming signals from animation behaviours and FSM-forwarded events.
    /// - Delegates combo step progression to ComboAttackState base class.
    /// - Raises HeroAttackHit events for each hit frame so CombatSystem can resolve damage.
    /// - Decides whether to return to ReadyState (AP remaining) or finish the hero turn.
    ///
    /// Signal routing:
    /// - HeroAttackHitFrameSignal      : forwarded by HeroStateMachine from AnimationEventProxy
    /// - HeroAttackClipEndedSignal     : forwarded by HeroStateMachine from behaviour
    /// - HeroTargetHitReactionEndedSignal : forwarded by HeroStateMachine from EventHub
    /// - HeroCombatHitResolvedSignal   : forwarded by HeroStateMachine from EventHub
    /// - HeroAttackSequenceEndedSignal : forwarded by HeroStateMachine from behaviour
    /// </summary>
    public sealed class HeroAttackState : ComboAttackState<HeroStateMachine>
    {
        // ------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a new hero attack state for the given combo configuration.
        /// </summary>
        /// <param name="stateMachine">Owning hero state machine.</param>
        /// <param name="attackVariant">Animation variant index for the attack.</param>
        /// <param name="attackIntensity">Intensity level, also defines combo step count.</param>
        public HeroAttackState(
            HeroStateMachine stateMachine,
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
        /// All signals arrive via HeroStateMachine.Notify().
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case HeroAttackHitFrameSignal hit:
                    HandleHitFrame(hit.HitDirection);
                    break;

                case HeroAttackClipEndedSignal:
                    HandleAttackClipEnded();
                    break;

                case HeroTargetHitReactionEndedSignal:
                    HandleTargetReactionEnded();
                    break;

                case HeroCombatHitResolvedSignal resolved:
                    HandleCombatHitResolved(resolved.Payload, StateMachine.Hero.EntityId);
                    break;

                case HeroAttackSequenceEndedSignal:
                    OnSequenceEnded();
                    break;
            }
        }

        // ------------------------------------------------------------------
        // PROTECTED METHODS
        // ------------------------------------------------------------------

        /// <summary>
        /// Attempts to consume one action point before starting the attack sequence.
        /// Returns false if the hero has no remaining AP, preventing the sequence.
        /// </summary>
        protected override bool TryConsumeActionPoint()
        {
            if (StateMachine.Hero.TryConsumeActionPoints(1))
                return true;

            GameLogger.Log(LogSourceType.Hero, "HeroAttackState: Not enough AP.");
            return false;
        }

        /// <summary>
        /// Triggers the attack animation on the hero animator with the configured
        /// variant and step count parameters.
        /// </summary>
        protected override void PlayAttackAnimation()
        {
            GameLogger.Log(
                LogSourceType.Hero,
                $"HeroAttackState: Start attack Variant={AttackVariant}, Steps={TotalSteps}");

            StateMachine.Hero.PlayAttack(AttackVariant, TotalSteps);
        }

        /// <summary>
        /// Advances the animator to the next combo step via trigger parameter.
        /// </summary>
        /// <param name="step">Next combo step index (1-based).</param>
        protected override void AdvanceCombo(int step)
        {
            StateMachine.Hero.AdvanceCombo(step);
        }

        /// <summary>
        /// Triggers a forced finish animation when the target dies mid-combo,
        /// allowing the hero to recover to idle without completing all steps.
        /// </summary>
        protected override void PlayForcedFinish()
        {
            StateMachine.Hero.PlayForcedAttackFinish();
        }

        /// <summary>
        /// Raises HeroAttackHit event at the authored hit frame so CombatSystem
        /// can resolve damage and apply it to the target.
        /// </summary>
        /// <param name="step">Current combo step index.</param>
        /// <param name="totalSteps">Total number of steps in this combo.</param>
        /// <param name="direction">Hit direction for knockback and animation purposes.</param>
        protected override void RaiseAttackHit(int step, int totalSteps, HitDirection direction)
        {
            StateMachine.EventHub.HeroEvents.HeroAttackHitEvent.Raise(
                new HeroAttackHitPayload(
                    attackId: StateMachine.Hero.EntityId,
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
        /// Returns to ReadyState if the hero has remaining AP, otherwise
        /// raises HeroTurnEnded to hand control to the next entity.
        /// </summary>
        private void OnSequenceEnded()
        {
            if (StateMachine.Hero.HasRemainingActionPoints)
            {
                StateMachine.SwitchToReadyState();
                return;
            }
            
            StateMachine.SwitchToIdleState();
            StateMachine.FinishHeroTurn();
        }
    }
}