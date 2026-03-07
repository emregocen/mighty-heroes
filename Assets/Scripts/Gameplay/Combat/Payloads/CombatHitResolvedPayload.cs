namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Emitted by CombatSystem after processing a hit intent (hero/enemy).
    /// This is the single source of truth for "this hit step is resolved".
    /// </summary>
    public readonly struct CombatHitResolvedPayload
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
        /// Combo step index from the original hit intent payload (1-based).
        /// </summary>
        public readonly int StepIndex;

        /// <summary>
        /// Total number of steps in the original hit intent payload's combo.
        /// This is needed to determine if the current hit is the last step of the combo.
        /// </summary>
        public readonly int TotalSteps;

        /// <summary>
        /// True if CombatSystem actually applied damage (target was alive).
        /// </summary>
        public readonly bool DamageApplied;

        /// <summary>
        /// True if target was alive before resolution.
        /// </summary>
        public readonly bool TargetWasAlive;

        /// <summary>
        /// True if target is alive after resolution.
        /// </summary>
        public readonly bool TargetIsAliveAfter;

        public CombatHitResolvedPayload(
            int attackerId,
            int targetId,
            int stepIndex,
            int totalSteps,
            bool damageApplied,
            bool targetWasAlive,
            bool targetIsAliveAfter)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            StepIndex = stepIndex;
            TotalSteps = totalSteps;
            DamageApplied = damageApplied;
            TargetWasAlive = targetWasAlive;
            TargetIsAliveAfter = targetIsAliveAfter;
        }
    }
}