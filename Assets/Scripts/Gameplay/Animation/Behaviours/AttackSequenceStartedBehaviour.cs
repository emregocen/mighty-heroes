using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Notifies the entity when the full attack sequence animation starts.
    /// Attach to the first state of an attack flow in the Animator Controller.
    /// Works for both HeroComponent and EnemyComponent via IAnimationSignalReceiver.
    /// </summary>
    public sealed class AttackSequenceStartedBehaviour : StateMachineBehaviour
    {
        // FIELDS

        private IAnimationSignalReceiver _receiver;

        // UNITY CALLBACKS

        /// <summary>
        /// Resolves IAnimationSignalReceiver on first entry and caches it.
        /// Then fires NotifyAttackSequenceStarted.
        /// </summary>
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveReceiver(animator);

            if (_receiver == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    "AttackSequenceStartedBehaviour: No IAnimationSignalReceiver found.");

                return;
            }

            _receiver.NotifyAttackSequenceStarted();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Searches the Animator's GameObject and all parents
        /// for IAnimationSignalReceiver.
        /// Cached after first successful resolve
        /// to avoid repeated traversal.
        /// </summary>
        /// <param name="animator">
        /// Animator that owns this behaviour.
        /// </param>
        private void ResolveReceiver(Animator animator)
        {
            if (_receiver != null)
                return;

            _receiver =
                animator.GetComponentInParent<IAnimationSignalReceiver>();

            if (_receiver == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    $"AttackSequenceStartedBehaviour: IAnimationSignalReceiver not found on '{animator.name}' or any parent.");
            }
        }
    }
}