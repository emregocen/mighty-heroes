using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.StateMachine;
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
    /// - Delegates combo step progression to CombatAttackState base class.
    /// - Raises HeroAttackHit events for each hit frame so CombatSystem can resolve damage.
    /// - Switches to retreat at sequence end. Victory is not decided here:
    ///   HeroAttackSequenceEndedEvent lets BattleFlowController raise the single
    ///   victory chain, and HeroStateMachine switches to victory before this
    ///   state receives the sequence-ended signal.
    /// - Consumes the hero's pending attack buff once the sequence has landed
    ///   at least one hit, so the buff covers every step of a single combo.
    ///
    /// DamageType is carried from the resolved HeroActionExecution (set by the
    /// matched tile's ActionType — Attack or MagicAttack) through the base class
    /// and forwarded into HeroAttackHitPayload. This single state handles both
    /// physical and magical attacks; only the damage type differs.
    /// </summary>
    public sealed class HeroAttackState : CombatAttackState<HeroStateMachine>
    {
        // CONSTRUCTOR

        /// <summary>
        /// Creates a new hero attack state for the given combo configuration.
        /// </summary>
        public HeroAttackState(HeroStateMachine stateMachine, int attackVariant, int attackIntensity, DamageType damageType)
            : base(stateMachine, attackVariant, attackIntensity, damageType)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Raises HeroAttackStartedEvent and starts the combo sequence.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroAttackState");

            StateMachine.EventHub.HeroEvents.HeroAttackStartedEvent.Raise(
                new HeroAttackStartedPayload(
                    heroEntityId: StateMachine.Hero.EntityId,
                    attackVariant: AttackVariant,
                    totalSteps: TotalSteps));

            base.Enter();
        }

        /// <summary>
        /// Logs the state exit.
        /// </summary>
        public override void Exit()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroAttackState.Exit called.");
        }

        /// <summary>
        /// Routes incoming signals to the appropriate combo step handler.
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

        // PROTECTED METHODS

        /// <summary>
        /// Attempts to consume one action point before starting the attack sequence.
        /// </summary>
        protected override bool TryConsumeActionPoint()
        {
            if (StateMachine.Hero.TryConsumeActionPoints(1))
                return true;

            GameLogger.Log(LogSourceType.Hero, "HeroAttackState: Not enough AP.");
            return false;
        }

        /// <summary>
        /// Triggers the attack animation on the hero animator.
        /// </summary>
        protected override void PlayAttackAnimation()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"HeroAttackState: Start attack Variant={AttackVariant}, Steps={TotalSteps}, DamageType={DamageType}");

            StateMachine.Hero.PlayAttack(AttackVariant, TotalSteps);
        }

        /// <summary>
        /// Advances the animator to the next combo step.
        /// </summary>
        protected override void AdvanceCombo(int step)
        {
            StateMachine.Hero.AdvanceCombo(step);
        }

        /// <summary>
        /// Triggers a forced finish animation when the target dies mid-combo.
        /// </summary>
        protected override void PlayForcedFinish()
        {
            StateMachine.Hero.PlayForcedAttackFinish();
        }

        /// <summary>
        /// Raises HeroAttackHit event at the authored hit frame, carrying this
        /// sequence's DamageType so CombatSystem applies the correct damage type.
        /// </summary>
        protected override void RaiseAttackHit(int step, int totalSteps, HitDirection direction)
        {
            StateMachine.EventHub.HeroEvents.HeroAttackHitEvent.Raise(
                new HeroAttackHitPayload(
                    attackId: StateMachine.Hero.EntityId,
                    stepIndex: step,
                    totalSteps: totalSteps,
                    attackIntensity: totalSteps,
                    hitDirection: direction,
                    damageType: DamageType));
        }

        /// <summary>
        /// Called by base class when AP check fails at sequence start.
        /// </summary>
        protected override void NotifySequenceEnded()
        {
            OnSequenceEnded();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Handles the end of a full attack sequence: consumes the attack buff
        /// and transitions to retreat. On the winning sequence the hero is
        /// already in victory and never receives this call.
        /// </summary>
        private void OnSequenceEnded()
        {
            GameLogger.Log(LogSourceType.Hero,
                "HeroAttackState - OnSequenceEnded called.");

            if (LastHitStep > 0)
                StateMachine.Hero.ConsumeAttackBuff();

            GameLogger.Log(LogSourceType.Hero, "HeroAttackState - Switching to RetreatState.");
            StateMachine.SwitchToRetreatState();
        }
    }
}