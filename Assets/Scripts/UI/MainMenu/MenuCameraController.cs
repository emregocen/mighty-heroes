using DG.Tweening;
using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Slowly orbits the camera around the hero in the main menu.
    ///
    /// Tracks a relative offset angle rather than absolute world angle so
    /// OrbitAngle maps directly to degrees swept regardless of starting orientation.
    /// The camera sweeps forward then returns via Yoyo loop.
    /// </summary>
    public sealed class MenuCameraController : MonoBehaviour
    {
        // FIELDS

        [Header("Orbit Target")]
        [Tooltip("The point the camera orbits around. Assign the hero root transform.")]
        [SerializeField] private Transform _orbitTarget;

        [Header("Orbit Settings")]
        [Tooltip("Total degrees the camera sweeps in one direction before reversing.")]
        [SerializeField] private float _orbitAngle = 60f;

        [Tooltip("Seconds to complete one full sweep in one direction.")]
        [SerializeField] private float _sweepDuration = 20f;

        [SerializeField] private Ease _sweepEase = Ease.InOutSine;

        private Vector3 _startPosition;
        private float _currentAngleOffset;
        private Tween _orbitTween;

        // UNITY CALLBACKS

        private void Start()
        {
            if (_orbitTarget == null)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MenuCameraController: OrbitTarget is not assigned.");
                return;
            }

            _startPosition = transform.position;
            _currentAngleOffset = 0f;

            StartOrbit();
        }

        private void OnDestroy()
        {
            _orbitTween?.Kill();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Tweens _currentAngleOffset from 0 to _orbitAngle and back via Yoyo.
        /// ApplyAngle recalculates world position each frame from this offset.
        /// </summary>
        private void StartOrbit()
        {
            _orbitTween = DOTween.To(
                    () => _currentAngleOffset,
                    value =>
                    {
                        _currentAngleOffset = value;
                        ApplyAngle(value);
                    },
                    _orbitAngle,
                    _sweepDuration)
                .SetEase(_sweepEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Rotates the start position around the orbit target by the given
        /// relative angle offset and repositions the camera there.
        /// </summary>
        private void ApplyAngle(float angleOffset)
        {
            Vector3 offset = _startPosition - _orbitTarget.position;

            Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.up);
            Vector3 newOffset = rotation * offset;

            transform.position = _orbitTarget.position + newOffset;

            Vector3 lookTarget = _orbitTarget.position +
                                 Vector3.up * (offset.y * 0.5f);
            transform.LookAt(lookTarget);
        }
    }
}