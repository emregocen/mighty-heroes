namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Represents the calculated damage result before or after it is applied.
    ///
    /// This payload is useful for UI systems such as floating damage numbers
    /// and visual feedback.
    /// </summary>
    public readonly struct CombatDamageResolvedPayload
    {
        /// <summary>
        /// ID of the attacker (hero or enemy) as defined in their respective combat entities.
        /// </summary>
        public readonly int AttackerId;
        
        /// <summary>
        /// ID of the target (hero or enemy) as defined in their respective combat entities.
        /// </summary>
        public readonly int TargetId;
        
        /// <summary>
        /// Final damage amount calculated by CombatSystem for this hit step.
        /// This is the actual damage that will be applied to the target's health.
        /// It can be zero if the target was already dead or if the attack had no damage.
        /// </summary>
        public readonly int DamageAmount;

        public CombatDamageResolvedPayload(
            int attackerId,
            int targetId,
            int damageAmount)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            DamageAmount = damageAmount;
        }
    }
}