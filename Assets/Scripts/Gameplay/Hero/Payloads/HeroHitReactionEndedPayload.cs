namespace MightyHeroes.Gameplay.Hero.Payloads
{
    /// <summary>
    /// Raised when the hero hit reaction animation has fully completed.
    /// Used to synchronize enemy combo progression.
    /// </summary>
    public sealed class HeroHitReactionEndedPayload
    {
        public int HeroEntityId { get; }

        public HeroHitReactionEndedPayload(int heroEntityId)
        {
            HeroEntityId = heroEntityId;
        }
    }
}