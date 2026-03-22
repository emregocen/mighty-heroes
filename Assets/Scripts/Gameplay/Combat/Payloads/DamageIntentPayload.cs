// File: Assets/Scripts/Gameplay/Combat/Payloads/DamageIntentPayload.cs

using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Raised by CombatSystem after damage has been calculated but before it is applied.
    ///
    /// This is the decoupling boundary between CombatSystem and entity components.
    /// CombatSystem raises this event and its job ends here.
    /// The target entity listens, identifies itself by EntityId, and applies the damage.
    ///
    /// Why this exists:
    /// Without this event, CombatSystem must hold direct references to entity components
    /// and call ReceiveDamage() on them. That creates a CombatSystem → Entity coupling
    /// which prevents testability, replay, PvP, and clean separation of concerns.
    ///
    /// Flow:
    /// CombatSystem raises DamageIntentPayload
    ///   → HeroComponent listens: if TargetEntityId matches → Health.ApplyDamage → HeroDamagedEvent
    ///   → EnemyComponent listens: if TargetEntityId matches → Health.ApplyDamage → EnemyDamagedEvent
    /// </summary>
    public sealed class DamageIntentPayload
    {
        /// <summary>
        /// Unique runtime identifier of the entity that should receive this damage.
        /// Entities filter by this field to self-identify as the target.
        /// </summary>
        public int TargetEntityId { get; }

        /// <summary>
        /// Unique runtime identifier of the attacking entity.
        /// Used for lifesteal, counter-attack, and analytics.
        /// </summary>
        public int AttackerEntityId { get; }

        /// <summary>
        /// Final resolved damage value calculated by CombatResolver.
        /// This value is ready to be applied directly to health.
        /// </summary>
        public int DamageAmount { get; }

        /// <summary>
        /// Category of damage. Determines shield interaction rules.
        /// </summary>
        public DamageType DamageType { get; }

        /// <summary>
        /// Direction of the incoming hit.
        /// Used by the target to select the correct hit reaction animation.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Combo step index that produced this damage intent.
        /// Forwarded into CombatHitResolvedPayload for combo synchronization.
        /// </summary>
        public int StepIndex { get; }

        /// <summary>
        /// Total steps in the combo that produced this damage intent.
        /// Forwarded into CombatHitResolvedPayload for combo synchronization.
        /// </summary>
        public int TotalSteps { get; }

        /// <summary>
        /// Creates a new damage intent payload.
        /// </summary>
        public DamageIntentPayload(
            int targetEntityId,
            int attackerEntityId,
            int damageAmount,
            DamageType damageType,
            HitDirection hitDirection,
            int stepIndex,
            int totalSteps)
        {
            TargetEntityId = targetEntityId;
            AttackerEntityId = attackerEntityId;
            DamageAmount = damageAmount;
            DamageType = damageType;
            HitDirection = hitDirection;
            StepIndex = stepIndex;
            TotalSteps = totalSteps;
        }
    }
}