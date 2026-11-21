using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Entry point for the main menu scene.
    /// The play button starts the highest unlocked level through BattleLoader,
    /// which loads the level's environment scene and BattleScene.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        // FIELDS

        [Header("UI")]
        [SerializeField] private Button _playButton;

        [Header("Levels")]
        [Tooltip("Levels in play order. Play starts the highest unlocked one.")]
        [SerializeField] private LevelCatalogData _levelCatalog;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_playButton == null)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MainMenuController: Play button reference is missing.");
                return;
            }
            GameLogger.Log(LogSourceType.UI, "MainMenuController: Play button reference found, wiring up click event.");
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
            int levelIndex = LevelProgressStore.HighestUnlockedLevel;

            GameLogger.Log(LogSourceType.UI, $"MainMenuController: Play clicked, starting level {levelIndex}.");
            BattleLoader.LoadLevel(_levelCatalog, levelIndex);
        }
    }
}