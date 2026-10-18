using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Hero.Payloads
{
    /// <summary>
    /// Payload published when the hero attack animation reaches its hit frame.
    ///
    /// This payload represents the exact authored animation moment at which
    /// combat resolution should occur (damage, VFX, hit reaction, etc.).
    ///
    /// IMPORTANT:
    /// - This is emitted PER COMBO STEP (step 1, step 2, ...).
    /// - It includes an AttackId so downstream systems can correlate
    ///   subsequent "reaction ended" and "step resolved" signals.
    /// </summary>
    public sealed class HeroAttackHitPayload
    {
        // PROPERTIES

        /// <summary>
        /// Correlation id for the whole combo attack sequence.
        /// All steps in the same combo share the same AttackId.
        /// </summary>
        public int AttackId { get; }

        /// <summary>
        /// 1-based index of the combo step that produced this hit frame.
        /// Example: 1, 2, 3...
        /// </summary>
        public int StepIndex { get; }

        /// <summary>
        /// Total number of steps in the combo sequence.
        /// Example: 3 means this combo has step 1/3, 2/3, 3/3.
        /// </summary>
        public int TotalSteps { get; }

        /// <summary>
        /// Intensity level used by combat to calculate damage.
        /// </summary>
        public int AttackIntensity { get; }

        /// <summary>
        /// Direction of the strike as authored in the attack animation.
        /// Used to drive target hit reaction direction selection.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Damage type for this attack sequence, resolved from the matched
        /// tile's ActionType (Attack → Physical, MagicAttack → Magical).
        /// </summary>
        public DamageType DamageType { get; }

        /// <summary>
        /// Creates a new hero attack hit payload.
        /// </summary>
        public HeroAttackHitPayload(
            int attackId,
            int stepIndex,
            int totalSteps,
            int attackIntensity,
            HitDirection hitDirection,
            DamageType damageType)
        {
            AttackId = attackId;
            StepIndex = stepIndex;
            TotalSteps = totalSteps;
            AttackIntensity = attackIntensity;
            HitDirection = hitDirection;
            DamageType = damageType;
        }
    }
}