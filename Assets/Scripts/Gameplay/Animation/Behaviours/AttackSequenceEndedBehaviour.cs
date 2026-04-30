using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Notifies the entity when the full attack sequence animation has completed.
    /// Attach to the final state of an attack flow in the Animator Controller.
    /// This is typically the ReturnToIdle state that follows FollowThrough.
    /// Works for both HeroComponent and EnemyComponent via IAnimationSignalReceiver.
    /// </summary>
    public sealed class AttackSequenceEndedBehaviour : StateMachineBehaviour
    {
        // ─── FIELDS ───────────────────────────────────────────────────

        private IAnimationSignalReceiver _receiver;

        // ─── UNITY CALLBACKS ──────────────────────────────────────────

        /// <summary>
        /// Resolves IAnimationSignalReceiver on first entry and caches it.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ResolveReceiver(animator);
        }

        /// <summary>
        /// Fires NotifyAttackSequenceEnded when the state exits.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_receiver == null)
            {
                GameLogger.Log(LogSourceType.Animation,
                    "AttackSequenceEndedBehaviour: No IAnimationSignalReceiver found.");
                return;
            }

            _receiver.NotifyAttackSequenceEnded();
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
                    $"AttackSequenceEndedBehaviour: IAnimationSignalReceiver not found on '{animator.name}' or any parent.");
        }
    }
}