using MightyHeroes.Core.Scene;
using MightyHeroes.Gameplay.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MightyHeroes.EditorTools
{
    /// <summary>
    /// Editor convenience: whenever BattleScene is opened on its own, the
    /// environment scene of the first level in the main level catalog is
    /// opened additively and made the active scene (its lighting is used), so
    /// the battle can be played directly from the Editor.
    ///
    /// Toggle with Mighty Heroes > Auto Open Battle Environment.
    /// </summary>
    [InitializeOnLoad]
    public static class BattleEnvironmentAutoLoader
    {
        // CONSTANTS

        private const string MenuPath = "Mighty Heroes/Auto Open Battle Environment";
        private const string EnabledKey = "MightyHeroes.AutoOpenBattleEnvironment";

        // PROPERTIES

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        // CONSTRUCTOR

        static BattleEnvironmentAutoLoader()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        // PUBLIC METHODS

        /// <summary>Turns the automatic environment opening on or off.</summary>
        [MenuItem(MenuPath)]
        public static void ToggleEnabled()
        {
            IsEnabled = !IsEnabled;
        }

        /// <summary>Shows the current state as a check mark in the menu.</summary>
        [MenuItem(MenuPath, true)]
        public static bool ValidateToggleEnabled()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        // PRIVATE METHODS

        /// <summary>Returns the environment scene name of the first catalog level, or null.</summary>
        private static string FindEnvironmentSceneName()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:LevelCatalogData"))
            {
                LevelCatalogData catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogData>(AssetDatabase.GUIDToAssetPath(guid));

                if (catalog != null && catalog.Count > 0 && catalog.Levels[0] != null && !string.IsNullOrEmpty(catalog.Levels[0].EnvironmentSceneName))
                    return catalog.Levels[0].EnvironmentSceneName;
            }

            return null;
        }

        /// <summary>Returns the asset path of a scene by name, or null.</summary>
        private static string FindScenePath(string sceneName)
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                    return path;
            }

            return null;
        }

        // EVENT HANDLERS

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!IsEnabled || mode != OpenSceneMode.Single || scene.name != SceneNames.Battle)
                return;

            string environmentName = FindEnvironmentSceneName();
            string environmentPath = environmentName != null ? FindScenePath(environmentName) : null;

            if (environmentPath == null)
                return;

            Scene environment = EditorSceneManager.OpenScene(environmentPath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(environment);
        }
    }
}
