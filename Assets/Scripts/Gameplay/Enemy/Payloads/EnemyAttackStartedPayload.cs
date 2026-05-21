using UnityEngine;

namespace MightyHeroes.Gameplay.Enemy.Payloads
{
    /// <summary>
    /// Carries data for an enemy attack sequence start event.
    /// </summary>
    public readonly struct EnemyAttackStartedPayload
    {
        // PROPERTIES

        /// <summary>
        /// Gets the entity identifier of the attacking enemy.
        /// </summary>
        public int EnemyId { get; }

        /// <summary>
        /// Gets the transform that should be used as the visual focus point
        /// of the attacking enemy.
        /// </summary>
        public Transform AttackerFocusPoint { get; }

        // CONSTRUCTORS

        /// <summary>
        /// Creates a payload for an enemy attack sequence start.
        /// </summary>
        /// <param name="enemyId">Entity identifier of the attacking enemy.</param>
        /// <param name="attackerFocusPoint">Visual focus point of the attacking enemy.</param>
        public EnemyAttackStartedPayload(int enemyId, Transform attackerFocusPoint)
        {
            EnemyId = enemyId;
            AttackerFocusPoint = attackerFocusPoint;
        }
    }
}