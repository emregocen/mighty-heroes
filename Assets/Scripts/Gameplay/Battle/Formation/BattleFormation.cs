using System;
using System.Collections.Generic;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Enemy;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Manages enemy formation layout for a single battle session.
    ///
    /// Responsibilities:
    /// - Owns the line and slot structure of the enemy formation.
    /// - Enforces front-line access rule: only front-line enemies are targetable.
    /// - Maintains the currently selected attack target.
    /// - Handles automatic target reselection when the selected enemy dies:
    ///   prefers the slot to the right, falls back to the left.
    /// - Advances the effective front line when a line is fully cleared.
    ///
    /// Implements IBattleField for CombatSystem (target resolution)
    /// and IFormation for TurnSystem (line-ordered sequencing).
    /// </summary>
    public sealed class BattleFormation : IBattleField, IFormation
    {
        // FIELDS

        /// <summary>
        /// Raised whenever the current battle target changes.
        /// </summary>
        public event Action<Transform> CurrentTargetTransformChanged;
        private readonly List<BattleLine> _lines = new List<BattleLine>();
        private EnemyComponent _currentTarget;

        // PROPERTIES

        /// <summary>
        /// All battle lines ordered front to back (index 0 = front row).
        /// </summary>
        public IReadOnlyList<BattleLine> Lines => _lines;

        /// <summary>
        /// The front-most line that still contains at least one alive enemy.
        /// Returns null only when all enemies in the formation are defeated.
        /// </summary>
        public BattleLine FrontLine
        {
            get
            {
                foreach (BattleLine line in _lines)
                {
                    if (line.HasAliveEnemy)
                        return line;
                }

                return null;
            }
        }

        /// <summary>
        /// The currently selected attack target.
        /// Always belongs to the current front line.
        /// Null only when all enemies are defeated.
        /// </summary>
        public EnemyComponent CurrentTarget => _currentTarget;

        /// <summary>
        /// The transform used by presentation systems as the current target focus point.
        /// </summary>
        public Transform CurrentTargetFocus =>
            _currentTarget != null
                ? _currentTarget.TargetFocusPoint
                : null;

        /// <summary>
        /// Raised when the current target focus transform changes.
        /// </summary>
        public event Action<Transform> CurrentTargetFocusChanged;

        // PUBLIC METHODS — Initialization

        /// <summary>
        /// Registers enemies into the formation and selects the default target.
        /// Enemies are sorted by LineIndex then SlotIndex so formation layout
        /// is independent of Inspector array ordering.
        /// Must be called by BattleBootstrapper after all enemy components are initialized.
        /// </summary>
        /// <param name="entries">Formation entries produced during enemy initialization.</param>
        public void Initialize(IReadOnlyList<EnemyFormationEntry> entries)
        {
            _lines.Clear();
            _currentTarget = null;

            int maxLineIndex = 0;
            foreach (EnemyFormationEntry entry in entries)
            {
                if (entry.LineIndex > maxLineIndex)
                    maxLineIndex = entry.LineIndex;
            }

            for (int i = 0; i <= maxLineIndex; i++)
                _lines.Add(new BattleLine(i));

            // Sort by LineIndex then SlotIndex — explicit positioning,
            // Inspector array order does not affect formation layout.
            List<EnemyFormationEntry> sorted = new List<EnemyFormationEntry>(entries);
            sorted.Sort((a, b) =>
            {
                int lineCompare = a.LineIndex.CompareTo(b.LineIndex);
                return lineCompare != 0 ? lineCompare : a.SlotIndex.CompareTo(b.SlotIndex);
            });

            foreach (EnemyFormationEntry entry in sorted)
            {
                if (entry.Enemy != null)
                    _lines[entry.LineIndex].AddEnemy(entry.Enemy);
            }

            SelectDefaultTarget();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleFormation initialized. Lines={_lines.Count} Target={_currentTarget?.EntityId}");
        }

        // PUBLIC METHODS — Target Selection

        /// <summary>
        /// Selects the enemy to the right of the current target within the front line.
        /// Wraps around to the leftmost alive enemy if no alive enemy exists to the right.
        /// Call this from the UI right-arrow button handler.
        /// </summary>
        public void SelectNextTarget()
        {
            BattleLine front = FrontLine;
            if (front == null || _currentTarget == null)
                return;

            EnemyComponent next = front.GetNextAliveEnemy(_currentTarget);
            if (next != null)
                SetTarget(next);
        }

        /// <summary>
        /// Selects the enemy to the left of the current target within the front line.
        /// Wraps around to the rightmost alive enemy if no alive enemy exists to the left.
        /// Call this from the UI left-arrow button handler.
        /// </summary>
        public void SelectPreviousTarget()
        {
            BattleLine front = FrontLine;
            if (front == null || _currentTarget == null)
                return;

            EnemyComponent previous = front.GetPreviousAliveEnemy(_currentTarget);
            if (previous != null)
                SetTarget(previous);
        }

        /// <summary>
        /// Explicitly selects a specific enemy as the attack target.
        /// Only succeeds if the enemy belongs to the current front line and is alive.
        /// Returns true if selection succeeded.
        /// </summary>
        /// <param name="enemy">Enemy to select.</param>
        public bool TrySelectTarget(EnemyComponent enemy)
        {
            BattleLine front = FrontLine;
            if (front == null || enemy == null || !enemy.IsAlive || !front.Contains(enemy))
                return false;

            SetTarget(enemy);
            return true;
        }

        // PUBLIC METHODS — Death Notification

        /// <summary>
        /// Notifies the formation that an enemy has been defeated.
        ///
        /// If the defeated enemy was the current target, a new target is
        /// automatically selected: prefers the slot to the right,
        /// falls back to the left within the same line.
        /// If the line is fully cleared, SelectDefaultTarget picks
        /// the first alive enemy in the new front line.
        ///
        /// Must be called by BattleBootstrapper.OnEntityDied
        /// before updating TurnContext alive count.
        /// </summary>
        /// <param name="enemyId">Runtime entity id of the defeated enemy.</param>
        public void NotifyEnemyDefeated(int enemyId)
        {
            bool wasTarget = _currentTarget != null && _currentTarget.EntityId == enemyId;
            int defeatedSlotIndex = -1;
            BattleLine defeatedLine = null;

            foreach (BattleLine line in _lines)
            {
                if (!line.ContainsId(enemyId))
                    continue;

                for (int i = 0; i < line.Enemies.Count; i++)
                {
                    if (line.Enemies[i] != null && line.Enemies[i].EntityId == enemyId)
                    {
                        defeatedSlotIndex = i;
                        break;
                    }
                }

                defeatedLine = line;
                line.RemoveEnemy(enemyId);
                break;
            }

            if (!wasTarget)
                return;

            // Reselect within the same line first (right then left).
            if (defeatedLine != null && defeatedLine.HasAliveEnemy)
            {
                EnemyComponent nearest = defeatedLine.GetNearestAliveEnemy(defeatedSlotIndex);
                if (nearest != null)
                {
                    SetTarget(nearest);
                    return;
                }
            }

            // Line cleared — fall back to new front line default.
            SelectDefaultTarget();
        }

        // IBattleField IMPLEMENTATION

        /// <summary>
        /// Returns the current attack target. Used by CombatSystem to apply hero attack damage.
        /// </summary>
        public ICombatTarget GetCurrentTarget()
        {
            return _currentTarget;
        }

        /// <summary>
        /// Finds an enemy combat entity by runtime id across all lines.
        /// Used by CombatSystem to route enemy attack hit events.
        /// </summary>
        public ICombatEntity FindEnemyById(int enemyId)
        {
            foreach (BattleLine line in _lines)
            {
                ICombatEntity found = line.FindById(enemyId);
                if (found != null)
                    return found;
            }

            return null;
        }

        // PRIVATE METHODS

        /// <summary>
        /// Sets the current active enemy target and notifies presentation listeners.
        /// </summary>
        /// <param name="enemy">Enemy that becomes the active target.</param>
        private void SetTarget(EnemyComponent enemy)
        {
            _currentTarget = enemy;
            
            CurrentTargetFocusChanged?.Invoke(CurrentTargetFocus);
        }

        private void SelectDefaultTarget()
        {
            SetTarget(FrontLine?.GetFirstAliveEnemy());
        }
    }
}