using MightyHeroes.Core.Scene;
using MightyHeroes.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Entry point for the main menu scene.
    /// Wires the play button to the scene transition controller.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        // FIELDS

        [Header("UI")]
        [SerializeField] private Button _playButton;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_playButton == null)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MainMenuController: Play button reference is missing.");
                return;
            }

            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        // EVENT HANDLERS

        private void OnPlayButtonClicked()
        {
            if (SceneTransitionController.Instance == null)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MainMenuController: SceneTransitionController not found, loading directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Battle);
                return;
            }

            SceneTransitionController.Instance.LoadScene(SceneNames.Battle);
        }
    }
}