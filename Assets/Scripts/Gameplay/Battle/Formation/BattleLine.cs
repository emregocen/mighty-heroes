using System.Collections.Generic;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Enemy;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Represents a single row in the enemy formation.
    ///
    /// Responsibilities:
    /// - Maintains ordered list of enemy slots in this row.
    /// - Reports whether any alive enemy exists in this row.
    /// - Supports directional navigation (left/right) within the row.
    /// - Supports enemy lookup and removal on death.
    ///
    /// Order in the list reflects left-to-right visual position in the scene.
    /// </summary>
    public sealed class BattleLine
    {
        // FIELDS

        private readonly int _lineIndex;
        private readonly List<EnemyComponent> _enemies = new List<EnemyComponent>();

        // PROPERTIES

        /// <summary>Index of this line in the formation. 0 = front row.</summary>
        public int LineIndex => _lineIndex;

        /// <summary>True if at least one alive enemy occupies this line.</summary>
        public bool HasAliveEnemy
        {
            get
            {
                foreach (EnemyComponent enemy in _enemies)
                {
                    if (enemy != null && enemy.IsAlive)
                        return true;
                }

                return false;
            }
        }

        /// <summary>Read-only access to all enemy slots in this line.</summary>
        public IReadOnlyList<EnemyComponent> Enemies => _enemies;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new empty battle line.
        /// </summary>
        /// <param name="lineIndex">Position of this line in the formation. 0 = front.</param>
        public BattleLine(int lineIndex)
        {
            _lineIndex = lineIndex;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Adds an enemy to this line. Preserves insertion order (left to right).
        /// </summary>
        public void AddEnemy(EnemyComponent enemy)
        {
            _enemies.Add(enemy);
        }

        /// <summary>
        /// Returns true if this line contains the given enemy component instance.
        /// </summary>
        public bool Contains(EnemyComponent enemy)
        {
            return _enemies.Contains(enemy);
        }

        /// <summary>
        /// Returns true if this line contains an enemy with the given runtime id.
        /// </summary>
        public bool ContainsId(int enemyId)
        {
            foreach (EnemyComponent enemy in _enemies)
            {
                if (enemy != null && enemy.EntityId == enemyId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Finds an enemy combat entity by runtime id within this line.
        /// Returns null if not found.
        /// </summary>
        public ICombatEntity FindById(int enemyId)
        {
            foreach (EnemyComponent enemy in _enemies)
            {
                if (enemy != null && enemy.EntityId == enemyId)
                    return enemy;
            }

            return null;
        }

        /// <summary>
        /// Returns the first alive enemy in this line (leftmost).
        /// Returns null if no alive enemy exists.
        /// </summary>
        public EnemyComponent GetFirstAliveEnemy()
        {
            foreach (EnemyComponent enemy in _enemies)
            {
                if (enemy != null && enemy.IsAlive)
                    return enemy;
            }

            return null;
        }

        /// <summary>
        /// Returns the alive enemy to the right of the given enemy within this line.
        /// Wraps around to the leftmost alive enemy if no alive enemy exists to the right.
        /// Returns null if no alive enemy exists in this line at all.
        /// </summary>
        /// <param name="current">Currently selected enemy.</param>
        public EnemyComponent GetNextAliveEnemy(EnemyComponent current)
        {
            int currentIndex = _enemies.IndexOf(current);

            // Search right of current
            for (int i = currentIndex + 1; i < _enemies.Count; i++)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            // Wrap: search from leftmost up to (not including) current
            for (int i = 0; i < currentIndex; i++)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            return null;
        }

        /// <summary>
        /// Returns the alive enemy to the left of the given enemy within this line.
        /// Wraps around to the rightmost alive enemy if no alive enemy exists to the left.
        /// Returns null if no alive enemy exists in this line at all.
        /// </summary>
        /// <param name="current">Currently selected enemy.</param>
        public EnemyComponent GetPreviousAliveEnemy(EnemyComponent current)
        {
            int currentIndex = _enemies.IndexOf(current);

            // Search left of current
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            // Wrap: search from rightmost down to (not including) current
            for (int i = _enemies.Count - 1; i > currentIndex; i--)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            return null;
        }

        /// <summary>
        /// Removes an enemy from this line by runtime id.
        /// Returns true if the enemy was found and removed, false otherwise.
        /// </summary>
        public bool RemoveEnemy(int enemyId)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] != null && _enemies[i].EntityId == enemyId)
                {
                    _enemies.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the alive enemy nearest to the given index.
        /// Searches right first, then left. Used for auto-selection after a death.
        /// Returns null if no alive enemy exists in this line.
        /// </summary>
        /// <param name="nearIndex">The index position to search from.</param>
        public EnemyComponent GetNearestAliveEnemy(int nearIndex)
        {
            int count = _enemies.Count;

            // Clamp nearIndex to valid range
            if (nearIndex >= count)
                nearIndex = count - 1;

            // Search right first
            for (int i = nearIndex; i < count; i++)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            // Search left
            for (int i = nearIndex - 1; i >= 0; i--)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive)
                    return _enemies[i];
            }

            return null;
        }
    }
}