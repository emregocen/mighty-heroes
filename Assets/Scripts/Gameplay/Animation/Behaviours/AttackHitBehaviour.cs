using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using MightyHeroes.Gameplay.Combat.Types;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Notifies the entity when the hit frame of an attack animation is reached.
    /// Attach to the Animator state that represents the active hit window of an attack.
    /// Works for both HeroComponent and EnemyComponent via IAnimationSignalReceiver.
    /// </summary>
    public sealed class AttackHitBehaviour : StateMachineBehaviour
    {
        // ─── FIELDS ───────────────────────────────────────────────────

        [Tooltip("Direction of the hit. Drives hit reaction animation on the target.")]
        [SerializeField] private HitDirection _hitDirection = HitDirection.Right;

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
        /// Fires NotifyAttackHit when the hit state exits.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_receiver == null)
            {
                GameLogger.Log(LogSourceType.Animation,
                    "AttackHitBehaviour: No IAnimationSignalReceiver found.");
                return;
            }

            _receiver.NotifyAttackHit(_hitDirection);
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
                    $"AttackHitBehaviour: IAnimationSignalReceiver not found on '{animator.name}' or any parent.");
        }
    }
}