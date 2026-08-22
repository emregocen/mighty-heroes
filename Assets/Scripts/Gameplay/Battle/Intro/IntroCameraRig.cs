using DG.Tweening;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// Drives a single intro camera through a sequence of offset keyframes
    /// relative to the hero, keeping the hero framed throughout while the
    /// camera itself moves — walk framing, taunt framing, then enemy reveal.
    ///
    /// Offset is expressed in hero-local space (so it rotates with hero facing)
    /// and applied every LateUpdate against the hero's current position, which
    /// is what makes the camera track the hero smoothly while he walks.
    ///
    /// Cross-fades between keyframes are done via DOTween tweening the offset
    /// and look-offset vectors — Cinemachine itself does no work here, this
    /// camera is manually driven and only handed off to CM_BattleCamera via
    /// priority swap once the intro finishes.
    /// </summary>
    public sealed class IntroCameraRig : MonoBehaviour
    {
        // FIELDS

        [SerializeField] private Transform _heroRoot;

        private Vector3 _currentOffset;
        private Vector3 _currentLookOffset;
        private Tween _offsetTween;
        private Tween _lookOffsetTween;

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (_heroRoot == null)
                return;

            Vector3 worldOffset = _heroRoot.rotation * _currentOffset;
            transform.position = _heroRoot.position + worldOffset;

            Vector3 lookTarget = _heroRoot.position + _heroRoot.rotation * _currentLookOffset;
            transform.LookAt(lookTarget);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Immediately snaps to the given offset with no tween. Call once before
        /// the sequence starts.
        /// </summary>
        public void SetImmediate(Vector3 offset, Vector3 lookOffset)
        {
            KillTweens();
            _currentOffset = offset;
            _currentLookOffset = lookOffset;
        }

        /// <summary>
        /// Tweens from the current offset to the target offset over duration.
        /// Returns the tween so callers can yield on it or chain further logic.
        /// </summary>
        public Tween MoveTo(Vector3 targetOffset, Vector3 targetLookOffset, float duration, Ease ease = Ease.InOutSine)
        {
            KillTweens();

            _offsetTween = DOTween.To(
                () => _currentOffset,
                value => _currentOffset = value,
                targetOffset,
                duration).SetEase(ease);

            _lookOffsetTween = DOTween.To(
                () => _currentLookOffset,
                value => _currentLookOffset = value,
                targetLookOffset,
                duration).SetEase(ease);

            return _offsetTween;
        }

        // PRIVATE METHODS

        private void KillTweens()
        {
            _offsetTween?.Kill();
            _lookOffsetTween?.Kill();
        }
    }
}