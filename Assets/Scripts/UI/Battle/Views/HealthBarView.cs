using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders health values through a slider and optional text.
    /// This class only handles presentation and does not own gameplay state.
    /// </summary>
    public sealed class HealthBarView : MonoBehaviour
    {
        // CONSTANTS

        private const int MinSliderValue = 0;
        private const int DefaultMaxSliderValue = 1;

        // FIELDS

        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private float _animationDuration = 0.20f;
        [SerializeField] private bool _hideWhenZero = true;
        [SerializeField] private float _hideDelay = 0.2f;
        [SerializeField] private float _hideFadeDuration = 0.3f;

        private Tween _valueTween;
        private Tween _fadeTween;

        // PROPERTIES

        /// <summary>
        /// Gets or sets a value indicating whether the view is currently marked as dead.
        /// This flag is kept for compatibility with the current implementation.
        /// </summary>
        private bool IsDead { get; set; }

        // UNITY CALLBACKS

        /// <summary>
        /// Resets the view to its default presentation state on initialization.
        /// </summary>
        private void Awake()
        {
            ResetView();
        }

        /// <summary>
        /// Stops active tweens when the object becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            KillAllTweens();
        }

        /// <summary>
        /// Stops active tweens when the object is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            KillAllTweens();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Sets the displayed health value.
        /// </summary>
        /// <param name="currentHealth">Current health value.</param>
        /// <param name="maxHealth">Maximum health value.</param>
        /// <param name="animate">Whether the slider value should animate.</param>
        public void SetValue(int currentHealth, int maxHealth, bool animate)
        {
            int safeMaxHealth = Mathf.Max(DefaultMaxSliderValue, maxHealth);
            int safeCurrentHealth = Mathf.Clamp(currentHealth, MinSliderValue, safeMaxHealth);

            UpdateText(safeCurrentHealth, safeMaxHealth);
            UpdateSlider(safeCurrentHealth, safeMaxHealth, animate);

            if (!_hideWhenZero)
            {
                return;
            }

            if (safeCurrentHealth <= 0)
            {
                PlayHideAnimation();
            }
            else
            {
                ShowImmediate();
            }
        }

        /// <summary>
        /// Resets the view to a safe default state.
        /// </summary>
        public void ResetView()
        {
            KillAllTweens();

            UpdateText(0, DefaultMaxSliderValue);

            if (_slider != null)
            {
                _slider.minValue = MinSliderValue;
                _slider.maxValue = DefaultMaxSliderValue;
                _slider.value = MinSliderValue;
            }

            SetVisible(true);
        }

        // PRIVATE METHODS - VISIBILITY

        /// <summary>
        /// Sets the visibility state of the view immediately.
        /// </summary>
        /// <param name="isVisible">Whether the view should be visible.</param>
        private void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        /// <summary>
        /// Shows the health bar immediately and restores interaction state.
        /// </summary>
        private void ShowImmediate()
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(true);
                return;
            }

            KillFadeTween();

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Plays a delayed fade-out animation for the health bar.
        /// </summary>
        private void PlayHideAnimation()
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(false);
                return;
            }

            KillFadeTween();

            _fadeTween = DOTween.Sequence()
                .AppendInterval(_hideDelay)
                .Append(_canvasGroup.DOFade(0f, _hideFadeDuration))
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                });
        }

        // PRIVATE METHODS - PRESENTATION

        /// <summary>
        /// Updates the health text presentation.
        /// </summary>
        /// <param name="currentHealth">Current health value.</param>
        /// <param name="maxHealth">Maximum health value.</param>
        private void UpdateText(int currentHealth, int maxHealth)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = $"{currentHealth}/{maxHealth}";
        }

        /// <summary>
        /// Updates the health slider presentation.
        /// </summary>
        /// <param name="currentHealth">Current health value.</param>
        /// <param name="maxHealth">Maximum health value.</param>
        /// <param name="animate">Whether the slider value should animate.</param>
        private void UpdateSlider(int currentHealth, int maxHealth, bool animate)
        {
            if (_slider == null)
            {
                return;
            }

            _slider.minValue = MinSliderValue;
            _slider.maxValue = maxHealth;

            KillValueTween();

            if (!animate)
            {
                _slider.value = currentHealth;
                return;
            }

            _valueTween = DOTween
                .To(() => _slider.value, value => _slider.value = value, currentHealth, _animationDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        // PRIVATE METHODS - TWEEN LIFECYCLE

        /// <summary>
        /// Stops the active value tween if one exists.
        /// </summary>
        private void KillValueTween()
        {
            _valueTween?.Kill();
            _valueTween = null;
        }

        /// <summary>
        /// Stops the active fade tween if one exists.
        /// </summary>
        private void KillFadeTween()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
        }

        /// <summary>
        /// Stops all active tweens owned by this view.
        /// </summary>
        private void KillAllTweens()
        {
            KillValueTween();
            KillFadeTween();
        }
    }
}