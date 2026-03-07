namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when a dead entity is revived.
    /// </summary>
    public sealed class EntityRevivedPayload
    {
        /// <summary>
        /// Entity that was revived.
        /// </summary>
        public int EntityId { get; }

        /// <summary>
        /// Health value after revive.
        /// </summary>
        public int CurrentHealth { get; }

        /// <summary>
        /// Optional source entity id. Can be 0 if unknown.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Creates an entity revived payload.
        /// </summary>
        public EntityRevivedPayload(int entityId, int currentHealth, int sourceEntityId)
        {
            EntityId = entityId;
            CurrentHealth = currentHealth;
            SourceEntityId = sourceEntityId;
        }
    }
}