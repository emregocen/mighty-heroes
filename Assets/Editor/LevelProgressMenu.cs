using MightyHeroes.Gameplay.Level;
using UnityEditor;
using UnityEngine;

namespace MightyHeroes.EditorTools
{
    /// <summary>
    /// Editor menu commands for level progress during development.
    /// </summary>
    public static class LevelProgressMenu
    {
        // PUBLIC METHODS

        /// <summary>Clears saved level progress so only the first level is unlocked.</summary>
        [MenuItem("Mighty Heroes/Reset Level Progress")]
        public static void ResetLevelProgress()
        {
            LevelProgressStore.ResetProgress();
            Debug.Log("Mighty Heroes: level progress reset. Only the first level is unlocked.");
        }
    }
}
