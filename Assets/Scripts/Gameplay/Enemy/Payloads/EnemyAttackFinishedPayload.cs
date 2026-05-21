namespace MightyHeroes.Gameplay.Enemy.Payloads
{
    /// <summary>
    /// Carries data for an enemy attack sequence finished event.
    /// </summary>
    public readonly struct EnemyAttackFinishedPayload
    {
        // PROPERTIES

        /// <summary>
        /// Gets the entity identifier of the enemy that finished attacking.
        /// </summary>
        public int EnemyId { get; }

        // CONSTRUCTORS

        /// <summary>
        /// Creates a payload for an enemy attack sequence finish.
        /// </summary>
        /// <param name="enemyId">Entity identifier of the enemy that finished attacking.</param>
        public EnemyAttackFinishedPayload(int enemyId)
        {
            EnemyId = enemyId;
        }
    }
}