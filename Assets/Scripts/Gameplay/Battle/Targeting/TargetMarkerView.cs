using DG.Tweening;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Targeting
{
    /// <summary>
    /// World-space marker that hovers above the currently selected enemy.
    /// Pure view: it follows the transform it is given, bobs up and down and
    /// plays a short scale pop whenever the target changes. The visual itself
    /// (mesh or sprite) is whatever is placed under the visual root.
    /// </summary>
    public sealed class TargetMarkerView : MonoBehaviour
    {
        // FIELDS

        [Header("Visual")]
        [Tooltip("Child that holds the marker mesh or sprite. Bob and pop are applied here. Defaults to this transform.")]
        [SerializeField] private Transform _visualRoot;

        [Header("Placement")]
        [Tooltip("World-space height above the followed transform (the enemy's target focus point).")]
        [SerializeField] private float _heightOffset = 0.6f;

        [Tooltip("Rotates the marker to face the main camera every frame. Keep on for sprites, turn off for 3D meshes.")]
        [SerializeField] private bool _faceCamera = true;

        [Header("Bob")]
        [SerializeField, Min(0f)] private float _bobAmplitude = 0.08f;
        [SerializeField, Min(0f)] private float _bobFrequency = 3f;

        [Header("Pop On Target Change")]
        [SerializeField, Min(1f)] private float _popScale = 1.4f;
        [SerializeField, Min(0f)] private float _popDuration = 0.2f;

        private Transform _target;
        private Transform _cameraTransform;
        private Vector3 _visualBaseScale;
        private bool _hasCachedScale;
        private Tween _popTween;

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (_target == null)
            {
                Hide();
                return;
            }

            UpdatePosition();
        }

        private void OnDisable()
        {
            KillPopTween();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Shows the marker above the given transform and plays the pop.
        /// </summary>
        /// <param name="target">Transform to follow, usually the enemy's target focus point.</param>
        public void Show(Transform target)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            _target = target;

            CacheVisualScale();
            gameObject.SetActive(true);
            UpdatePosition();
            PlayPop();
        }

        /// <summary>
        /// Hides the marker and stops following.
        /// </summary>
        public void Hide()
        {
            _target = null;
            gameObject.SetActive(false);
        }

        // PRIVATE METHODS

        /// <summary>Caches the visual root and its authored scale once.</summary>
        private void CacheVisualScale()
        {
            if (_hasCachedScale)
                return;

            if (_visualRoot == null)
                _visualRoot = transform;

            _visualBaseScale = _visualRoot.localScale;
            _hasCachedScale = true;
        }

        /// <summary>Places the marker above the target with the bob offset applied.</summary>
        private void UpdatePosition()
        {
            float bob = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;

            transform.position = _target.position + Vector3.up * (_heightOffset + bob);

            if (_faceCamera)
                FaceCamera();
        }

        /// <summary>Aligns the marker with the main camera so a flat sprite is always seen head-on.</summary>
        private void FaceCamera()
        {
            if (_cameraTransform == null)
            {
                Camera mainCamera = Camera.main;

                if (mainCamera == null)
                    return;

                _cameraTransform = mainCamera.transform;
            }

            transform.rotation = _cameraTransform.rotation;
        }

        /// <summary>Scales the visual up and eases it back to its authored scale.</summary>
        private void PlayPop()
        {
            KillPopTween();

            _visualRoot.localScale = _visualBaseScale * _popScale;
            _popTween = _visualRoot.DOScale(_visualBaseScale, _popDuration).SetEase(Ease.OutBack);
        }

        /// <summary>Stops the pop and restores the authored scale.</summary>
        private void KillPopTween()
        {
            _popTween?.Kill();
            _popTween = null;

            if (_hasCachedScale)
                _visualRoot.localScale = _visualBaseScale;
        }
    }
}
