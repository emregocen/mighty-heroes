using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Combat.Events
{
    /// <summary>
    /// Provides access to all combat-resolution-related events.
    ///
    /// Event flow:
    /// 1. DamageIntentEvent    — CombatSystem calculated damage, target should apply it.
    /// 2. CombatHitResolvedEvent — CombatSystem confirmed the hit outcome (alive/dead).
    /// 3. CombatDamageResolvedEvent — Optional, for floating numbers and VFX.
    /// </summary>
    public sealed class CombatEvents : EventGroupBase
    {
        /// <summary>
        /// Creates the combat events group using the shared event registry.
        /// </summary>
        /// <param name="registry">Shared event registry instance.</param>
        public CombatEvents(IEventRegistry registry)
            : base(registry)
        {
        }

        /// <summary>
        /// Raised by CombatSystem after damage has been calculated.
        /// Target entities listen to this event, match their EntityId,
        /// and apply the damage through their own health system.
        ///
        /// CombatSystem raises this and its job ends here.
        /// It does not call ReceiveDamage directly on any component.
        /// </summary>
        public GameEvent<CombatDamageIntentPayload> CombatDamageIntentEvent =>
            Registry.GetEvent<CombatDamageIntentPayload>();

        /// <summary>
        /// Raised after a hit step has been fully resolved.
        /// Consumed by attack states to drive combo flow and forced finish detection.
        /// </summary>
        public GameEvent<CombatHitResolvedPayload> CombatHitResolvedEvent =>
            Registry.GetEvent<CombatHitResolvedPayload>();

    }
}