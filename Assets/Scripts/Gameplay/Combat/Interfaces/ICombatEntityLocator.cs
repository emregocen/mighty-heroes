using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Resolves a combat entity's current world position by runtime id.
    /// Lets presentation systems (hit VFX, feedback) place effects on entities
    /// without depending on the battle scene setup.
    /// </summary>
    public interface ICombatEntityLocator
    {
        /// <summary>
        /// Returns the root world position of the entity with the given id.
        /// </summary>
        /// <param name="entityId">Runtime entity id.</param>
        /// <param name="position">Entity root world position when found.</param>
        /// <returns>True when the entity exists in the current battle.</returns>
        bool TryGetPosition(int entityId, out Vector3 position);
    }
}
