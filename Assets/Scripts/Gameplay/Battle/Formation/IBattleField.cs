using System.Collections.Generic;
using MightyHeroes.Gameplay.Combat.Interfaces;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Read-only battlefield interface exposed to CombatSystem.
    /// Provides target resolution without exposing formation layout details.
    ///
    /// CombatSystem uses this to find the current attack target and
    /// to route enemy attack events to the correct attacker entity.
    /// </summary>
    public interface IBattleField
    {
        /// <summary>
        /// Returns the currently selected attack target.
        /// Always belongs to the front line. Null only when all enemies are defeated.
        /// </summary>
        ICombatTarget GetCurrentTarget();

        /// <summary>
        /// Finds an enemy combat entity by its runtime id.
        /// Used by CombatSystem to route incoming enemy attack hit events.
        /// </summary>
        ICombatEntity FindEnemyById(int enemyId);

        /// <summary>
        /// Adds every alive enemy of the current front line to results (the list is
        /// cleared first). Used by CombatSystem for multi-target hits such as the
        /// ultimate. Callers must copy before applying damage, because a killed
        /// enemy is removed from its line immediately.
        /// </summary>
        void CollectFrontLineTargets(List<ICombatTarget> results);
    }
}