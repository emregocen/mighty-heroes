using System.Collections.Generic;
using UnityEngine;

namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Persists level progress on the device with PlayerPrefs: the highest
    /// unlocked level index and the best star rating of each level.
    /// Level 0 is always unlocked. Stars never decrease.
    ///
    /// Good enough for the demo; a save system can replace it later behind
    /// the same methods.
    /// </summary>
    public static class LevelProgressStore
    {
        // CONSTANTS

        private const string HighestUnlockedKey = "MightyHeroes.Progress.HighestUnlockedLevel";

        /// <summary>Best stars per level index, saved as a comma-separated list.</summary>
        private const string BestStarsKey = "MightyHeroes.Progress.BestStars";

        private const char StarsSeparator = ',';

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

        /// <summary>Returns the best stars earned on a level (0 when never won).</summary>
        /// <param name="levelIndex">Level index.</param>
        public static int GetBestStars(int levelIndex)
        {
            List<int> stars = LoadStars();
            return levelIndex >= 0 && levelIndex < stars.Count ? stars[levelIndex] : 0;
        }

        /// <summary>
        /// Saves the stars for a level only if they beat the saved best,
        /// so earned stars are never lost.
        /// </summary>
        /// <param name="levelIndex">Level index.</param>
        /// <param name="stars">Stars earned in this win.</param>
        public static void RecordStars(int levelIndex, int stars)
        {
            if (levelIndex < 0)
                return;

            List<int> saved = LoadStars();

            while (saved.Count <= levelIndex)
                saved.Add(0);

            if (stars <= saved[levelIndex])
                return;

            saved[levelIndex] = stars;
            PlayerPrefs.SetString(BestStarsKey, string.Join(StarsSeparator.ToString(), saved));
            PlayerPrefs.Save();
        }

        /// <summary>Clears saved progress so only level 0 is unlocked and no stars are kept.</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(HighestUnlockedKey);
            PlayerPrefs.DeleteKey(BestStarsKey);
            PlayerPrefs.Save();
        }

        // PRIVATE METHODS

        /// <summary>Reads the saved best stars list; unreadable entries count as 0.</summary>
        private static List<int> LoadStars()
        {
            var stars = new List<int>();
            string saved = PlayerPrefs.GetString(BestStarsKey, string.Empty);

            if (string.IsNullOrEmpty(saved))
                return stars;

            foreach (string entry in saved.Split(StarsSeparator))
                stars.Add(int.TryParse(entry, out int value) ? value : 0);

            return stars;
        }
    }
}
