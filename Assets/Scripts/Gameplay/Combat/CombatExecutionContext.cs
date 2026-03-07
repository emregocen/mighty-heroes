using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Combat
{
    /// <summary>
    /// Immutable data container describing a single combat execution.
    /// </summary>
    public sealed class CombatExecutionContext
    {
        // Properties

        /// <summary>
        /// Entity that initiated the attack.
        /// </summary>
        public ICombatAttacker Attacker { get; }

        /// <summary>
        /// Entity that receives the attack.
        /// </summary>
        public ICombatTarget Target { get; }

        /// <summary>
        /// Intensity value derived from puzzle match count or AI decision.
        /// </summary>
        public int AttackIntensity { get; }

        /// <summary>
        /// Direction from which the hit is applied.
        /// </summary>
        public HitDirection HitDirection { get; }

        // Constructor

        /// <summary>
        /// Creates a new combat execution context.
        /// </summary>
        public CombatExecutionContext(
            ICombatAttacker attacker,
            ICombatTarget target,
            int attackIntensity,
            HitDirection hitDirection)
        {
            Attacker = attacker;
            Target = target;
            AttackIntensity = attackIntensity;
            HitDirection = hitDirection;
        }
    }
}