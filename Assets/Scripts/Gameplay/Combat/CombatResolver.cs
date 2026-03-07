using MightyHeroes.Gameplay.Combat.Interfaces;

namespace MightyHeroes.Gameplay.Combat
{
    /// <summary>
    /// Default combat damage resolver.
    /// Provides a deterministic and minimal damage formula.
    /// </summary>
    public sealed class CombatResolver : ICombatResolver
    {
        // Public methods

        /// <summary>
        /// Resolves final damage value based on attack intensity.
        /// Attacker can be null for systems that do not provide attacker stats yet.
        /// </summary>
        public int ResolveDamage(ICombatAttacker attacker, ICombatTarget target, int attackIntensity)
        {
            if (target == null || !target.IsAlive)
                return 0;

            if (attackIntensity < 0)
                attackIntensity = 0;

            int baseDamage = 10;
            int intensityDamage = attackIntensity * 5;

            return baseDamage + intensityDamage;
        }
    }
}