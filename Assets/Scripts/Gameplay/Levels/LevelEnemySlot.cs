using System;
using MightyHeroes.Gameplay.Enemy.Data;
using UnityEngine;

namespace MightyHeroes.Gameplay.Levels
{
    /// <summary>
    /// One enemy placement in a level: which enemy, in which formation row and slot.
    /// Row 0 is the front row. Slot 0 is the leftmost slot of the row as seen on screen.
    /// </summary>
    [Serializable]
    public struct LevelEnemySlot
    {
        // FIELDS

        [Tooltip("Enemy type to spawn.")]
        [SerializeField] private EnemyData _enemy;

        [Tooltip("Formation row. 0 = front row.")]
        [SerializeField, Range(0, 2)] private int _rowIndex;

        [Tooltip("Slot within the row. 0 = leftmost on screen.")]
        [SerializeField, Range(0, 4)] private int _slotIndex;

        // PROPERTIES

        /// <summary>Enemy type to spawn.</summary>
        public EnemyData Enemy => _enemy;

        /// <summary>Formation row. 0 = front row.</summary>
        public int RowIndex => _rowIndex;

        /// <summary>Slot within the row. 0 = leftmost on screen.</summary>
        public int SlotIndex => _slotIndex;
    }
}
