using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Enemy;

namespace MightyHeroes.Gameplay.Enemy.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour responsible for detecting
    /// when an enemy hit reaction animation has finished.
    ///
    /// IMPORTANT
    /// ---------
    /// This behaviour is the SINGLE authority that decides
    /// when a hit reaction animation is considered completed.
    ///
    /// It bridges the animator state lifecycle to gameplay logic
    /// by notifying the EnemyStateMachine at the correct time.
    /// </summary>
    public sealed class EnemyHitReactionEndedBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Ensures the completion notification is fired only once
        /// per animator state entry.
        /// </summary>
        private bool _hasTriggered;

        /// <summary>
        /// Cached reference to the EnemyComponent resolved
        /// from the animator hierarchy.
        /// </summary>
        private EnemyComponent _enemy;

        /// <summary>
        /// Called when the animator enters the hit reaction state.
        /// Resets internal flags and resolves EnemyComponent if needed.
        /// </summary>
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            _hasTriggered = false;

            if (_enemy != null)
                return;

            _enemy = animator.GetComponentInParent<EnemyComponent>();

            if (_enemy == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    "EnemyHitReactionEndedBehaviour could not find EnemyComponent."
                );
            }
        }

        /// <summary>
        /// Called every frame while the hit reaction state is active.
        /// Monitors animation progress to determine completion timing.
        /// </summary>
        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            if (_hasTriggered)
                return;

            // Normalized time may exceed 1.0 for looping states
            float normalizedProgress = stateInfo.normalizedTime % 1f;

            // Hit reaction is considered complete once the animation
            // passes the authored completion threshold.
            if (normalizedProgress >= 0.5f)
            {
                _hasTriggered = true;
                _enemy?.NotifyHitReactionEnded();
            }
        }

        /// <summary>
        /// Called when the animator exits the hit reaction state.
        /// No logic is required here because completion is handled
        /// deterministically during OnStateUpdate.
        /// </summary>
        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            // Intentionally left empty.
        }
    }
}