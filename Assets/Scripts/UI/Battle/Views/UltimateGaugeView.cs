using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders the hero's ultimate gauge: a slider filled by matched affinity
    /// tiles, a "current/capacity" label and a ready state when the gauge is full.
    ///
    /// Ready state: the optional highlight object is shown and the optional pulse
    /// target scales in a loop, telling the player that the next affinity match
    /// fires the ultimate. Both are optional; the bar works without them.
    ///
    /// Pure presentation, driven by UltimateGaugePresenter. Awake only applies the
    /// empty state when no value has been pushed yet, because Awake order is
    /// undefined relative to BattleBootstrapper.
    /// </summary>
    public sealed class UltimateGaugeView : MonoBehaviour
    {
        // CONSTANTS

        private const int DefaultCapacity = 1;

        // FIELDS

        [Header("References")]
        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _valueText;

        [Header("Ready State (optional)")]
        [Tooltip("Shown only while the gauge is full (glow, frame, 'READY' label).")]
        [SerializeField] private GameObject _readyHighlight;

        [Tooltip("Scaled in a loop while the gauge is full. Leave empty for no pulse.")]
        [SerializeField] private RectTransform _pulseTarget;

        [SerializeField] private float _pulseScale = 1.06f;
        [SerializeField] private float _pulseDuration = 0.5f;

        [Header("Animation")]
        [SerializeField] private float _fillDuration = 0.25f;

        [Header("Visibility")]
        [SerializeField] private float _visibleAlpha = 1f;
        [SerializeField] private float _hiddenAlpha = 0f;

        private Tween _fillTween;
        private Tween _pulseTween;
        private bool _isReady;
        private bool _hasOwnerState;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (!_hasOwnerState)
                ResetView();
        }

        private void OnDisable()
        {
            KillFillTween();
            StopPulse();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Updates the bar, the label and the ready state.
        /// </summary>
        /// <param name="current">Current gauge value.</param>
        /// <param name="capacity">Gauge capacity.</param>
        /// <param name="animate">Ease the slider to the new value.</param>
        public void SetValue(int current, int capacity, bool animate)
        {
            _hasOwnerState = true;

            int safeCapacity = Mathf.Max(DefaultCapacity, capacity);
            int safeCurrent = Mathf.Clamp(current, 0, safeCapacity);

            if (_valueText != null)
                _valueText.text = $"{safeCurrent}/{safeCapacity}";

            UpdateSlider(safeCurrent, safeCapacity, animate);
            SetReady(safeCurrent >= safeCapacity);
        }

        /// <summary>Shows or hides the whole gauge.</summary>
        public void SetVisible(bool isVisible)
        {
            _hasOwnerState = true;
            ApplyVisibility(isVisible);
        }

        // PRIVATE METHODS

        /// <summary>Applies the empty, hidden default state without marking owner state.</summary>
        private void ResetView()
        {
            KillFillTween();

            if (_slider != null)
            {
                _slider.minValue = 0;
                _slider.maxValue = DefaultCapacity;
                _slider.value = 0;
            }

            if (_valueText != null)
                _valueText.text = string.Empty;

            SetReady(false);
            ApplyVisibility(false);
        }

        private void UpdateSlider(int current, int capacity, bool animate)
        {
            if (_slider == null)
                return;

            KillFillTween();
            _slider.minValue = 0;
            _slider.maxValue = capacity;

            if (!animate || _fillDuration <= 0f || !isActiveAndEnabled)
            {
                _slider.value = current;
                return;
            }

            _fillTween = _slider.DOValue(current, _fillDuration).SetEase(Ease.OutCubic).SetLink(gameObject);
        }

        /// <summary>Turns the ready highlight and pulse on or off.</summary>
        private void SetReady(bool isReady)
        {
            if (_readyHighlight != null)
                _readyHighlight.SetActive(isReady);

            if (_isReady == isReady)
                return;

            _isReady = isReady;

            if (isReady)
                StartPulse();
            else
                StopPulse();
        }

        private void StartPulse()
        {
            StopPulse();

            if (_pulseTarget == null || !isActiveAndEnabled)
                return;

            _pulseTarget.localScale = Vector3.one;
            _pulseTween = _pulseTarget.DOScale(_pulseScale, _pulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        private void StopPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = null;

            if (_pulseTarget != null)
                _pulseTarget.localScale = Vector3.one;
        }

        private void KillFillTween()
        {
            _fillTween?.Kill();
            _fillTween = null;
        }

        /// <summary>Applies visibility through the CanvasGroup when assigned, otherwise by toggling the GameObject.</summary>
        private void ApplyVisibility(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _canvasGroup.alpha = isVisible ? _visibleAlpha : _hiddenAlpha;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
