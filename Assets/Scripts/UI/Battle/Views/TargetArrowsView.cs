using System;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Two on-screen arrow buttons for touch target selection, placed at the
    /// left and right middle of the screen. They do the same job as the Q and E
    /// keys: select the previous or next alive enemy in the front line.
    ///
    /// Pure presentation. HeroTargetInputController subscribes to the click
    /// events and decides when the arrows are visible.
    /// </summary>
    public sealed class TargetArrowsView : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [Tooltip("Left arrow: selects the previous enemy (same as Q).")]
        [SerializeField] private Button _previousButton;

        [Tooltip("Right arrow: selects the next enemy (same as E).")]
        [SerializeField] private Button _nextButton;

        [Tooltip("Optional. When set, visibility fades this group; otherwise the buttons are activated and deactivated.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        // PROPERTIES

        /// <summary>Raised when the left arrow is pressed.</summary>
        public event Action PreviousRequested;

        /// <summary>Raised when the right arrow is pressed.</summary>
        public event Action NextRequested;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_previousButton != null)
                _previousButton.onClick.AddListener(OnPreviousClicked);

            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_previousButton != null)
                _previousButton.onClick.RemoveListener(OnPreviousClicked);

            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNextClicked);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Shows or hides both arrows. Hidden arrows do not block touches.
        /// </summary>
        /// <param name="isVisible">True to show the arrows.</param>
        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
                _canvasGroup.interactable = isVisible;
                _canvasGroup.blocksRaycasts = isVisible;
                return;
            }

            if (_previousButton != null)
                _previousButton.gameObject.SetActive(isVisible);

            if (_nextButton != null)
                _nextButton.gameObject.SetActive(isVisible);
        }

        // EVENT HANDLERS

        private void OnPreviousClicked()
        {
            PreviousRequested?.Invoke();
        }

        private void OnNextClicked()
        {
            NextRequested?.Invoke();
        }
    }
}
