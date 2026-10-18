namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when a heal visual effect should be played on an entity.
    ///
    /// This is a presentation request, independent of whether the heal changed
    /// any health value, so the effect still plays when the target is at full
    /// health. The entity that receives the heal is identified by TargetEntityId,
    /// which allows companions to heal the hero in the future.
    /// </summary>
    public sealed class HealEffectRequestedPayload
    {
        /// <summary>
        /// Runtime id of the entity that should show the heal effect.
        /// </summary>
        public int TargetEntityId { get; }

        /// <summary>
        /// Runtime id of the entity providing the heal. Can be 0 if unknown.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Creates a heal effect requested payload.
        /// </summary>
        public HealEffectRequestedPayload(int targetEntityId, int sourceEntityId)
        {
            TargetEntityId = targetEntityId;
            SourceEntityId = sourceEntityId;
        }
    }
}