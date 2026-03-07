using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;

namespace MightyHeroes.Gameplay.Hero.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour responsible for signaling
    /// when a hero hit reaction has effectively ended.
    ///
    /// IMPORTANT
    /// ---------
    /// This behaviour is the SINGLE source of truth for
    /// "hero hit reaction ended" semantics.
    ///
    /// Combo systems and enemy attack flow must rely on
    /// this signal for deterministic synchronization.
    /// </summary>
    public sealed class HeroHitReactionEndedBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Ensures the ended signal is fired only once per state entry.
        /// </summary>
        private bool _hasTriggered;

        /// <summary>
        /// Cached reference to the owning HeroComponent,
        /// resolved from the animator hierarchy.
        /// </summary>
        private HeroComponent _hero;

        /// <summary>
        /// Called when the animator enters the hit reaction state.
        /// Resets internal flags and resolves required references.
        /// </summary>
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            _hasTriggered = false;

            if (_hero != null)
                return;

            _hero = animator.GetComponentInParent<HeroComponent>();

            if (_hero == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    "HeroHitReactionEndedBehaviour could not find HeroComponent."
                );
            }
        }

        /// <summary>
        /// Called every frame while the hit reaction animation is playing.
        /// Fires the ended signal once the animation passes the defined threshold.
        /// </summary>
        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            if (_hasTriggered)
                return;

            float normalizedProgress = stateInfo.normalizedTime % 1f;

            // Gameplay considers reaction "ended" after this point
            if (normalizedProgress >= 0.4f)
            {
                GameLogger.Log(LogSourceType.Animation, "HeroReactionEnded signal fired.");
                _hasTriggered = true;
                _hero?.NotifyHitReactionEnded();
            }
        }

        /// <summary>
        /// Called when the animator exits the hit reaction state.
        /// No logic is required here because gameplay completion
        /// is already handled deterministically in OnStateUpdate.
        /// </summary>
        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            // Intentionally empty
        }
    }
}