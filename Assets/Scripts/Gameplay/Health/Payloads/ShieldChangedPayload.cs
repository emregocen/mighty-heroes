namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when an entity's shield value changes.
    /// </summary>
    public sealed class ShieldChangedPayload
    {
        /// <summary>
        /// Entity whose shield changed.
        /// </summary>
        public int EntityId { get; }

        /// <summary>
        /// Previous shield value.
        /// </summary>
        public int PreviousShield { get; }

        /// <summary>
        /// New shield value.
        /// </summary>
        public int CurrentShield { get; }

        /// <summary>
        /// Maximum shield at the time of change.
        /// </summary>
        public int MaxShield { get; }

        /// <summary>
        /// Optional source entity id. Can be 0 if unknown.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Creates a shield changed payload.
        /// </summary>
        public ShieldChangedPayload(int entityId, int previousShield, int currentShield, int maxShield, int sourceEntityId)
        {
            EntityId = entityId;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            MaxShield = maxShield;
            SourceEntityId = sourceEntityId;
        }
    }
}