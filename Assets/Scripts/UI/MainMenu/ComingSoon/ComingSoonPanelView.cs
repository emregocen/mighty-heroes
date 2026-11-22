using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MightyHeroes.UI.MainMenu.ComingSoon
{
    /// <summary>
    /// Shared full-screen "Coming Soon" overlay for locked main menu features.
    /// The root holds a dimmed black backdrop, a sliced frame with margins and a
    /// single TMP text. Tapping anywhere closes the panel and returns to the menu.
    /// Keep the root inactive in the scene; Show activates it on top of the canvas.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ComingSoonPanelView : MonoBehaviour, IPointerClickHandler
    {
        // CONSTANTS

        /// <summary>Rich text layout: feature name, description and the "Coming Soon" line.</summary>
        private const string MessageFormat = "<color=#{2}>COMING SOON</color>\n<b><size=+20>{0}</size></b>\n{1}";

        // FIELDS

        [Header("References")]
        [Tooltip("Text inside the frame. Receives the feature name, description and the Coming Soon line.")]
        [SerializeField] private TMP_Text _messageText;

        [Tooltip("Frame transform, scaled in when the panel opens.")]
        [SerializeField] private RectTransform _frame;

        [Header("Style")]
        [Tooltip("Color of the 'COMING SOON' line.")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.82f, 0.48f, 1f);

        [Header("Animation")]
        [Tooltip("Fade duration in seconds (unscaled time).")]
        [SerializeField] private float _fadeDuration = 0.2f;

        [Tooltip("Frame scale at the start of the open animation.")]
        [SerializeField] private float _openStartScale = 0.9f;

        private CanvasGroup _canvasGroup;
        private Sequence _sequence;

        // PROPERTIES

        /// <summary>True while the panel is open or opening.</summary>
        public bool IsOpen { get; private set; }

        // UNITY CALLBACKS

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            IsOpen = false;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Opens the panel on top of the canvas with the given feature info.
        /// </summary>
        /// <param name="featureName">Feature title, e.g. "Guild".</param>
        /// <param name="description">Short English description of the feature.</param>
        public void Show(string featureName, string description)
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_messageText != null)
                _messageText.text = string.Format(MessageFormat, featureName, description, ColorUtility.ToHtmlStringRGBA(_highlightColor));

            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            IsOpen = true;

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;

            if (_frame != null)
                _frame.localScale = Vector3.one * _openStartScale;

            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.Join(_canvasGroup.DOFade(1f, _fadeDuration));

            if (_frame != null)
                _sequence.Join(_frame.DOScale(1f, _fadeDuration * 1.5f).SetEase(Ease.OutBack));
        }

        /// <summary>
        /// Fades the panel out and deactivates it.
        /// </summary>
        public void Hide()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            _canvasGroup.blocksRaycasts = false;

            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.Join(_canvasGroup.DOFade(0f, _fadeDuration));
            _sequence.OnComplete(() => gameObject.SetActive(false));
        }

        // EVENT HANDLERS

        /// <summary>Any tap on the backdrop, frame or text closes the panel.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
