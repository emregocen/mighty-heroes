using MightyHeroes.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.MainMenu.ComingSoon
{
    /// <summary>
    /// Marks a main menu button as a locked feature. Shows an optional lock
    /// badge and opens the shared Coming Soon panel with this feature's info
    /// when the button is tapped.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class LockedFeatureButton : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [Tooltip("Shared Coming Soon panel in the scene.")]
        [SerializeField] private ComingSoonPanelView _comingSoonPanel;

        [Tooltip("Optional lock icon child, shown while the feature is locked.")]
        [SerializeField] private GameObject _lockBadge;

        [Header("Content")]
        [Tooltip("Feature title shown in the panel, e.g. Guild.")]
        [SerializeField] private string _featureName;

        [Tooltip("Short English description shown in the panel.")]
        [TextArea(2, 5)]
        [SerializeField] private string _description;

        private Button _button;

        // UNITY CALLBACKS

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);

            if (_lockBadge != null)
                _lockBadge.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        // EVENT HANDLERS

        private void OnButtonClicked()
        {
            if (_comingSoonPanel == null)
            {
                GameLogger.Warning(LogSourceType.UI, $"LockedFeatureButton on {name}: Coming Soon panel is not assigned.");
                return;
            }

            _comingSoonPanel.Show(_featureName, _description);
        }
    }
}
