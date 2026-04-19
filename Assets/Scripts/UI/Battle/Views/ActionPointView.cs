using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Renders action points through toggle-based pip indicators and optional text.
    /// This class only handles presentation and does not own gameplay state.
    /// </summary>
    public sealed class ActionPointView : MonoBehaviour
    {
        // 1) FIELDS

        [Header("Pip Setup")]
        [SerializeField] private RectTransform _pipContainer;
        [SerializeField] private Toggle _pipPrefab;

        [Header("Optional Text")]
        [SerializeField] private TMP_Text _valueText;

        [Header("Visibility")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly List<Toggle> _pipInstances = new List<Toggle>();

        // 3) UNITY CALLBACKS

        private void Awake()
        {
            if (_pipPrefab != null)
            {
                _pipPrefab.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            ClearPips();
        }

        // 4) PUBLIC METHODS

        /// <summary>
        /// Sets the displayed action point values.
        /// </summary>
        /// <param name="currentActionPoints">Current action point value.</param>
        /// <param name="maxActionPoints">Maximum action point value.</param>
        public void SetValue(int currentActionPoints, int maxActionPoints)
        {
            int safeMax = Mathf.Max(0, maxActionPoints);
            int safeCurrent = Mathf.Clamp(currentActionPoints, 0, safeMax);

            SetPipCount(safeMax);
            RefreshPips(safeCurrent);
            RefreshText(safeCurrent, safeMax);
        }

        /// <summary>
        /// Sets the visibility state of the view.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets the interactable presentation state.
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = isInteractable ? 1f : 0.75f;
            _canvasGroup.interactable = isInteractable;
            _canvasGroup.blocksRaycasts = isInteractable;
        }

        /// <summary>
        /// Dims or brightens the view to indicate whether this entity's turn is active.
        /// Pip states and pip count are preserved — only visual opacity changes.
        /// Call this on turn start and turn end instead of SetVisible.
        /// </summary>
        /// <param name="isActive">True when this entity's turn is active, false otherwise.</param>
        public void SetTurnActive(bool isActive)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = isActive ? 1f : 0.75f;
        }

        /// <summary>
        /// Resets the view to default state.
        /// </summary>
        public void ResetView()
        {
            ClearPips();
            RefreshText(0, 0);
            SetVisible(true);
            SetInteractable(true);
        }

        // 6) PRIVATE METHODS

        /// <summary>
        /// Ensures the internal pip pool contains enough instances for the requested count.
        /// Existing extra instances are kept for reuse instead of being destroyed.
        /// </summary>
        /// <param name="maxCount">Required visible pip count.</param>
        private void SetPipCount(int maxCount)
        {
            if (_pipContainer == null || _pipPrefab == null)
            {
                return;
            }

            while (_pipInstances.Count < maxCount)
            {
                Toggle instance = Instantiate(_pipPrefab, _pipContainer);
                instance.gameObject.SetActive(true);
                instance.isOn = false;

                _pipInstances.Add(instance);
            }

            for (int i = 0; i < _pipInstances.Count; i++)
            {
                bool shouldBeActive = i < maxCount;

                if (_pipInstances[i] != null)
                {
                    _pipInstances[i].gameObject.SetActive(shouldBeActive);
                }
            }
        }

        /// <summary>
        /// Updates toggle states based on current AP.
        /// </summary>
        private void RefreshPips(int currentValue)
        {
            for (int i = 0; i < _pipInstances.Count; i++)
            {
                Toggle toggle = _pipInstances[i];

                if (toggle == null)
                    continue;

                bool isActive = i < currentValue;
                toggle.isOn = isActive;
            }
        }

        /// <summary>
        /// Updates optional text.
        /// </summary>
        private void RefreshText(int current, int max)
        {
            if (_valueText == null)
                return;

            _valueText.text = $"{current}/{max}";
        }

        /// <summary>
        /// Destroys all pip instances.
        /// </summary>
        private void ClearPips()
        {
            for (int i = 0; i < _pipInstances.Count; i++)
            {
                Toggle instance = _pipInstances[i];

                if (instance != null)
                    Destroy(instance.gameObject);
            }

            _pipInstances.Clear();
        }
    }
}