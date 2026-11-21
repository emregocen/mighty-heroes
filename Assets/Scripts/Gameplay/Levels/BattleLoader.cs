using MightyHeroes.Core.Logging;
using MightyHeroes.Core.Scene;
using UnityEngine.SceneManagement;

namespace MightyHeroes.Gameplay.Levels
{
    /// <summary>
    /// Single entry point for starting a battle level from any scene (main menu,
    /// Victory next, Defeat retry).
    ///
    /// Selects the level in LevelSession, then loads the level's environment
    /// scene as the active scene and BattleScene additively on top, so the
    /// battle setup finds the environment when it initializes. A level without
    /// an environment scene loads BattleScene alone. Uses the fading
    /// SceneTransitionController when it exists, otherwise loads directly.
    /// </summary>
    public static class BattleLoader
    {
        // PUBLIC METHODS

        /// <summary>Starts the level at the given catalog index.</summary>
        /// <param name="catalog">Level catalog.</param>
        /// <param name="levelIndex">Catalog index of the level to play.</param>
        public static void LoadLevel(LevelCatalogData catalog, int levelIndex)
        {
            if (catalog == null || catalog.Count == 0)
            {
                GameLogger.Error(LogSourceType.UI, "BattleLoader: level catalog is missing or empty.");
                return;
            }

            int index = levelIndex < 0 ? 0 : (levelIndex >= catalog.Count ? catalog.Count - 1 : levelIndex);
            LevelData level = catalog.Levels[index];
            LevelSession.SelectLevel(index);

            string environmentScene = level != null ? level.EnvironmentSceneName : null;
            string[] scenes = string.IsNullOrEmpty(environmentScene) ? new[] { SceneNames.Battle } : new[] { environmentScene, SceneNames.Battle };

            GameLogger.Log(LogSourceType.UI, $"BattleLoader: loading level {index} ({string.Join(" + ", scenes)}).");

            if (SceneTransitionController.Instance != null)
            {
                SceneTransitionController.Instance.LoadScenes(scenes);
                return;
            }

            SceneManager.LoadScene(scenes[0], LoadSceneMode.Single);

            for (int i = 1; i < scenes.Length; i++)
                SceneManager.LoadScene(scenes[i], LoadSceneMode.Additive);
        }
    }
}
