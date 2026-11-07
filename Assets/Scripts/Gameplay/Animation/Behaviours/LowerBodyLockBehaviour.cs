using DG.Tweening;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Keeps the legs in the idle pose while a Base Layer animation plays,
    /// by blending the lower-body lock layer in on enter and out on exit.
    ///
    /// Layer convention: every action animation (attack, heal, ultimate, taunt)
    /// stays on the Base Layer. The lock layer ("LowerBodyLock") holds only an
    /// idle state with a lower-body avatar mask and Override blending at
    /// default weight 0. Any state or sub-state machine whose clip moves the
    /// feet in place gets this behaviour; no extra layer per animation.
    ///
    /// Attach to a single state or to a sub-state machine. On a sub-state
    /// machine the behaviour receives enter/exit for every child state; it
    /// counts active child states so transitions between children do not
    /// blend the lock out and back in.
    /// </summary>
    public sealed class LowerBodyLockBehaviour : StateMachineBehaviour
    {
        // FIELDS

        [Tooltip("Animator layer that holds the idle pose for the legs.")]
        [SerializeField] private string _lockLayerName = "LowerBodyLock";

        [Tooltip("Seconds to blend the lock in when the state starts.")]
        [SerializeField, Min(0f)] private float _blendInDuration = 0.15f;

        [Tooltip("Seconds to blend the lock out when the state ends.")]
        [SerializeField, Min(0f)] private float _blendOutDuration = 0.2f;

        private int _lockLayerIndex = -1;
        private int _activeStateCount;
        private Tween _weightTween;

        // UNITY CALLBACKS

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!ResolveLayerIndex(animator))
                return;

            _activeStateCount++;

            if (_activeStateCount == 1)
                BlendLockWeight(animator, 1f, _blendInDuration);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_lockLayerIndex < 0)
                return;

            _activeStateCount = Mathf.Max(0, _activeStateCount - 1);

            if (_activeStateCount == 0)
                BlendLockWeight(animator, 0f, _blendOutDuration);
        }

        // PRIVATE METHODS

        /// <summary>Caches the lock layer index. Returns false if the layer does not exist.</summary>
        private bool ResolveLayerIndex(Animator animator)
        {
            if (_lockLayerIndex < 0)
                _lockLayerIndex = animator.GetLayerIndex(_lockLayerName);

            return _lockLayerIndex >= 0;
        }

        /// <summary>Tweens the lock layer weight, replacing any running blend.</summary>
        private void BlendLockWeight(Animator animator, float targetWeight, float duration)
        {
            _weightTween?.Kill();

            int layerIndex = _lockLayerIndex;

            if (duration <= 0f)
            {
                animator.SetLayerWeight(layerIndex, targetWeight);
                return;
            }

            _weightTween = DOTween.To(() => animator.GetLayerWeight(layerIndex), value => animator.SetLayerWeight(layerIndex, value), targetWeight, duration).SetLink(animator.gameObject);
        }
    }
}
