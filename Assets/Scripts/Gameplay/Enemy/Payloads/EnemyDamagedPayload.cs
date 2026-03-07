using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Enemy.Payloads
{
    /// <summary>
    /// Payload raised when an enemy receives confirmed and resolved damage.
    ///
    /// This event represents a finalized combat outcome and is published
    /// only after all combat resolution steps have completed
    /// (e.g. no miss, block, dodge, or immunity).
    ///
    /// Responsibilities:
    /// - Carry domain-level damage information
    /// - Identify which enemy instance was damaged
    /// - Provide hit direction data for reaction logic
    ///
    /// This payload is DOMAIN-SAFE:
    /// - Does not reference Unity components
    /// - Can be used safely in replay, analytics, save/load, or PvP systems
    /// </summary>
    public sealed class EnemyDamagedPayload
    {
        /// <summary>
        /// Unique runtime identifier of the damaged enemy entity.
        /// This identifier is used to route the event to the correct EnemyStateMachine instance.
        /// </summary>
        public int TargetEnemyId { get; }

        /// <summary>
        /// Final resolved damage value applied to the enemy.
        /// This value has already passed all combat calculations.
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// Direction from which the hit was received.
        /// Used to select the appropriate hit reaction animation.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Creates a new enemy damaged payload.
        /// </summary>
        /// <param name="targetEnemyId">Unique runtime identifier of the damaged enemy.</param>
        /// <param name="damage">Resolved damage amount.</param>
        /// <param name="hitDirection">Direction from which the hit was received.</param>
        public EnemyDamagedPayload(
            int targetEnemyId,
            int damage,
            HitDirection hitDirection)
        {
            TargetEnemyId = targetEnemyId;
            Damage = damage;
            HitDirection = hitDirection;
        }
    }
}