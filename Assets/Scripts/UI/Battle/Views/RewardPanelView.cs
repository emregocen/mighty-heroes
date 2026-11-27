using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Reward popup opened by Collect Rewards. Fades in over the victory screen,
    /// pops the frame and counts the gold and XP up from zero. The OK button
    /// raises OkClicked. Keep the root inactive in the scene; Show activates it.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class RewardPanelView : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [Tooltip("Frame scaled in when the panel opens.")]
        [SerializeField] private RectTransform _frame;

        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _xpText;
        [SerializeField] private Button _okButton;

        [Header("Format")]
        [Tooltip("{0} is the gold amount.")]
        [SerializeField] private string _goldFormat = "+{0} Gold";

        [Tooltip("{0} is the XP amount.")]
        [SerializeField] private string _xpFormat = "+{0} XP";

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.25f;

        [Tooltip("Duration of the number count-up in seconds.")]
        [SerializeField] private float _countDuration = 0.8f;

        private CanvasGroup _canvasGroup;
        private Sequence _sequence;

        // PROPERTIES

        /// <summary>Raised when the OK button is pressed.</summary>
        public event Action OkClicked;

        // UNITY CALLBACKS

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_okButton != null)
                _okButton.onClick.AddListener(OnOkButtonClicked);
        }

        private void OnDestroy()
        {
            _sequence?.Kill();

            if (_okButton != null)
                _okButton.onClick.RemoveListener(OnOkButtonClicked);
        }

        // PUBLIC METHODS

        /// <summary>Opens the panel and counts the rewards up.</summary>
        /// <param name="gold">Gold earned.</param>
        /// <param name="xp">XP earned.</param>
        public void Show(int gold, int xp)
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            transform.SetAsLastSibling();
            gameObject.SetActive(true);

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = false;

            SetAmount(_goldText, _goldFormat, 0);
            SetAmount(_xpText, _xpFormat, 0);

            if (_frame != null)
                _frame.localScale = Vector3.one * 0.85f;

            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.Append(_canvasGroup.DOFade(1f, _fadeDuration));

            if (_frame != null)
                _sequence.Join(_frame.DOScale(1f, _fadeDuration * 1.5f).SetEase(Ease.OutBack));

            _sequence.Append(DOVirtual.Float(0f, gold, _countDuration, value => SetAmount(_goldText, _goldFormat, Mathf.RoundToInt(value))).SetEase(Ease.OutQuad));
            _sequence.Join(DOVirtual.Float(0f, xp, _countDuration, value => SetAmount(_xpText, _xpFormat, Mathf.RoundToInt(value))).SetEase(Ease.OutQuad));
            _sequence.AppendCallback(() => _canvasGroup.interactable = true);
        }

        // PRIVATE METHODS

        private static void SetAmount(TMP_Text text, string format, int value)
        {
            if (text != null)
                text.text = string.Format(format, value);
        }

        // EVENT HANDLERS

        private void OnOkButtonClicked()
        {
            _canvasGroup.interactable = false;
            OkClicked?.Invoke();
        }
    }
}
