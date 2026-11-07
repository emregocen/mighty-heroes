using System.Collections.Generic;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Enemy;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// ICombatEntityLocator for the battle scene.
    ///
    /// Holds the hero and a snapshot of every enemy taken at battle start, keyed
    /// by entity id. It intentionally does not query BattleFormation, because a
    /// killed enemy is removed from the formation inside Health.ApplyDamage,
    /// before CombatHitResolvedEvent fires; presentation for that killing hit
    /// (hit VFX) still needs the enemy's position. Defeated enemies keep their
    /// GameObject in the scene, so their position stays valid.
    /// </summary>
    public sealed class BattleEntityLocator : ICombatEntityLocator
    {
        // FIELDS

        private readonly HeroComponent _hero;
        private readonly Dictionary<int, EnemyComponent> _enemiesById = new Dictionary<int, EnemyComponent>();

        // CONSTRUCTOR

        /// <summary>
        /// Creates a locator for the hero and the configured formation enemies.
        /// Must be created after enemies are initialized so their ids are assigned.
        /// </summary>
        /// <param name="hero">Battle hero.</param>
        /// <param name="enemyFormation">Formation configuration listing every battle enemy.</param>
        public BattleEntityLocator(HeroComponent hero, IEnumerable<EnemyFormationConfig> enemyFormation)
        {
            _hero = hero;

            if (enemyFormation == null)
                return;

            foreach (EnemyFormationConfig config in enemyFormation)
            {
                if (config.Enemy != null)
                    _enemiesById[config.Enemy.EntityId] = config.Enemy;
            }
        }

        // PUBLIC METHODS

        /// <summary>
        /// Returns the root world position of the hero or any battle enemy,
        /// including enemies that have already been defeated.
        /// </summary>
        public bool TryGetPosition(int entityId, out Vector3 position)
        {
            if (_hero != null && entityId == _hero.EntityId)
            {
                position = _hero.Position;
                return true;
            }

            if (_enemiesById.TryGetValue(entityId, out EnemyComponent enemy) && enemy != null)
            {
                position = enemy.Position;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
