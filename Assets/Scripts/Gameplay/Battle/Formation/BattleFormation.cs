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
        // CONSTANTS

        private const int ScreenDirectionRight = 1;
        private const int ScreenDirectionLeft = -1;

        // FIELDS

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
        /// Selects the alive front-line enemy that is next to the right of the
        /// current target on screen. Wraps around to the leftmost one.
        /// Used by the E key and the right arrow button.
        /// </summary>
        public void SelectNextTarget()
        {
            SelectTargetOnScreen(ScreenDirectionRight);
        }

        /// <summary>
        /// Selects the alive front-line enemy that is next to the left of the
        /// current target on screen. Wraps around to the rightmost one.
        /// Used by the Q key and the left arrow button.
        /// </summary>
        public void SelectPreviousTarget()
        {
            SelectTargetOnScreen(ScreenDirectionLeft);
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
        /// the middle alive enemy in the new front line.
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

        /// <summary>
        /// Adds every alive enemy of the current front line to results (cleared first).
        /// </summary>
        public void CollectFrontLineTargets(List<ICombatTarget> results)
        {
            results.Clear();

            BattleLine frontLine = FrontLine;

            if (frontLine == null)
                return;

            foreach (EnemyComponent enemy in frontLine.Enemies)
            {
                if (enemy != null && enemy.IsAlive)
                    results.Add(enemy);
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Moves the target to the closest alive front-line enemy in the given
        /// screen direction. Left and right come from the enemies' screen
        /// positions, so they always match what the player sees, whatever the
        /// slot indices in the formation data are. When nothing lies in that
        /// direction, wraps to the farthest enemy on the other side.
        /// Falls back to slot order when no main camera is available.
        /// </summary>
        /// <param name="direction">ScreenDirectionRight or ScreenDirectionLeft.</param>
        private void SelectTargetOnScreen(int direction)
        {
            BattleLine front = FrontLine;

            if (front == null || _currentTarget == null)
                return;

            Camera camera = Camera.main;

            if (camera == null)
            {
                EnemyComponent fallback = direction > 0 ? front.GetNextAliveEnemy(_currentTarget) : front.GetPreviousAliveEnemy(_currentTarget);

                if (fallback != null)
                    SetTarget(fallback);

                return;
            }

            float currentX = camera.WorldToScreenPoint(_currentTarget.transform.position).x;

            EnemyComponent closest = null;
            float closestDistance = float.MaxValue;
            EnemyComponent wrapTarget = null;
            float wrapDistance = 0f;

            foreach (EnemyComponent enemy in front.Enemies)
            {
                if (enemy == null || !enemy.IsAlive || enemy == _currentTarget)
                    continue;

                float offset = (camera.WorldToScreenPoint(enemy.transform.position).x - currentX) * direction;

                if (offset > 0f && offset < closestDistance)
                {
                    closest = enemy;
                    closestDistance = offset;
                }
                else if (offset <= 0f && -offset >= wrapDistance)
                {
                    wrapTarget = enemy;
                    wrapDistance = -offset;
                }
            }

            EnemyComponent next = closest != null ? closest : wrapTarget;

            if (next != null)
                SetTarget(next);
        }

        /// <summary>
        /// Sets the current active enemy target and notifies presentation listeners.
        /// </summary>
        /// <param name="enemy">Enemy that becomes the active target.</param>
        private void SetTarget(EnemyComponent enemy)
        {
            _currentTarget = enemy;
            
            CurrentTargetFocusChanged?.Invoke(CurrentTargetFocus);
        }

        /// <summary>
        /// Selects the alive front-line enemy closest to the middle of the row
        /// (the one straight ahead of the hero in a centered formation), so the
        /// hero does not start the battle turned toward a side enemy.
        /// </summary>
        private void SelectDefaultTarget()
        {
            BattleLine front = FrontLine;

            if (front == null)
            {
                SetTarget(null);
                return;
            }

            float middle = (front.Enemies.Count - 1) * 0.5f;
            EnemyComponent best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < front.Enemies.Count; i++)
            {
                EnemyComponent enemy = front.Enemies[i];

                if (enemy == null || !enemy.IsAlive)
                    continue;

                float distance = Mathf.Abs(i - middle);

                if (distance < bestDistance)
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }

            SetTarget(best);
        }
    }
}