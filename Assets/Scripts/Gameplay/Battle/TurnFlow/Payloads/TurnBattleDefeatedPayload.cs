namespace MightyHeroes.Battle.TurnFlow.Payloads
{
    /// <summary>
    /// Raised when the hero dies and the battle ends in defeat.
    /// </summary>
    public sealed class TurnBattleDefeatedPayload
    {
        /// <summary>Number of enemies still alive when the hero died.</summary>
        public int RemainingEnemyCount { get; }

        public TurnBattleDefeatedPayload(int remainingEnemyCount)
        {
            RemainingEnemyCount = remainingEnemyCount;
        }
    }
}