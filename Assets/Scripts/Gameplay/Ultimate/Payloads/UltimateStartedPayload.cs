namespace MightyHeroes.Gameplay.Ultimate.Payloads
{
    /// <summary>
    /// Raised by HeroUltimateState when the ultimate starts (gauge consumed,
    /// animation triggered). Used by presentation such as the ultimate camera
    /// and combat feedback to switch into ultimate mode.
    /// </summary>
    public readonly struct UltimateStartedPayload
    {
        // PROPERTIES

        /// <summary>Hero that fires the ultimate.</summary>
        public int HeroEntityId { get; }

        /// <summary>Tier of the triggering affinity match (1..3).</summary>
        public int Tier { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates an ultimate started payload.
        /// </summary>
        public UltimateStartedPayload(int heroEntityId, int tier)
        {
            HeroEntityId = heroEntityId;
            Tier = tier;
        }
    }
}
