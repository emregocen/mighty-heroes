using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Combat.Events
{
    /// <summary>
    /// Provides access to all combat-resolution-related events.
    ///
    /// These events represent the authoritative results of
    /// combat intent processing performed by CombatSystem.
    ///
    /// Combat events do not control animations or state machines.
    /// They only communicate resolved outcomes.
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
        /// Raised after a combat hit intent has been fully resolved.
        ///
        /// This event is emitted by CombatSystem for every hit frame,
        /// even if the target was already dead.
        ///
        /// It represents the authoritative resolution result of that hit step.
        ///
        /// Consumers:
        /// - HeroAttackState (to decide combo progression or forced finish)
        /// - EnemyAttackState
        /// - Damage UI systems
        /// </summary>
        public GameEvent<CombatHitResolvedPayload> CombatHitResolvedEvent =>
            Registry.GetEvent<CombatHitResolvedPayload>();

        /// <summary>
        /// Raised when final damage has been calculated and confirmed,
        /// before or after it is applied to the target.
        ///
        /// This event is optional and can be used for:
        /// - Floating damage numbers
        /// - Sound effects
        /// - Analytics
        ///
        /// It does not imply that the target survived the hit.
        /// </summary>
        public GameEvent<CombatDamageResolvedPayload> CombatDamageResolvedEvent =>
            Registry.GetEvent<CombatDamageResolvedPayload>();
    }
}