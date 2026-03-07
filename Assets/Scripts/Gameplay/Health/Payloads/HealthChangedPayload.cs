using MightyHeroes.Gameplay.Health.Types;

namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when an entity's health value changes for any reason.
    /// </summary>
    public sealed class HealthChangedPayload
    {
        /// <summary>
        /// Entity whose health changed.
        /// </summary>
        public int EntityId { get; }

        /// <summary>
        /// Previous health value.
        /// </summary>
        public int PreviousHealth { get; }

        /// <summary>
        /// New health value.
        /// </summary>
        public int CurrentHealth { get; }

        /// <summary>
        /// Maximum health at the time of change.
        /// </summary>
        public int MaxHealth { get; }

        /// <summary>
        /// Reason for the health change.
        /// </summary>
        public HealthChangeReason Reason { get; }

        /// <summary>
        /// Optional source entity id. Can be 0 if unknown.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Creates a health changed payload.
        /// </summary>
        public HealthChangedPayload(
            int entityId,
            int previousHealth,
            int currentHealth,
            int maxHealth,
            HealthChangeReason reason,
            int sourceEntityId)
        {
            EntityId = entityId;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            Reason = reason;
            SourceEntityId = sourceEntityId;
        }
    }
}