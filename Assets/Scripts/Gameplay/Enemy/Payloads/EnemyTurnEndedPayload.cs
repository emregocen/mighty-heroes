namespace MightyHeroes.Gameplay.Enemy.Payloads
{
    /// <summary>
    /// Payload raised by an EnemyStateMachine when a single enemy
    /// has fully completed its action phase for the current turn.
    ///
    /// This payload does NOT indicate that the entire enemy turn
    /// has ended. It only signals completion of one enemy.
    /// </summary>
    public sealed class EnemyTurnEndedPayload
    {
        /// <summary>
        /// Runtime identifier of the enemy that completed its turn.
        /// Used by the TurnEnemyState to advance sequencing.
        /// </summary>
        public int EnemyId { get; }

        /// <summary>
        /// Creates a new enemy turn completion payload.
        /// </summary>
        /// <param name="enemyId">Enemy runtime identifier.</param>
        public EnemyTurnEndedPayload(int enemyId)
        {
            EnemyId = enemyId;
        }
    }
}