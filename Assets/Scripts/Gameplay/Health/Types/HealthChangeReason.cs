namespace MightyHeroes.Gameplay.Health.Types
{
    /// <summary>
    /// Describes why a health value has changed.
    /// Used by UI, analytics, and debug systems to interpret HealthChangedEvent.
    /// </summary>
    public enum HealthChangeReason
    {
        /// <summary>Reason could not be determined or was not specified.</summary>
        Unknown = 0,

        /// <summary>Health decreased due to incoming combat damage.</summary>
        Damage = 1,

        /// <summary>Health increased due to a heal effect.</summary>
        Heal = 2,

        /// <summary>Health was restored after the entity was revived from death.</summary>
        Revive = 3,

        /// <summary>
        /// Health was clamped downward because MaxHealth was reduced via SetMaxHealth.
        /// Current health exceeded the new maximum and was adjusted to match.
        /// </summary>
        StatChange = 4,
    }
}