using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;
using MightyHeroes.Gameplay.Ultimate;
using MightyHeroes.Gameplay.Ultimate.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Hero state for the ultimate, entered when the gauge is full and the
    /// player matches the affinity color.
    ///
    /// Flow:
    /// - Enter: pay the AP cost from UltimateData, empty the gauge, raise
    ///   UltimateStartedEvent and play the ultimate animation (heal clip for now,
    ///   so it shares the heal signals, lower-body lock and sword layer handling).
    /// - Effect frame: raise UltimateHitEvent once; CombatSystem damages every
    ///   target of the target mode (front line for the demo).
    /// - Sequence end: raise HeroAttackSequenceEndedEvent so BattleFlowController
    ///   can drain a pending victory and the kill-cam can release, then
    ///   UltimateEndedEvent, then continue to victory, ready or turn end.
    ///
    /// If the effect frame never arrives, the hit is applied once on sequence end.
    /// </summary>
    public sealed class HeroUltimateState : HeroBaseState
    {
        // FIELDS

        private readonly int _variant;
        private readonly int _tier;
        private bool _isHitApplied;
        private bool _isVictoryPending;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the ultimate state for the given animation variant and match tier.
        /// </summary>
        /// <param name="stateMachine">Owning hero state machine.</param>
        /// <param name="variant">Ultimate animation variant.</param>
        /// <param name="tier">Tier of the triggering affinity match.</param>
        public HeroUltimateState(HeroStateMachine stateMachine, int variant, int tier) : base(stateMachine)
        {
            _variant = variant;
            _tier = tier;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Pays the AP cost, consumes the gauge and starts the ultimate animation.
        /// </summary>
        public override void Enter()
        {
            UltimateGauge gauge = StateMachine.Hero.UltimateGauge;

            if (gauge == null || !TryPayActionPoints(gauge.Data.ApCostMode))
            {
                GameLogger.Log(LogSourceType.Hero, "HeroUltimateState: no gauge or not enough AP, returning to ready.");
                ContinueAfterUltimate();
                return;
            }

            _isVictoryPending = false;
            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent.Register(OnAllEnemiesDefeated);

            gauge.Consume();

            GameLogger.Log(LogSourceType.Hero, $"Enter HeroUltimateState Tier={_tier} ApCost={gauge.Data.ApCostMode}");

            StateMachine.EventHub.HeroEvents.UltimateStartedEvent.Raise(new UltimateStartedPayload(StateMachine.Hero.EntityId, _tier));
            StateMachine.Hero.PlayUltimate(_variant);
        }

        /// <summary>
        /// Unsubscribes from AllEnemiesDefeatedEvent.
        /// </summary>
        public override void Exit()
        {
            StateMachine.EventHub.TurnEvents.AllEnemiesDefeatedEvent.Unregister(OnAllEnemiesDefeated);
        }

        /// <summary>
        /// Routes the ultimate animation signals (shared with the heal clip).
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case HeroHealEffectFrameSignal:
                    ApplyHit();
                    break;

                case HeroHealSequenceEndedSignal:
                    HandleSequenceEnded();
                    break;
            }
        }

        // PRIVATE METHODS

        /// <summary>Pays the configured AP cost. Returns false when the hero cannot pay it.</summary>
        private bool TryPayActionPoints(UltimateApCostMode costMode)
        {
            switch (costMode)
            {
                case UltimateApCostMode.One:
                    return StateMachine.Hero.TryConsumeActionPoints(1);

                case UltimateApCostMode.AllRemaining:
                    int remaining = StateMachine.Hero.CurrentActionPoints;
                    return remaining > 0 && StateMachine.Hero.TryConsumeActionPoints(remaining);

                default:
                    return true;
            }
        }

        /// <summary>Raises the ultimate hit exactly once.</summary>
        private void ApplyHit()
        {
            if (_isHitApplied)
                return;

            _isHitApplied = true;

            UltimateData data = StateMachine.Hero.UltimateGauge.Data;

            StateMachine.EventHub.HeroEvents.UltimateHitEvent.Raise(new UltimateHitPayload(StateMachine.Hero.EntityId, _tier, data.DamageMultiplier, data.TargetMode));
        }

        /// <summary>
        /// Applies the hit if the effect frame was missed, consumes the attack buff,
        /// announces the end of the sequence and continues.
        /// </summary>
        private void HandleSequenceEnded()
        {
            if (!_isHitApplied)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroUltimateState: effect frame missing, applying hit on sequence end.");
                ApplyHit();
            }

            StateMachine.Hero.ConsumeAttackBuff();

            StateMachine.EventHub.HeroEvents.HeroAttackSequenceEndedEvent.Raise(new HeroAttackSequenceEndedPayload());
            StateMachine.EventHub.HeroEvents.UltimateEndedEvent.Raise(new UltimateEndedPayload(StateMachine.Hero.EntityId));

            if (_isVictoryPending)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroUltimateState: all enemies defeated, switching to victory.");
                StateMachine.SwitchToVictoryState();
                return;
            }

            ContinueAfterUltimate();
        }

        /// <summary>Returns to ready while AP remains, otherwise ends the hero turn.</summary>
        private void ContinueAfterUltimate()
        {
            if (!StateMachine.Hero.HasRemainingActionPoints)
            {
                StateMachine.SwitchToIdleState();
                StateMachine.FinishHeroTurn();
                return;
            }

            StateMachine.SwitchToReadyState();
        }

        // EVENT HANDLERS

        private void OnAllEnemiesDefeated(TurnAllEnemiesDefeatedPayload payload)
        {
            _isVictoryPending = true;
        }
    }
}
