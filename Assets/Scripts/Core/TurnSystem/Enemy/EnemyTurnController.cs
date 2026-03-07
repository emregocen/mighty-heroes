using System.Collections.Generic;
using MightyHeroes.Gameplay.Enemy.StateMachine;

namespace MightyHeroes.Core.TurnSystem.Enemy
{
    /// <summary>
    /// Controls sequential execution of enemies during enemy turn.
    /// Ensures only ONE enemy acts at a time.
    /// </summary>
    public sealed class EnemyTurnController
    {
        private readonly List<EnemyStateMachine> _enemies;
        private int _currentIndex;

        public EnemyTurnController(List<EnemyStateMachine> enemies)
        {
            _enemies = enemies;
            _currentIndex = 0;
        }

        /// <summary>
        /// Resets turn order for a new enemy turn.
        /// </summary>
        public void Reset()
        {
            _currentIndex = 0;
        }

        /// <summary>
        /// Returns next enemy that should act, or null if finished.
        /// </summary>
        public EnemyStateMachine GetNextEnemy()
        {
            if (_currentIndex >= _enemies.Count)
                return null;

            return _enemies[_currentIndex++];
        }
    }
}