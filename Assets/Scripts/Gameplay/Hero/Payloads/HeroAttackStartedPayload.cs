namespace MightyHeroes.Gameplay.Hero.Payloads
{
    /// <summary>
    /// Carries data for a hero attack sequence start event.
    /// </summary>
    public readonly struct HeroAttackStartedPayload
    {
        // PROPERTIES

        /// <summary>
        /// Gets the entity identifier of the attacking hero.
        /// </summary>
        public int HeroEntityId { get; }

        /// <summary>
        /// Gets the attack variant index.
        /// </summary>
        public int AttackVariant { get; }

        /// <summary>
        /// Gets the combo step count used by the attack.
        /// </summary>
        public int TotalSteps { get; }

        // CONSTRUCTORS

        /// <summary>
        /// Creates a hero attack started payload.
        /// </summary>
        /// <param name="heroEntityId">
        /// Entity identifier of the attacking hero.
        /// </param>
        /// <param name="attackVariant">
        /// Attack variant index.
        /// </param>
        /// <param name="totalSteps">
        /// Combo step count used by the attack.
        /// </param>
        public HeroAttackStartedPayload(
            int heroEntityId,
            int attackVariant,
            int totalSteps)
        {
            HeroEntityId = heroEntityId;
            AttackVariant = attackVariant;
            TotalSteps = totalSteps;
        }
    }
}