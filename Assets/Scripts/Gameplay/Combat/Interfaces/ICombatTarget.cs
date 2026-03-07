using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Represents an entity that can receive combat damage.
    /// </summary>
    public interface ICombatTarget : ICombatEntity
    {
        /// <summary>
        /// Current health value of the target.
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Applies damage to the target.
        /// </summary>
        void ReceiveDamage(int damage, HitDirection hitDirection);
    }
}