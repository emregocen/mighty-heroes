using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Health.Payloads;

namespace MightyHeroes.Gameplay.Health.Events
{
    /// <summary>
    /// Provides access to all health-related gameplay events.
    /// Used to publish and listen to health, shield, death, revive and lifesteal signals.
    /// </summary>
    public sealed class HealthEvents : EventGroupBase
    {
        /// <summary>
        /// Creates the health events group using the shared event registry.
        /// </summary>
        /// <param name="registry">Shared event registry instance.</param>
        public HealthEvents(IEventRegistry registry)
            : base(registry)
        {
        }

        /// <summary>
        /// Raised when damage is applied to a target.
        /// Includes shield absorption and final applied amounts.
        /// </summary>
        public GameEvent<DamageAppliedPayload> DamageAppliedEvent =>
            Registry.GetEvent<DamageAppliedPayload>();

        /// <summary>
        /// Raised when an entity's health value changes.
        /// Used by UI and state machines to react to health updates.
        /// </summary>
        public GameEvent<HealthChangedPayload> HealthChangedEvent =>
            Registry.GetEvent<HealthChangedPayload>();

        /// <summary>
        /// Raised when an entity's shield value changes.
        /// </summary>
        public GameEvent<ShieldChangedPayload> ShieldChangedEvent =>
            Registry.GetEvent<ShieldChangedPayload>();

        /// <summary>
        /// Raised when an entity dies.
        /// State machines should listen to this and transition to a defeated/dead state.
        /// </summary>
        public GameEvent<EntityDiedPayload> EntityDiedEvent =>
            Registry.GetEvent<EntityDiedPayload>();

        /// <summary>
        /// Raised when an entity is revived.
        /// </summary>
        public GameEvent<EntityRevivedPayload> EntityRevivedEvent =>
            Registry.GetEvent<EntityRevivedPayload>();

        /// <summary>
        /// Raised when lifesteal healing is applied to the source entity.
        /// The payload reports the applied heal amount, not the calculation rules.
        /// </summary>
        public GameEvent<LifestealAppliedPayload> LifestealAppliedEvent =>
            Registry.GetEvent<LifestealAppliedPayload>();

        /// <summary>
        /// Raised when a heal visual effect should be played on a target entity.
        /// Consumed by HealEffectView. Independent of health value changes.
        /// </summary>
        public GameEvent<HealEffectRequestedPayload> HealEffectRequestedEvent =>
            Registry.GetEvent<HealEffectRequestedPayload>();
    }
}