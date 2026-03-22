namespace MightyHeroes.Battle.TurnFlow.Payloads
{
    /// <summary>
    /// Raised when the enemy phase selects the next enemy to act.
    /// Only the enemy whose EnemyId matches should start its turn-owned behavior.
    /// </summary>
    public readonly struct TurnEnemyStartedPayload
    {
        /// <summary>
        /// The enemy that is allowed to act right now.
        /// </summary>
        public int EnemyId { get; }

        public TurnEnemyStartedPayload(int enemyId)
        {
            EnemyId = enemyId;
        }
    }
}