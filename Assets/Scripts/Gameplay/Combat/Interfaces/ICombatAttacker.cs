using MightyHeroes.Gameplay.Combat.Modifiers;

namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Represents an entity capable of initiating combat actions.
    /// </summary>
    public interface ICombatAttacker : ICombatEntity
    {
        /// <summary>
        /// Runtime combat modifiers applied to this attacker's outgoing damage.
        /// </summary>
        CombatModifiers Modifiers { get; }
    }
}
