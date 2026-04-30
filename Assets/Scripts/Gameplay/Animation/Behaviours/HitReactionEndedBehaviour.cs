using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Notifies the entity when a hit reaction animation has effectively completed.
    /// Fires once per state entry when normalised time passes the defined threshold.
    /// Attach to the hit reaction state in the Animator Controller.
    /// Works for both HeroComponent and EnemyComponent via IAnimationSignalReceiver.
    /// </summary>
    public sealed class HitReactionEndedBehaviour : StateMachineBehaviour
    {
        // ─── CONSTANTS ────────────────────────────────────────────────

        /// <summary>
        /// Normalised time threshold after which the hit reaction is considered complete.
        /// Gameplay advances before the full animation finishes for responsive feel.
        /// </summary>
        private const float ReactionEndThreshold = 0.5f;

        // ─── FIELDS ───────────────────────────────────────────────────

        /// <summary>Ensures the signal fires only once per state entry.</summary>
        private bool _hasTriggered;

        private IAnimationSignalReceiver _receiver;

        // ─── UNITY CALLBACKS ──────────────────────────────────────────

        /// <summary>
        /// Resets the trigger guard and resolves IAnimationSignalReceiver on first entry.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _hasTriggered = false;
            ResolveReceiver(animator);
        }

        /// <summary>
        /// Fires NotifyHitReactionEnded once normalised time passes ReactionEndThreshold.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_hasTriggered)
                return;

            float normalizedProgress = stateInfo.normalizedTime % 1f;

            if (normalizedProgress < ReactionEndThreshold)
                return;

            _hasTriggered = true;

            if (_receiver == null)
            {
                GameLogger.Log(LogSourceType.Animation,
                    "HitReactionEndedBehaviour: No IAnimationSignalReceiver found.");
                return;
            }

            GameLogger.Log(LogSourceType.Animation, "HitReactionEndedBehaviour: signal fired.");
            _receiver.NotifyHitReactionEnded();
        }

        /// <summary>
        /// Intentionally empty. Completion is handled in OnStateUpdate via threshold.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // ─── PRIVATE METHODS ──────────────────────────────────────────

        /// <summary>
        /// Searches the Animator's GameObject and all parents for IAnimationSignalReceiver.
        /// Cached after first successful resolve to avoid repeated traversal.
        /// </summary>
        /// <param name="animator">Animator that owns this behaviour.</param>
        private void ResolveReceiver(Animator animator)
        {
            if (_receiver != null)
                return;

            _receiver = animator.GetComponentInParent<IAnimationSignalReceiver>();

            if (_receiver == null)
                GameLogger.Log(LogSourceType.Animation,
                    $"HitReactionEndedBehaviour: IAnimationSignalReceiver not found on '{animator.name}' or any parent.");
        }
    }
}