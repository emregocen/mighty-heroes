using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Active state for single-step, self-targeted support actions driven by
    /// puzzle matches (Heal, Shield, Buff).
    ///
    /// Unlike HeroAttackState there is no combo progression and no target
    /// selection. One AP is consumed on Enter, the support animation plays,
    /// and the effect value (already resolved by HeroActionResolver from
    /// ActionTierData) is applied at the authored effect frame
    /// (HeroHealEffectFrameSignal), so health bar, floating text and particle
    /// land on the same frame.
    ///
    /// If the effect frame never arrives (missing animation event), the effect
    /// is applied once on HeroHealSequenceEndedSignal as a safety fallback and
    /// a log entry is written so the missing event can be fixed in the clip.
    ///
    /// Animation note: all support types currently share the heal clip and
    /// its signals. A dedicated clip per type can be selected later inside
    /// HeroComponent without changing this state.
    /// </summary>
    public sealed class HeroSupportState : HeroBaseState
    {
        // FIELDS

        private readonly ActionType _supportType;
        private readonly int _variant;
        private readonly int _effectValue;
        private bool _isEffectApplied;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new hero support state for the given support action.
        /// </summary>
        /// <param name="stateMachine">Owning hero state machine.</param>
        /// <param name="supportType">Support action type (Heal, Shield, Buff).</param>
        /// <param name="variant">Animation variant index.</param>
        /// <param name="effectValue">Resolved effect amount for the action tier.</param>
        public HeroSupportState(HeroStateMachine stateMachine, ActionType supportType, int variant, int effectValue)
            : base(stateMachine)
        {
            _supportType = supportType;
            _variant = variant;
            _effectValue = effectValue;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Attempts to consume one action point, then starts the support animation.
        /// The effect itself is applied later at the effect frame.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, $"Enter HeroSupportState Type={_supportType} Value={_effectValue}");

            if (!StateMachine.Hero.TryConsumeActionPoints(1))
            {
                GameLogger.Log(LogSourceType.Hero, "HeroSupportState: Not enough AP.");
                OnSequenceEnded();
                return;
            }

            StateMachine.Hero.PlayHeal(_variant);
        }

        /// <summary>
        /// No-op. No EventHub subscriptions are owned by this state.
        /// </summary>
        public override void Exit()
        {
        }

        /// <summary>
        /// Routes support animation signals: the effect frame applies the
        /// effect, the sequence end triggers the next transition.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case HeroHealEffectFrameSignal:
                    ApplyEffect();
                    break;

                case HeroHealSequenceEndedSignal:
                    HandleSequenceEnded();
                    break;
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Applies the support effect exactly once and requests the matching
        /// presentation effect.
        /// </summary>
        private void ApplyEffect()
        {
            if (_isEffectApplied)
                return;

            _isEffectApplied = true;

            switch (_supportType)
            {
                case ActionType.Heal:
                    ApplyHeal();
                    break;

                case ActionType.Shield:
                    StateMachine.Hero.ApplyShield(_effectValue);
                    break;

                case ActionType.Buff:
                    StateMachine.Hero.ApplyAttackBuff(_effectValue);
                    break;

                default:
                    GameLogger.Log(LogSourceType.Hero,
                        $"HeroSupportState: SupportType={_supportType} has no effect implementation yet.");
                    break;
            }
        }

        /// <summary>
        /// Heals the hero and requests the heal particle effect on the hero.
        /// </summary>
        private void ApplyHeal()
        {
            int heroId = StateMachine.Hero.EntityId;

            StateMachine.Hero.ApplyHeal(_effectValue);

            StateMachine.EventHub.HealthEvents.HealEffectRequestedEvent.Raise(
                new HealEffectRequestedPayload(targetEntityId: heroId, sourceEntityId: heroId));
        }

        /// <summary>
        /// Applies the effect as a fallback if the effect frame was missed,
        /// then continues with the regular transition.
        /// </summary>
        private void HandleSequenceEnded()
        {
            if (!_isEffectApplied)
            {
                GameLogger.Log(LogSourceType.Hero,
                    $"HeroSupportState: effect frame missing for Type={_supportType}, applying on sequence end.");
                ApplyEffect();
            }

            OnSequenceEnded();
        }

        /// <summary>
        /// Transitions to ReadyState if AP remains, otherwise ends the hero turn.
        /// Reached directly from Enter (AP exhausted) or from the sequence ended signal.
        /// </summary>
        private void OnSequenceEnded()
        {
            if (!StateMachine.Hero.HasRemainingActionPoints)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroSupportState: No AP remaining, finishing turn.");
                StateMachine.SwitchToIdleState();
                StateMachine.FinishHeroTurn();
                return;
            }

            GameLogger.Log(LogSourceType.Hero, "HeroSupportState: Returning to ready.");
            StateMachine.SwitchToReadyState();
        }
    }
}
