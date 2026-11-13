namespace MightyHeroes.Gameplay.Ultimate.Payloads
{
    /// <summary>
    /// Raised by HeroUltimateState when the ultimate animation has finished,
    /// before the hero state machine transitions onward.
    /// </summary>
    public readonly struct UltimateEndedPayload
    {
        // PROPERTIES

        /// <summary>Hero that fired the ultimate.</summary>
        public int HeroEntityId { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates an ultimate ended payload.
        /// </summary>
        public UltimateEndedPayload(int heroEntityId)
        {
            HeroEntityId = heroEntityId;
        }
    }
}
