using UnityEngine;

namespace MightyHeroes.Core.TurnSystem
{
    /// <summary>
    /// Holds all runtime data required by the turn state machine.
    /// Acts as the single source of truth for turn-level battle context.
    /// This class is intentionally summary-oriented and does not store entity references.
    /// </summary>
    public sealed class TurnContext
    {
        // Fields

        /// <summary>
        /// Number of currently alive enemies in the battle.
        /// Used only to determine whether the enemy turn phase should run.
        /// </summary>
        private int _aliveEnemyCount;

        // Properties

        /// <summary>
        /// Sequential index of the current turn.
        /// Starts from zero and increments after each full turn cycle.
        /// </summary>
        public int TurnIndex { get; private set; }

        /// <summary>
        /// Indicates whether the current turn is the first turn of the battle.
        /// </summary>
        public bool IsFirstTurn => TurnIndex == 0;

        /// <summary>
        /// Indicates whether the battle has reached a terminal state.
        /// </summary>
        public bool IsBattleEnded { get; private set; }

        /// <summary>
        /// Indicates whether the hero has companions that can act.
        /// </summary>
        public bool HasCompanions { get; private set; }

        /// <summary>
        /// Indicates whether at least one enemy is alive.
        /// </summary>
        public bool HasEnemies => _aliveEnemyCount > 0;

        /// <summary>
        /// Gets the number of currently alive enemies.
        /// </summary>
        public int AliveEnemyCount => _aliveEnemyCount;

        // Public methods

        /// <summary>
        /// Resets the turn context to its initial state.
        /// Called once at battle start.
        /// </summary>
        public void Reset()
        {
            TurnIndex = 0;
            IsBattleEnded = false;
            HasCompanions = false;
            _aliveEnemyCount = 0;
        }

        /// <summary>
        /// Advances the turn index by one.
        /// Called after a full turn cycle completes.
        /// </summary>
        public void AdvanceTurn()
        {
            TurnIndex++;
        }

        /// <summary>
        /// Marks the battle as ended.
        /// </summary>
        public void EndBattle()
        {
            IsBattleEnded = true;
        }

        /// <summary>
        /// Updates whether companions are available to act.
        /// </summary>
        /// <param name="hasCompanions">True if companions exist and can act.</param>
        public void SetHasCompanion(bool hasCompanions)
        {
            HasCompanions = hasCompanions;
        }

        /// <summary>
        /// Registers a newly spawned or initialized enemy.
        /// Called during battle setup.
        /// </summary>
        public void RegisterEnemy()
        {
            _aliveEnemyCount++;
        }

        /// <summary>
        /// Unregisters an enemy that has died or left the battle.
        /// Called exactly once per enemy death.
        /// </summary>
        public void UnregisterEnemy()
        {
            _aliveEnemyCount = Mathf.Max(0, _aliveEnemyCount - 1);
        }
    }
}