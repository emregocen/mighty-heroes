namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when lifesteal healing is applied to a source entity based on dealt damage.
    /// This event does not compute lifesteal; it only reports the applied result.
    /// </summary>
    public sealed class LifestealAppliedPayload
    {
        /// <summary>
        /// Source entity that received lifesteal healing.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Target entity from which lifesteal was derived.
        /// </summary>
        public int TargetEntityId { get; }

        /// <summary>
        /// Amount healed by lifesteal.
        /// </summary>
        public int HealAmount { get; }

        /// <summary>
        /// Creates a lifesteal applied payload.
        /// </summary>
        public LifestealAppliedPayload(int sourceEntityId, int targetEntityId, int healAmount)
        {
            SourceEntityId = sourceEntityId;
            TargetEntityId = targetEntityId;
            HealAmount = healAmount;
        }
    }
}