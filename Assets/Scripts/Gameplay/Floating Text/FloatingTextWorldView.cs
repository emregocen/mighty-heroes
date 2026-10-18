using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// World-space floating text: damage numbers, heal numbers, K.O., Overkill.
    /// Always faces the active camera (billboard). Spawned above the target
    /// entity's transform with a small random offset so simultaneous instances
    /// don't overlap exactly.
    ///
    /// Pooled by FloatingTextPool — Play/ForceReturn are the only entry points,
    /// this component never destroys itself.
    /// </summary>
    public sealed class FloatingTextWorldView : MonoBehaviour, IFloatingTextHandle
    {
        // FIELDS

        [SerializeField] private TextMeshPro _label;

        private Camera _billboardCamera;
        private Sequence _sequence;
        private System.Action<IFloatingTextHandle> _onReturnToPool;

        // PROPERTIES

        public bool IsActive { get; private set; }

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (!IsActive)
                return;

            Camera cam = _billboardCamera != null ? _billboardCamera : Camera.main;

            if (cam == null)
                return;

            Vector3 directionToCamera = cam.transform.position - transform.position;

            if (directionToCamera.sqrMagnitude < 0.0001f)
                return;

            // TextMeshPro's mesh is authored facing the opposite way (this is why
            // you had to manually add Y=180 rotation to read it correctly in a
            // static test) — so after computing the look-at rotation, we add a
            // 180-degree yaw to compensate for that authoring direction.
            transform.rotation = Quaternion.LookRotation(directionToCamera) * Quaternion.Euler(0f, 180f, 0f);
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Positions this instance at worldPosition plus a random offset, then
        /// plays punch-in, rise, and fade animation per settings. Calls
        /// onReturnToPool when the sequence completes.
        /// </summary>

        private Vector3 _debugWorldPos;
        public void PlayAt(Vector3 worldPosition, string text, FloatingTextConfig.TypeSettings settings,
            System.Action<IFloatingTextHandle> onReturnToPool)
        {
            _billboardCamera = Camera.main;
            _onReturnToPool = onReturnToPool;

            Vector3 randomOffset = Random.insideUnitSphere * settings.RandomOffsetRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y);

            transform.position = worldPosition + randomOffset;
            transform.localScale = Vector3.one * settings.StartScale;

            ApplyAppearance(settings);

            if (_label != null)
            {
                _label.text = text;
                _label.alpha = 1f;
            }

            gameObject.SetActive(true);
            IsActive = true;

            PlaySequence(settings);
        }

        /// <summary>Interface entry point — worldPosition must be set via PlayAt beforehand by the caller pattern used in this project (see FloatingTextPresenter).</summary>
        public void Play(string text, FloatingTextConfig.TypeSettings settings, System.Action<IFloatingTextHandle> onReturnToPool)
        {
            PlayAt(transform.position, text, settings, onReturnToPool);
        }

        public void ForceReturn()
        {
            _sequence?.Kill();
            ReturnToPool();
        }

        // PRIVATE METHODS

        private void PlaySequence(FloatingTextConfig.TypeSettings settings)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            float startTime = Time.time;
            Debug.Log($"[FloatingTextWorldView] PlaySequence START Lifetime={settings.Lifetime} FadeStartFraction={settings.FadeStartFraction}");

            _sequence.Append(
                transform.DOScale(1f, settings.PunchInDuration).SetEase(Ease.OutBack));

            _sequence.Join(
                transform.DOMoveY(transform.position.y + settings.RiseDistance,
                    settings.Lifetime).SetEase(Ease.OutSine));

            if (_label != null)
            {
                float fadeStart = settings.Lifetime * settings.FadeStartFraction;
                float fadeDuration = settings.Lifetime - fadeStart;

                _sequence.Insert(fadeStart,
                    DOTween.To(() => _label.alpha, a => _label.alpha = a, 0f, fadeDuration));
            }

            _sequence.OnComplete(() =>
            {
                Debug.Log($"[FloatingTextWorldView] PlaySequence COMPLETE after {Time.time - startTime:F2}s (expected {settings.Lifetime})");
                ReturnToPool();
            });
        }

        private void ReturnToPool()
        {
            IsActive = false;
            gameObject.SetActive(false);

            System.Action<IFloatingTextHandle> callback = _onReturnToPool;
            _onReturnToPool = null;
            callback?.Invoke(this);
        }

        /// <summary>
        /// Applies per-type appearance to the label: shared face/outline/underlay/glow
        /// style via TextStyleApplier (the same system used across the project's UI),
        /// plus per-context font size and weight that TextStyleConfig doesn't cover.
        /// </summary>
        private void ApplyAppearance(FloatingTextConfig.TypeSettings settings)
        {
            if (_label == null)
                return;

            if (settings.Style != null)
                TextStyleApplier.Apply(_label, settings.Style);

            _label.fontSize = settings.FontSize;
            _label.fontStyle = settings.FontStyle;
        }

    }


}