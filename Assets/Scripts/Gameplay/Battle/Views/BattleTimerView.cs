using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders the battle timer remaining seconds.
    ///
    /// Shows the seconds as an integer string. When the warning flag is set,
    /// switches the label color and runs a scale pulse via DOTween. The pulse
    /// is killed when the warning flag clears or the view is disabled.
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

        [Header("Warning Pulse")]
        [SerializeField] private float _pulseScale = 1.18f;
        [SerializeField] private float _pulseDuration = 0.45f;
        [SerializeField] private Ease _pulseEase = Ease.InOutSine;

        private Tween _pulseTween;
        private bool _isWarningActive;

        // UNITY CALLBACKS

        private void OnDisable()
        {
            KillPulse();
        }

        // PUBLIC METHODS

        /// <summary>Shows the view and resets it to the supplied seconds value.</summary>
        public void Show(int initialSeconds)
        {
            if (_root != null)
                _root.SetActive(true);

            SetLabelText(initialSeconds);
            SetWarningActive(false);
        }

        /// <summary>Hides the view. Called when the battle ends or no timer is configured.</summary>
        public void Hide()
        {
            KillPulse();

            if (_root != null)
                _root.SetActive(false);
        }

        /// <summary>Updates the displayed seconds.</summary>
        public void SetRemainingSeconds(int seconds)
        {
            SetLabelText(seconds);
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