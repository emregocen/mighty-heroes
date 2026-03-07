namespace MightyHeroes.Core.TurnSystem.Payloads
{
    /// <summary>
    /// Contains contextual data for the end of a turn cycle.
    /// </summary>
    public readonly struct TurnEndedPayload
    {
        // 1) PROPERTIES

        /// <summary>
        /// Sequential index of the turn that has just ended.
        /// </summary>
        public int TurnIndex { get; }

        /// <summary>
        /// Indicates whether the battle ended during this turn.
        /// </summary>
        public bool IsBattleEnded { get; }

        // 2) CONSTRUCTOR

        public TurnEndedPayload(int turnIndex, bool isBattleEnded)
        {
            TurnIndex = turnIndex;
            IsBattleEnded = isBattleEnded;
        }
    }
}