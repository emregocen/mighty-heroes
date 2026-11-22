using UnityEngine;

namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Persists level progress on the device with PlayerPrefs: the highest
    /// unlocked level index. Level 0 is always unlocked.
    ///
    /// Good enough for the demo; a save system can replace it later behind
    /// the same methods.
    /// </summary>
    public static class LevelProgressStore
    {
        // CONSTANTS

        private const string HighestUnlockedKey = "MightyHeroes.Progress.HighestUnlockedLevel";

        // PROPERTIES

        /// <summary>Highest unlocked level index (0 when nothing is saved).</summary>
        public static int HighestUnlockedLevel => PlayerPrefs.GetInt(HighestUnlockedKey, 0);

        // PUBLIC METHODS

        /// <summary>
        /// Unlocks the given level index if it is higher than the current
        /// highest unlocked level, and saves.
        /// </summary>
        /// <param name="levelIndex">Level index to unlock.</param>
        public static void UnlockLevel(int levelIndex)
        {
            if (levelIndex <= HighestUnlockedLevel)
                return;

            PlayerPrefs.SetInt(HighestUnlockedKey, levelIndex);
            PlayerPrefs.Save();
        }

        /// <summary>Clears saved progress so only level 0 is unlocked.</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(HighestUnlockedKey);
            PlayerPrefs.Save();
        }
    }
}
