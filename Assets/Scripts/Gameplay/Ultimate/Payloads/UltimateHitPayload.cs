namespace MightyHeroes.Gameplay.Ultimate.Payloads
{
    /// <summary>
    /// Raised by HeroUltimateState at the ultimate's effect frame. CombatSystem
    /// resolves damage against every target selected by TargetMode.
    /// </summary>
    public readonly struct UltimateHitPayload
    {
        // PROPERTIES

        /// <summary>Hero that fires the ultimate.</summary>
        public int HeroEntityId { get; }

        /// <summary>Tier of the triggering affinity match; used as attack intensity.</summary>
        public int Tier { get; }

        /// <summary>Multiplier applied to the regular attack damage of the tier.</summary>
        public float DamageMultiplier { get; }

        /// <summary>Which enemies are hit.</summary>
        public UltimateTargetMode TargetMode { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates an ultimate hit payload.
        /// </summary>
        public UltimateHitPayload(int heroEntityId, int tier, float damageMultiplier, UltimateTargetMode targetMode)
        {
            HeroEntityId = heroEntityId;
            Tier = tier;
            DamageMultiplier = damageMultiplier;
            TargetMode = targetMode;
        }
    }
}
