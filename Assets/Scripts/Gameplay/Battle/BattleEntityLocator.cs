using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// ICombatEntityLocator for the battle scene: the hero by reference and
    /// enemies through BattleFormation's id lookup.
    /// </summary>
    public sealed class BattleEntityLocator : ICombatEntityLocator
    {
        // FIELDS

        private readonly HeroComponent _hero;
        private readonly BattleFormation _formation;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a locator for the given hero and enemy formation.
        /// </summary>
        /// <param name="hero">Battle hero.</param>
        /// <param name="formation">Enemy formation used to find enemies by id.</param>
        public BattleEntityLocator(HeroComponent hero, BattleFormation formation)
        {
            _hero = hero;
            _formation = formation;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Returns the root world position of the hero or a formation enemy.
        /// </summary>
        public bool TryGetPosition(int entityId, out Vector3 position)
        {
            if (_hero != null && entityId == _hero.EntityId)
            {
                position = _hero.Position;
                return true;
            }

            ICombatEntity enemy = _formation != null ? _formation.FindEnemyById(entityId) : null;

            if (enemy != null)
            {
                position = enemy.Position;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
