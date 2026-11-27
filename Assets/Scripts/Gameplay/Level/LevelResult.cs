namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Outcome of a won battle: how many turns it took and the stars it earned.
    /// </summary>
    public readonly struct LevelResult
    {
        // PROPERTIES

        /// <summary>Turns taken to win, counting the winning turn.</summary>
        public int TurnsTaken { get; }

        /// <summary>Stars earned for this win (1-3).</summary>
        public int Stars { get; }

        // CONSTRUCTOR

        /// <summary>Creates a level result.</summary>
        public LevelResult(int turnsTaken, int stars)
        {
            TurnsTaken = turnsTaken;
            Stars = stars;
        }
    }
}
