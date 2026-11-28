namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Converts the number of turns taken to win into a 1-3 star rating using
    /// the level's turn limits. A win always gives at least one star.
    /// </summary>
    public static class BattleStarRating
    {
        // CONSTANTS

        /// <summary>Highest possible rating.</summary>
        public const int MaxStars = 3;

        /// <summary>Limits used when no level data is available.</summary>
        private const int DefaultThreeStarMaxTurns = 3;
        private const int DefaultTwoStarMaxTurns = 5;

        // PUBLIC METHODS

        /// <summary>Returns the star rating for a win in the given number of turns.</summary>
        /// <param name="level">Level that was won; null uses the default limits.</param>
        /// <param name="turnsTaken">Turns taken to win, counting the winning turn.</param>
        public static int Calculate(LevelData level, int turnsTaken)
        {
            int threeStarMaxTurns = level != null ? level.ThreeStarMaxTurns : DefaultThreeStarMaxTurns;
            int twoStarMaxTurns = level != null ? level.TwoStarMaxTurns : DefaultTwoStarMaxTurns;

            if (turnsTaken <= threeStarMaxTurns)
                return 3;

            if (turnsTaken <= twoStarMaxTurns)
                return 2;

            return 1;
        }
    }
}
