using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when an entity's health reaches zero and the entity dies.
    /// 
    /// State machines, battle flow and UI systems can listen to this event
    /// to react to death transitions.
    /// </summary>
    public sealed class EntityDiedPayload
    {
        /// <summary>
        /// Identifier of the entity that died.
        /// </summary>
        public int EntityId { get; }

        /// <summary>
        /// Identifier of the entity that caused the death.
        /// Can be zero if unknown or not applicable.
        /// </summary>
        public int KillerEntityId { get; }

        /// <summary>
        /// Direction context of the final hit.
        /// Used for death animation reactions.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Creates a new death payload.
        /// </summary>
        /// <param name="entityId">Dead entity identifier.</param>
        /// <param name="killerEntityId">Killer entity identifier.</param>
        /// <param name="hitDirection">Direction of the killing blow.</param>
        public EntityDiedPayload(int entityId, int killerEntityId, HitDirection hitDirection)
        {
            EntityId = entityId;
            KillerEntityId = killerEntityId;
            HitDirection = hitDirection;
        }
    }
}