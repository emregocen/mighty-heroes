namespace MightyHeroes.Battle.TurnFlow.Payloads
{
    /// <summary>
    /// Payload raised when all enemies are defeated and the hero wins the battle.
    /// Carries the final turn index for star rating and reward calculation.
    /// </summary>
    public sealed class TurnBattleVictoryPayload
    {
        /// <summary>
        /// The turn index at which the last enemy was defeated.
        /// Used by reward and rating systems to evaluate performance.
        /// </summary>
        public int FinalTurnIndex { get; }

        /// <summary>
        /// Creates a new BattleVictoryPayload.
        /// </summary>
        /// <param name="finalTurnIndex">Turn index when victory was achieved.</param>
        public TurnBattleVictoryPayload(int finalTurnIndex)
        {
            FinalTurnIndex = finalTurnIndex;
        }
    }
}