namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Raised when an entity's pending attack buff percent changes
    /// (granted, replaced by a higher value, or consumed).
    /// </summary>
    public sealed class AttackBuffChangedPayload
    {
        // PROPERTIES

        /// <summary>Entity whose attack buff changed.</summary>
        public int EntityId { get; }

        /// <summary>Previous attack bonus percent.</summary>
        public int PreviousPercent { get; }

        /// <summary>New attack bonus percent. Zero means the buff was consumed.</summary>
        public int CurrentPercent { get; }

        /// <summary>Entity that granted or consumed the buff. 0 if unknown.</summary>
        public int SourceEntityId { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates an attack buff changed payload.
        /// </summary>
        public AttackBuffChangedPayload(int entityId, int previousPercent, int currentPercent, int sourceEntityId)
        {
            EntityId = entityId;
            PreviousPercent = previousPercent;
            CurrentPercent = currentPercent;
            SourceEntityId = sourceEntityId;
        }
    }
}
