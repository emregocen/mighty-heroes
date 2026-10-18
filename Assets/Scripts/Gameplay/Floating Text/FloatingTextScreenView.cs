using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Screen-space floating text: combo callouts. Fixed anchor position
    /// (top-center area of the HUD canvas), no billboard needed since it
    /// already lives on a screen-space canvas.
    /// </summary>
    public sealed class FloatingTextScreenView : MonoBehaviour, IFloatingTextHandle
    {
        // FIELDS

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _rectTransform;

        private Vector2 _restingAnchoredPosition;
        private Sequence _sequence;
        private System.Action<IFloatingTextHandle> _onReturnToPool;

        // PROPERTIES

        public bool IsActive { get; private set; }

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _restingAnchoredPosition = _rectTransform.anchoredPosition;
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        // PUBLIC METHODS

        public void Play(string text, FloatingTextConfig.TypeSettings settings, System.Action<IFloatingTextHandle> onReturnToPool)
        {
            _onReturnToPool = onReturnToPool;

            _rectTransform.anchoredPosition = _restingAnchoredPosition;
            _rectTransform.localScale = Vector3.one * settings.StartScale;
            _rectTransform.localRotation = Quaternion.identity;

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

            _sequence.Append(
                _rectTransform.DOScale(1f, settings.PunchInDuration).SetEase(Ease.OutBack));

            _sequence.Join(
                _rectTransform.DOAnchorPosY(
                    _restingAnchoredPosition.y + settings.RiseDistance,
                    settings.Lifetime).SetEase(Ease.OutSine));

            if (_label != null)
            {
                float fadeStart = settings.Lifetime * settings.FadeStartFraction;
                float fadeDuration = settings.Lifetime - fadeStart;

                _sequence.Insert(fadeStart,
                    DOTween.To(() => _label.alpha, a => _label.alpha = a, 0f, fadeDuration));
            }

            _sequence.OnComplete(ReturnToPool);
        }

        private void ReturnToPool()
        {
            IsActive = false;
            gameObject.SetActive(false);

            System.Action<IFloatingTextHandle> callback = _onReturnToPool;
            _onReturnToPool = null;
            callback?.Invoke(this);
        }
        
        /// <summary>Applies the same shared style system used by FloatingTextWorldView.</summary>
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