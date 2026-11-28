using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Defeat screen content: ribbon drops in, emblem pops, the lose text
    /// punches in, then the Continue button appears. Every reference is
    /// optional, so the layout can change without code changes. Runs on
    /// unscaled time.
    /// </summary>
    public sealed class DefeatPanelView : MonoBehaviour
    {
        // FIELDS

        [Header("Header")]
        [Tooltip("Ribbon or banner that drops in from above.")]
        [SerializeField] private RectTransform _ribbon;

        [Tooltip("Emblem on the ribbon, popped in after the drop.")]
        [SerializeField] private RectTransform _emblem;

        [Header("Content")]
        [Tooltip("Lose text, e.g. YOU LOSE!")]
        [SerializeField] private TMP_Text _loseText;

        [SerializeField] private Button _continueButton;

        [Header("Timing")]
        [Tooltip("Ribbon drop distance in UI units.")]
        [SerializeField] private float _ribbonDropDistance = 600f;

        private Sequence _sequence;

        // PROPERTIES

        /// <summary>Raised when the Continue button is pressed.</summary>
        public event Action ContinueClicked;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        private void OnDestroy()
        {
            _sequence?.Kill();

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Hides the animated parts at once and plays the reveal after the given
        /// delay, so it starts when the defeat canvas has become visible.
        /// </summary>
        /// <param name="startDelay">Seconds to wait before the first animation.</param>
        public void Play(float startDelay)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.AppendInterval(startDelay);

            AppendHeader();
            AppendLoseText();
            AppendContinueButton();
        }

        // PRIVATE METHODS

        /// <summary>Ribbon drop and emblem pop.</summary>
        private void AppendHeader()
        {
            if (_ribbon != null)
            {
                Vector2 target = _ribbon.anchoredPosition;
                _ribbon.anchoredPosition = target + Vector2.up * _ribbonDropDistance;
                _sequence.Append(_ribbon.DOAnchorPos(target, 0.45f).SetEase(Ease.OutBack));
            }

            if (_emblem != null)
            {
                _emblem.localScale = Vector3.zero;
                _sequence.Append(_emblem.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }
        }

        /// <summary>Lose text scales down from large while fading in.</summary>
        private void AppendLoseText()
        {
            if (_loseText == null)
                return;

            RectTransform rect = _loseText.rectTransform;
            rect.localScale = Vector3.one * 2f;
            _loseText.alpha = 0f;

            _sequence.Append(rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            _sequence.Join(DOVirtual.Float(0f, 1f, 0.15f, value => _loseText.alpha = value));
        }

        /// <summary>Continue button pops in and becomes clickable at the end.</summary>
        private void AppendContinueButton()
        {
            if (_continueButton == null)
                return;

            Transform button = _continueButton.transform;
            button.localScale = Vector3.zero;
            _continueButton.interactable = false;

            _sequence.Append(button.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            _sequence.AppendCallback(() => _continueButton.interactable = true);
        }

        // EVENT HANDLERS

        private void OnContinueButtonClicked()
        {
            _continueButton.interactable = false;
            ContinueClicked?.Invoke();
        }
    }
}
