using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Active heal state for the hero during a puzzle-driven heal action.
    ///
    /// Unlike HeroAttackState this is a single-step action: there is no combo
    /// progression and no target selection. The heal amount is applied
    /// immediately on Enter and the animation plays for feedback.
    ///
    /// The heal particle effect is requested at an authored animation frame
    /// (HeroHealEffectFrameSignal) through HealEffectRequestedEvent, so it is
    /// timed by the clip and does not depend on a health value change.
    /// The state then waits for HeroHealSequenceEndedSignal before deciding
    /// the next transition.
    ///
    /// Scope note: heals the hero itself only. Targeting a companion is out
    /// of scope until companions exist.
    /// </summary>
    public sealed class HeroHealState : HeroBaseState
    {
        // CONSTANTS

        private const int BaseHealAmount = 10;
        private const int IntensityHealAmount = 5;

        // FIELDS

        private readonly int _healVariant;
        private readonly int _healIntensity;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new hero heal state for the given heal configuration.
        /// </summary>
        public HeroHealState(HeroStateMachine stateMachine, int healVariant, int healIntensity)
            : base(stateMachine)
        {
            _healVariant = healVariant;
            _healIntensity = healIntensity < 1 ? 1 : healIntensity;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Attempts to consume one action point, then applies the heal and
        /// starts the heal animation.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroHealState");

            if (!StateMachine.Hero.TryConsumeActionPoints(1))
            {
                GameLogger.Log(LogSourceType.Hero, "HeroHealState: Not enough AP.");
                OnSequenceEnded();
                return;
            }

            int healAmount = ResolveHealAmount(_healIntensity);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroHealState: Applying heal Amount={healAmount}, Variant={_healVariant}");

            StateMachine.Hero.ApplyHeal(healAmount);
            StateMachine.Hero.PlayHeal(_healVariant);
        }

        /// <summary>
        /// No-op. No EventHub subscriptions are owned by this state.
        /// </summary>
        public override void Exit()
        {
        }

        /// <summary>
        /// Routes heal animation signals: the effect frame requests the heal
        /// particle effect, the sequence end triggers the next transition.
        /// </summary>
        public override void Handle(IStateSignal signal)
        {
            switch (signal)
            {
                case HeroHealEffectFrameSignal:
                    OnHealEffectFrame();
                    break;

                case HeroHealSequenceEndedSignal:
                    OnSequenceEnded();
                    break;
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Resolves the heal amount from intensity using a simple linear formula,
        /// symmetrical to CombatResolver's damage formula.
        /// </summary>
        /// <param name="intensity">Resolved action intensity.</param>
        /// <returns>Heal amount to apply.</returns>
        private int ResolveHealAmount(int intensity)
        {
            return BaseHealAmount + (intensity * IntensityHealAmount);
        }

        /// <summary>
        /// Transitions to ReadyState if AP remains, otherwise ends the hero turn.
        /// Heal has no movement phase, so this is reached directly from Enter
        /// (AP exhausted) or from the heal sequence ended signal.
        /// </summary>
        private void OnSequenceEnded()
        {
            if (!StateMachine.Hero.HasRemainingActionPoints)
            {
                GameLogger.Log(LogSourceType.Hero, "HeroHealState: No AP remaining, finishing turn.");
                StateMachine.SwitchToIdleState();
                StateMachine.FinishHeroTurn();
                return;
            }

            GameLogger.Log(LogSourceType.Hero, "HeroHealState: Returning to ready.");
            StateMachine.SwitchToReadyState();
        }

        /// <summary>
        /// Requests the heal particle effect on the hero at the authored frame.
        /// </summary>
        private void OnHealEffectFrame()
        {
            int heroId = StateMachine.Hero.EntityId;

            GameLogger.Log(LogSourceType.Hero,
                $"HeroHealState: effect frame received, requesting heal effect for entity {heroId}");
                
            StateMachine.EventHub.HealthEvents.HealEffectRequestedEvent.Raise(
                new HealEffectRequestedPayload(
                    targetEntityId: heroId,
                    sourceEntityId: heroId));
        }
    }
}