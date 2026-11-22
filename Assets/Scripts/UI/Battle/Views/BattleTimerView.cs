using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders the battle timer remaining seconds.
    ///
    /// Shows the seconds as an integer string. When the warning flag is set,
    /// switches the label color and runs a scale pulse via DOTween. The pulse
    /// is killed when the warning flag clears or the view is disabled.
    ///
    /// Optional radial progress: a Filled Image (Radial 360) and/or a Slider
    /// show the remaining time as a 0..1 ratio of the total duration. It is
    /// full when the timer starts and empty exactly when the timer expires,
    /// whatever the duration. Both references may be left empty.
    ///
    /// Owns no logic — controlled entirely by BattleTimerPresenter.
    /// </summary>
    public sealed class BattleTimerView : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = new Color(1f, 0.35f, 0.35f, 1f);

        [Header("Radial Progress (optional)")]
        [Tooltip("Image with Image Type = Filled (e.g. Radial 360). Its fill amount shows the remaining time ratio.")]
        [SerializeField] private Image _radialFill;

        [Tooltip("Slider whose normalized value shows the remaining time ratio.")]
        [SerializeField] private Slider _radialSlider;

        [Tooltip("Seconds the ring takes to ease to each new value.")]
        [SerializeField, Min(0f)] private float _radialStepDuration = 0.25f;

        [Header("Warning Pulse")]
        [SerializeField] private float _pulseScale = 1.18f;
        [SerializeField] private float _pulseDuration = 0.45f;
        [SerializeField] private Ease _pulseEase = Ease.InOutSine;

        private Tween _pulseTween;
        private Tween _radialTween;
        private bool _isWarningActive;
        private float _radialProgress = 1f;

        // UNITY CALLBACKS

        private void OnDisable()
        {
            KillPulse();
            KillRadialTween();
        }

        // PUBLIC METHODS

        /// <summary>Shows the view and resets it to the supplied seconds value.</summary>
        public void Show(int initialSeconds)
        {
            if (_root != null)
                _root.SetActive(true);

            SetLabelText(initialSeconds);
            SetWarningActive(false);
            SetRemainingRatio(1f, animate: false);
        }

        /// <summary>Hides the view. Called when the battle ends or no timer is configured.</summary>
        public void Hide()
        {
            KillPulse();
            KillRadialTween();

            if (_root != null)
                _root.SetActive(false);
        }

        /// <summary>Updates the displayed seconds.</summary>
        public void SetRemainingSeconds(int seconds)
        {
            SetLabelText(seconds);
        }

        /// <summary>
        /// Updates the radial progress to the remaining time ratio (1 = full, 0 = expired).
        /// Values are clamped to 0..1. Does nothing when no radial reference is assigned.
        /// </summary>
        /// <param name="ratio">Remaining time divided by total duration.</param>
        /// <param name="animate">Ease to the value over the step duration instead of snapping.</param>
        public void SetRemainingRatio(float ratio, bool animate)
        {
            if (_radialFill == null && _radialSlider == null)
                return;

            float target = Mathf.Clamp01(ratio);

            KillRadialTween();

            if (!animate || _radialStepDuration <= 0f || !isActiveAndEnabled)
            {
                ApplyRadialProgress(target);
                return;
            }

            _radialTween = DOTween.To(() => _radialProgress, ApplyRadialProgress, target, _radialStepDuration).SetEase(Ease.OutSine).SetLink(gameObject);
        }

        /// <summary>
        /// Toggles warning visuals: red color + scale pulse loop when active,
        /// normal color + no pulse when inactive.
        /// </summary>
        public void SetWarningActive(bool isActive)
        {
            if (_isWarningActive == isActive)
                return;

            _isWarningActive = isActive;

            if (_label != null)
                _label.color = isActive ? _warningColor : _normalColor;

            if (isActive)
                StartPulse();
            else
                KillPulse();
        }

        // PRIVATE METHODS

        private void SetLabelText(int seconds)
        {
            if (_label != null)
                _label.text = seconds.ToString();
        }

        /// <summary>Writes the progress value to the assigned radial image and/or slider.</summary>
        private void ApplyRadialProgress(float value)
        {
            _radialProgress = value;

            if (_radialFill != null)
                _radialFill.fillAmount = value;

            if (_radialSlider != null)
                _radialSlider.normalizedValue = value;
        }

        private void KillRadialTween()
        {
            _radialTween?.Kill();
            _radialTween = null;
        }

        private void StartPulse()
        {
            KillPulse();

            if (_label == null)
                return;

            _label.transform.localScale = Vector3.one;

            _pulseTween = _label.transform
                .DOScale(_pulseScale, _pulseDuration)
                .SetEase(_pulseEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void KillPulse()
        {
            if (_pulseTween == null)
                return;

            _pulseTween.Kill();
            _pulseTween = null;

            if (_label != null)
                _label.transform.localScale = Vector3.one;
        }
    }
}