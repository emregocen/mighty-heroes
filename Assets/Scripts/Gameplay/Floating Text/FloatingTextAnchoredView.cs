using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Screen-space floating text anchored to a world position: damage, heal,
    /// shield, buff, K.O. and Overkill numbers.
    ///
    /// Lives on a screen-space canvas as a UI element. Every LateUpdate the
    /// world anchor is projected to screen space and the text is placed there,
    /// plus an animated pixel offset (random spread, stack, rise). Because the
    /// text is UI, its on-screen size does not depend on camera distance and it
    /// can never be cut by the camera near clip plane.
    ///
    /// Offsets in TypeSettings (RiseDistance, RandomOffsetRadius) are in canvas
    /// reference pixels and are scaled by Canvas.scaleFactor, so they look the
    /// same on every device resolution.
    ///
    /// Pooled by FloatingTextPool. PlayAt and ForceReturn are the only entry
    /// points; this component never destroys itself.
    /// </summary>
    public sealed class FloatingTextAnchoredView : MonoBehaviour, IFloatingTextHandle
    {
        // FIELDS

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _rectTransform;

        private Canvas _rootCanvas;
        private Camera _worldCamera;
        private Vector3 _worldAnchor;
        private Vector2 _baseOffset;
        private float _riseOffset;
        private Sequence _sequence;
        private System.Action<IFloatingTextHandle> _onReturnToPool;

        // PROPERTIES

        /// <summary>True while this instance is showing and not available in the pool.</summary>
        public bool IsActive { get; private set; }

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (IsActive)
                UpdateScreenPosition();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Anchors this instance to worldAnchor with an extra upward stack offset
        /// (canvas pixels), then plays punch-in, rise and fade per settings.
        /// Calls onReturnToPool when the sequence completes.
        /// </summary>
        /// <param name="worldAnchor">World position the text follows on screen.</param>
        /// <param name="stackOffset">Upward offset in canvas pixels from the stack tracker.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="settings">Per-type style and motion settings.</param>
        /// <param name="onReturnToPool">Callback invoked when the instance finishes.</param>
        public void PlayAt(Vector3 worldAnchor, float stackOffset, string text, FloatingTextConfig.TypeSettings settings, System.Action<IFloatingTextHandle> onReturnToPool)
        {
            ResolveReferences();

            _onReturnToPool = onReturnToPool;
            _worldAnchor = worldAnchor;
            _riseOffset = 0f;

            Vector2 randomOffset = Random.insideUnitCircle * settings.RandomOffsetRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y);
            _baseOffset = randomOffset + Vector2.up * stackOffset;

            float tilt = Random.Range(-settings.RandomTiltDegrees, settings.RandomTiltDegrees);
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            _rectTransform.localScale = Vector3.one * settings.StartScale;

            ApplyAppearance(settings);

            if (_label != null)
            {
                _label.text = text;
                _label.alpha = 1f;
            }

            gameObject.SetActive(true);
            IsActive = true;

            UpdateScreenPosition();
            PlaySequence(settings);
        }

        /// <summary>
        /// Interface entry point. Replays at the last world anchor with no stack
        /// offset; FloatingTextService always uses PlayAt instead.
        /// </summary>
        public void Play(string text, FloatingTextConfig.TypeSettings settings, System.Action<IFloatingTextHandle> onReturnToPool)
        {
            PlayAt(_worldAnchor, 0f, text, settings, onReturnToPool);
        }

        /// <summary>Stops the animation immediately and returns this instance to the pool.</summary>
        public void ForceReturn()
        {
            _sequence?.Kill();
            ReturnToPool();
        }

        // PRIVATE METHODS

        /// <summary>Caches the rect transform, root canvas and world camera.</summary>
        private void ResolveReferences()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_rootCanvas == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                _rootCanvas = canvas != null ? canvas.rootCanvas : null;
            }

            _worldCamera = Camera.main;
        }

        /// <summary>
        /// Projects the world anchor to screen space and places the text there,
        /// adding the animated pixel offset. Hides the label while the anchor is
        /// behind the camera.
        /// </summary>
        private void UpdateScreenPosition()
        {
            if (_worldCamera == null || _rootCanvas == null)
                return;

            Vector3 screenPoint = _worldCamera.WorldToScreenPoint(_worldAnchor);

            if (_label != null)
                _label.enabled = screenPoint.z > 0f;

            if (screenPoint.z <= 0f)
                return;

            Vector2 pixelOffset = (_baseOffset + Vector2.up * _riseOffset) * _rootCanvas.scaleFactor;
            Vector2 screenPosition = (Vector2)screenPoint + pixelOffset;

            Camera canvasCamera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;
            RectTransform parentRect = (RectTransform)_rectTransform.parent;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPosition, canvasCamera, out Vector3 worldPosition))
                _rectTransform.position = worldPosition;
        }

        /// <summary>Builds the punch-in, rise and fade sequence.</summary>
        private void PlaySequence(FloatingTextConfig.TypeSettings settings)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            _sequence.Append(_rectTransform.DOScale(1f, settings.PunchInDuration).SetEase(Ease.OutBack));

            _sequence.Join(DOTween.To(() => _riseOffset, value => _riseOffset = value, settings.RiseDistance, settings.Lifetime).SetEase(Ease.OutSine));

            if (_label != null)
            {
                float fadeStart = settings.Lifetime * settings.FadeStartFraction;
                float fadeDuration = settings.Lifetime - fadeStart;

                _sequence.Insert(fadeStart, DOTween.To(() => _label.alpha, alpha => _label.alpha = alpha, 0f, fadeDuration));
            }

            _sequence.OnComplete(ReturnToPool);
        }

        /// <summary>Hides this instance and hands it back to the pool.</summary>
        private void ReturnToPool()
        {
            IsActive = false;
            gameObject.SetActive(false);

            System.Action<IFloatingTextHandle> callback = _onReturnToPool;
            _onReturnToPool = null;
            callback?.Invoke(this);
        }

        /// <summary>
        /// Applies per-type appearance: shared face/outline/underlay/glow style via
        /// TextStyleApplier plus per-type font size and weight.
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
