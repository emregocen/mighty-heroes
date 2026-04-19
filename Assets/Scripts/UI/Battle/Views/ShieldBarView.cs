using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders shield values through a slider and optional text.
    /// This class only handles presentation and does not own gameplay state.
    /// </summary>
    public sealed class ShieldBarView : MonoBehaviour
    {
        // 0) CONSTANTS

        private const int MinSliderValue = 0;
        private const int DefaultMaxSliderValue = 1;

        // 1) FIELDS

        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private float _animationDuration = 0.20f;
        [SerializeField] private float _visibleAlpha = 1.00f;
        [SerializeField] private float _hiddenAlpha = 0.00f;

        private Tween _valueTween;

        // 3) CONSTRUCTOR / UNITY CALLBACKS

        private void Awake()
        {
            ResetView();
        }

        private void OnDisable()
        {
            KillTween();
        }

        private void OnDestroy()
        {
            KillTween();
        }

        // 4) PUBLIC METHODS

        /// <summary>
        /// Sets the displayed shield value.
        /// </summary>
        /// <param name="currentShield">Current shield value.</param>
        /// <param name="maxShield">Maximum shield value.</param>
        /// <param name="animate">Whether the slider value should animate.</param>
        public void SetValue(int currentShield, int maxShield, bool animate)
        {
            int safeMaxShield = Mathf.Max(DefaultMaxSliderValue, maxShield);
            int safeCurrentShield = Mathf.Clamp(currentShield, MinSliderValue, safeMaxShield);

            UpdateText(safeCurrentShield, safeMaxShield);
            UpdateSlider(safeCurrentShield, safeMaxShield, animate);
        }

        /// <summary>
        /// Sets the visibility state of the view.
        /// </summary>
        /// <param name="isVisible">Whether the view should be visible.</param>
        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _canvasGroup.alpha = isVisible ? _visibleAlpha : _hiddenAlpha;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        /// <summary>
        /// Resets the view to a safe default state.
        /// </summary>
        public void ResetView()
        {
            KillTween();

            UpdateText(0, DefaultMaxSliderValue);

            if (_slider != null)
            {
                _slider.minValue = MinSliderValue;
                _slider.maxValue = DefaultMaxSliderValue;
                _slider.value = MinSliderValue;
            }

            SetVisible(false);
        }

        // 6) PRIVATE METHODS

        /// <summary>
        /// Updates the shield text presentation.
        /// </summary>
        /// <param name="currentShield">Current shield value.</param>
        /// <param name="maxShield">Maximum shield value.</param>
        private void UpdateText(int currentShield, int maxShield)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = $"{currentShield}/{maxShield}";
        }

        /// <summary>
        /// Updates the shield slider presentation.
        /// </summary>
        /// <param name="currentShield">Current shield value.</param>
        /// <param name="maxShield">Maximum shield value.</param>
        /// <param name="animate">Whether the slider value should animate.</param>
        private void UpdateSlider(int currentShield, int maxShield, bool animate)
        {
            if (_slider == null)
            {
                return;
            }

            _slider.minValue = MinSliderValue;
            _slider.maxValue = maxShield;

            KillTween();

            if (!animate)
            {
                _slider.value = currentShield;
                return;
            }

            _valueTween = DOTween
                .To(() => _slider.value, value => _slider.value = value, currentShield, _animationDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        /// <summary>
        /// Stops the active tween if one exists.
        /// </summary>
        private void KillTween()
        {
            _valueTween?.Kill();
            _valueTween = null;
        }
    }
}