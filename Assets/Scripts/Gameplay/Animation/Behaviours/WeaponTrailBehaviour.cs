using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Equipment.Views;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Drives weapon trail and slash arc effects via WeaponView.
    /// WeaponView is resolved via GetComponentInParent so it can live on a parent
    /// GameObject above the Animator.
    /// OnStateEnter → BeginTrail + PlaySlashEffect or PlaySpecialEffect
    /// OnStateExit  → EndTrail when EndsTrailOnExit is true
    /// </summary>
    public sealed class WeaponTrailBehaviour : StateMachineBehaviour
    {
        // ─── FIELDS ───────────────────────────────────────────────────

        [Header("Attack Settings")]
        [Tooltip("Which side this swing lands on the target. Mirrors the slash arc.")]
        [SerializeField] private HitDirection _hitDirection = HitDirection.Right;

        [Tooltip("True for special or ultimate attacks. Plays SpecialEffectPrefab instead of SlashEffectPrefab.")]
        [SerializeField] private bool _isSpecialAttack;

        [Header("Phase")]
        [Tooltip("Enable on the state where the trail should stop.")]
        [SerializeField] private bool _endsTrailOnExit;

        private WeaponView _weaponView;

        // ─── UNITY CALLBACKS ──────────────────────────────────────────

        /// <summary>
        /// Resolves WeaponView, begins trail, and plays the appropriate slash effect.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ResolveWeaponView(animator);

            if (_weaponView == null)
                return;

            _weaponView.BeginTrail();

            if (_isSpecialAttack)
                _weaponView.PlaySpecialEffect(_hitDirection);
            else
                _weaponView.PlaySlashEffect(_hitDirection);
        }

        /// <summary>
        /// Ends trail on state exit when EndsTrailOnExit is true.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_endsTrailOnExit)
                return;

            ResolveWeaponView(animator);
            _weaponView?.EndTrail();
        }

        // ─── PRIVATE METHODS ──────────────────────────────────────────

        /// <summary>
        /// Searches the Animator's GameObject and all parents for WeaponView.
        /// Cached after first successful resolve to avoid repeated traversal.
        /// </summary>
        /// <param name="animator">Animator that owns this behaviour.</param>
        private void ResolveWeaponView(Animator animator)
        {
            if (_weaponView != null)
                return;

            _weaponView = animator.GetComponentInParent<WeaponView>();

            if (_weaponView == null)
                GameLogger.Log(LogSourceType.Animation,
                    $"WeaponTrailBehaviour: WeaponView not found on '{animator.name}' or any parent.");
        }
    }
}