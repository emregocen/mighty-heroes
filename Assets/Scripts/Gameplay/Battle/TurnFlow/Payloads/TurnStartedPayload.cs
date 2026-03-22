namespace MightyHeroes.Battle.TurnFlow.Payloads
{
    /// <summary>
    /// Contains contextual data for the start of a turn cycle.
    /// </summary>
    public readonly struct TurnStartedPayload
    {
        // 1) PROPERTIES

        /// <summary>
        /// Sequential index of the current turn.
        /// </summary>
        public int TurnIndex { get; }

        /// <summary>
        /// Indicates whether this is the very first turn of the battle.
        /// </summary>
        public bool IsFirstTurn { get; }

        // 2) CONSTRUCTOR

        public TurnStartedPayload(int turnIndex, bool isFirstTurn)
        {
            TurnIndex = turnIndex;
            IsFirstTurn = isFirstTurn;
        }
    }
}