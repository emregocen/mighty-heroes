using MightyHeroes.Battle.TurnFlow.Payloads;
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
    /// - Subscribes to AllEnemiesDefeatedEvent while active to set _isVictoryPending.
    /// - Decides whether to retreat or go directly to victory at sequence end.
    ///
    /// DamageType is carried from the resolved HeroActionExecution (set by the
    /// matched tile's ActionType — Attack or MagicAttack) through the base class
    /// and forwarded into HeroAttackHitPayload. This single state handles both
    /// physical and magical attacks; only the damage type differs.
    ///
    /// Victory pending pattern:
    /// AllEnemiesDefeatedEvent can fire mid-combo (inside the hit frame call stack).
    /// The flag defers the victory transition to OnSequenceEnded, which is the first
    /// point where the call stack has fully unwound and it is safe to raise
    /// TurnBattleVictoryEvent.
    /// </summary>
    public sealed class HeroAttackState : ComboAttackState<HeroStateMachine>
    {
        // FIELDS

        /// <summary>
        /// Set to true when AllEnemiesDefeatedEvent fires during an active combo.
        /// Checked in OnSequenceEnded to skip retreat and go directly to victory.
        /// </summary>
        private bool _isVictoryPending;

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
        /// Subscribes to AllEnemiesDefeatedEvent and raises HeroAttackStartedEvent.
        /// </summary>
        public override void Enter()
        {
            _isVictoryPending = false;

            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent
                .Register(OnAllEnemiesDefeated);

            GameLogger.Log(LogSourceType.Hero, "Enter HeroAttackState");

            StateMachine.EventHub.HeroEvents.HeroAttackStartedEvent.Raise(
                new HeroAttackStartedPayload(
                    heroEntityId: StateMachine.Hero.EntityId,
                    attackVariant: AttackVariant,
                    totalSteps: TotalSteps));

            base.Enter();
        }

        /// <summary>
        /// Unsubscribes from AllEnemiesDefeatedEvent on state exit.
        /// </summary>
        public override void Exit()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroAttackState.Exit called.");
            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent
                .Unregister(OnAllEnemiesDefeated);
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
        /// Handles the end of a full attack sequence.
        /// If all enemies were defeated during this sequence, raises TurnBattleVictoryEvent
        /// directly — this is the first safe point where the attack call stack has unwound.
        /// Otherwise transitions to retreat.
        /// </summary>
        private void OnSequenceEnded()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"HeroAttackState - OnSequenceEnded called. IsVictoryPending={_isVictoryPending}");

            if (_isVictoryPending)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroAttackState - Switching to VictoryState.");
                StateMachine.SwitchToVictoryState();
                return;
            }

            GameLogger.Log(LogSourceType.Hero, "HeroAttackState - Switching to RetreatState.");
            StateMachine.SwitchToRetreatState();
        }

        // EVENT HANDLERS

        /// <summary>
        /// Called when all enemies are defeated during an active combo sequence.
        /// Sets the pending flag — actual victory transition is deferred to
        /// OnSequenceEnded so the attack call stack can unwind cleanly first.
        /// </summary>
        private void OnAllEnemiesDefeated(TurnAllEnemiesDefeatedPayload payload)
        {
            GameLogger.Log(LogSourceType.Hero, "HeroAttackState - OnAllEnemiesDefeated received.");

            _isVictoryPending = true;
        }
    }
}