namespace MightyHeroes.Gameplay.Enemy.Payloads
{
    /// <summary>
    /// Payload raised when an enemy hit reaction animation
    /// has fully completed.
    ///
    /// This payload represents a PURE animation lifecycle signal.
    /// It does NOT imply:
    /// - Turn completion
    /// - Death or defeat
    /// - Combat resolution
    ///
    /// Its sole purpose is to inform other systems
    /// (e.g. Hero combo flow, combat synchronization)
    /// that the enemy has finished visually reacting to a hit.
    /// </summary>
    public sealed class EnemyHitReactionEndedPayload
    {
        // 1) PROPERTIES

        /// <summary>
        /// Runtime identifier of the enemy whose
        /// hit reaction animation has completed.
        ///
        /// This allows systems to:
        /// - Match the signal to a specific enemy
        /// - Support future multi-enemy scenarios
        /// </summary>
        public int EnemyId { get; }

        // 2) CONSTRUCTOR

        /// <summary>
        /// Creates a new enemy hit reaction ended payload.
        /// </summary>
        /// <param name="enemyId">
        /// Runtime identifier of the enemy whose
        /// hit reaction animation has ended.
        /// </param>
        public EnemyHitReactionEndedPayload(int enemyId)
        {
            EnemyId = enemyId;
        }
    }
}