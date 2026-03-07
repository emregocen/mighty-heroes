using MightyHeroes.Gameplay.Combat.Interfaces;

namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Resolves combat rules and converts attack intent
    /// into finalized damage values.
    /// </summary>
    public interface ICombatResolver
    {
        /// <summary>
        /// Calculates final damage for an attack.
        /// </summary>
        /// <param name="attacker">Source of the attack.</param>
        /// <param name="target">Target receiving the attack.</param>
        /// <param name="attackIntensity">Attack intensity level.</param>
        /// <returns>Final resolved damage value.</returns>
        int ResolveDamage(
            ICombatAttacker attacker,
            ICombatTarget target,
            int attackIntensity
        );
    }
}