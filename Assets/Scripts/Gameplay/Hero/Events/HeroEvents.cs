using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Gameplay.Hero.Events
{
    /// <summary>
    /// Provides access to all hero-related gameplay events.
    /// Used to signal hero lifecycle milestones to other systems.
    /// </summary>
    public sealed class HeroEvents : EventGroupBase
    {
         /// <summary>
        /// Creates the hero events group using the shared event registry.
        /// </summary>
        /// <param name="registry">Shared event registry instance.</param>
        public HeroEvents(IEventRegistry registry)
            : base(registry)
        {
        }

        /// <summary>
        /// Raised when the hero completes their action phase.
        /// Used by the Turn FSM to advance the turn flow.
        /// </summary>
        public GameEvent<HeroTurnEndedPayload> HeroTurnEndedEvent =>
            Registry.GetEvent<HeroTurnEndedPayload>();

        /// <summary>
        /// Raised when hero hit reaction animation has fully completed.
        ///
        /// This event is critical for synchronizing
        /// hero combo flow with enemy reactions.
        /// </summary>
        public GameEvent<HeroHitReactionEndedPayload> HeroHitReactionEndedEvent =>
            Registry.GetEvent<HeroHitReactionEndedPayload>();

        /// <summary>
        /// 
        /// </summary>
        public GameEvent<HeroAttackHitPayload> HeroAttackHitEvent =>
            Registry.GetEvent<HeroAttackHitPayload>();
    }
}