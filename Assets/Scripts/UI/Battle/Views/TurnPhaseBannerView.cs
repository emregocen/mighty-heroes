using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders and animates the battle turn phase banner text.
    /// This class is presentation-only and must be triggered externally.
    /// </summary>
    public sealed class TurnPhaseBannerView : MonoBehaviour
    {
        // 0) CONSTANTS

        private const float HiddenAlpha = 0f;
        private const float VisibleAlpha = 1f;

        // 1) SERIALIZED FIELDS

        [Header("References")]
        [SerializeField] private TMP_Text _phaseText;
        [SerializeField] private GameObject _visualRoot;

        [Header("Texts")]
        [SerializeField] private string _heroTurnText = "PLAYER TURN!";
        [SerializeField] private string _enemyTurnText = "ENEMY TURN!";

        [Header("Shake Animation")]
        [SerializeField] private float _shakeDuration = 1.0f;
        [SerializeField] private float _shakeStrength = 6.0f;
        [SerializeField] private int _shakeVibrato = 18;
        [SerializeField] private float _shakeRandomness = 45.0f;
        [SerializeField] private bool _shakeFadeOut = true;

        [Header("Exit Animation")]
        [SerializeField] private float _exitDuration = 0.30f;
        [SerializeField] private float _exitScale = 0.30f;
        [SerializeField] private Ease _exitScaleEase = Ease.InBack;
        [SerializeField] private Ease _exitFadeEase = Ease.InQuad;

        // 2) PRIVATE FIELDS

        private Sequence _phaseSequence;
        private RectTransform _phaseRectTransform;
        private Vector3 _initialLocalScale;
        private Vector2 _initialAnchoredPosition;

        // 3) UNITY METHODS

        /// <summary>
        /// Caches references and initializes the hidden state.
        /// </summary>
        private void Awake()
        {
            if (_phaseText != null)
                _phaseRectTransform = _phaseText.rectTransform;

            if (_phaseRectTransform != null)
            {
                _initialLocalScale = _phaseRectTransform.localScale;
                _initialAnchoredPosition = _phaseRectTransform.anchoredPosition;
            }

            ResetVisualState();
        }

        /// <summary>
        /// Releases active tween resources on destroy.
        /// </summary>
        private void OnDestroy()
        {
            KillSequence();
        }

        // 4) PUBLIC METHODS

        /// <summary>
        /// Plays the hero turn banner animation.
        /// </summary>
        public void PlayHeroTurn()
        {
            Play(_heroTurnText);
        }

        /// <summary>
        /// Plays the enemy turn banner animation.
        /// </summary>
        public void PlayEnemyTurn()
        {
            Play(_enemyTurnText);
        }

        /// <summary>
        /// Plays the banner animation using the provided label.
        /// </summary>
        /// <param name="phaseLabel">The text shown on the banner.</param>
        public void Play(string phaseLabel)
        {
            if (_phaseText == null || _phaseRectTransform == null)
                return;

            KillSequence();

            if (_visualRoot != null)
                _visualRoot.SetActive(true);
            else
                gameObject.SetActive(true);

            _phaseText.text = phaseLabel;
            _phaseText.alpha = VisibleAlpha;
            _phaseRectTransform.localScale = _initialLocalScale;
            _phaseRectTransform.anchoredPosition = _initialAnchoredPosition;

            _phaseSequence = DOTween.Sequence();
            _phaseSequence.SetUpdate(true);

            _phaseSequence.Append(
                _phaseRectTransform.DOShakeAnchorPos(
                        _shakeDuration,
                        _shakeStrength,
                        _shakeVibrato,
                        _shakeRandomness,
                        false,
                        _shakeFadeOut)
                    .SetEase(Ease.Linear));

            _phaseSequence.Append(
                _phaseRectTransform.DOScale(_initialLocalScale * _exitScale, _exitDuration)
                    .SetEase(_exitScaleEase));

            _phaseSequence.Join(
                DOTween.To(
                        () => _phaseText.alpha,
                        alpha => _phaseText.alpha = alpha,
                        HiddenAlpha,
                        _exitDuration)
                    .SetEase(_exitFadeEase));

            _phaseSequence.OnComplete(HideImmediately);
        }

        /// <summary>
        /// Stops active animation and hides the banner immediately.
        /// </summary>
        public void HideImmediately()
        {
            KillSequence();
            ResetVisualState();

            if (_visualRoot != null)
                _visualRoot.SetActive(false);
            else
                gameObject.SetActive(false);
        }

        // 5) PRIVATE METHODS

        /// <summary>
        /// Resets the visual state to its default hidden values.
        /// </summary>
        private void ResetVisualState()
        {
            if (_phaseText != null)
                _phaseText.alpha = HiddenAlpha;

            if (_phaseRectTransform != null)
            {
                _phaseRectTransform.localScale = _initialLocalScale;
                _phaseRectTransform.anchoredPosition = _initialAnchoredPosition;
            }
        }

        /// <summary>
        /// Safely kills the current tween sequence.
        /// </summary>
        private void KillSequence()
        {
            if (_phaseSequence == null)
                return;

            if (_phaseSequence.IsActive())
                _phaseSequence.Kill();

            _phaseSequence = null;
        }
    }
}