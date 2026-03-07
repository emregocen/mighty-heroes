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
        // 1) PROPERTIES

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
        ///
        /// You can interpret this in two ways (choose ONE and be consistent):
        /// - "Total intensity" of the whole combo (e.g., 3 for a 3-hit combo)
        /// - "Current step intensity" (e.g., stepIndex)
        ///
        /// I recommend keeping it as "TotalIntensity" and letting combat scale per step if needed.
        /// </summary>
        public int AttackIntensity { get; }

        /// <summary>
        /// Direction of the strike as authored in the attack animation.
        /// Used to drive target hit reaction direction selection.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Creates a new hero attack hit payload.
        /// </summary>
        public HeroAttackHitPayload(
            int attackId,
            int stepIndex,
            int totalSteps,
            int attackIntensity,
            HitDirection hitDirection)
        {
            AttackId = attackId;
            StepIndex = stepIndex;
            TotalSteps = totalSteps;
            AttackIntensity = attackIntensity;
            HitDirection = hitDirection;
        }
    }
}