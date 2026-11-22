namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Carries the level chosen in one scene to the battle scene that plays it.
    ///
    /// The main menu, Victory (next battle) and Defeat (retry) set
    /// SelectedLevelIndex before loading the battle scene; the battle setup
    /// reads it. It holds only this one value and lives for the app session.
    /// Persistent progress is kept separately in LevelProgressStore.
    /// </summary>
    public static class LevelSession
    {
        // PROPERTIES

        /// <summary>
        /// Catalog index of the level to play. -1 means none was selected
        /// (for example when the battle scene is started directly in the Editor).
        /// </summary>
        public static int SelectedLevelIndex { get; private set; } = -1;

        // PUBLIC METHODS

        /// <summary>Selects the level the next battle scene load will play.</summary>
        /// <param name="levelIndex">Catalog index of the level.</param>
        public static void SelectLevel(int levelIndex)
        {
            SelectedLevelIndex = levelIndex;
        }
    }
}
