using System;
using MightyHeroes.Gameplay.Enemy.Data;
using UnityEngine;

namespace MightyHeroes.Gameplay.Levels
{
    /// <summary>
    /// One enemy placement in a level: which enemy, in which formation row and slot.
    /// Row 0 is the front row. Slots only order the enemies of a row left to
    /// right on screen; the actual positions are laid out relative to the hero
    /// and centered by FormationLayoutConfig.
    /// </summary>
    [Serializable]
    public struct LevelEnemySlot
    {
        // FIELDS

        [Tooltip("Enemy type to spawn.")]
        [SerializeField] private EnemyData _enemy;

        [Tooltip("Formation row. 0 = front row.")]
        [SerializeField, Range(0, 2)] private int _rowIndex;

        [Tooltip("Order within the row, left to right on screen. Gaps are ignored; the row is centered.")]
        [SerializeField, Range(0, 4)] private int _slotIndex;

        [Tooltip("Optional fine-tune on top of the computed position. X > 0 moves right on screen, Z > 0 moves toward the hero. Usually zero.")]
        [SerializeField] private Vector3 _positionOffset;

        // PROPERTIES

        /// <summary>Enemy type to spawn.</summary>
        public EnemyData Enemy => _enemy;

        /// <summary>Formation row. 0 = front row.</summary>
        public int RowIndex => _rowIndex;

        /// <summary>Slot within the row. 0 = leftmost on screen.</summary>
        public int SlotIndex => _slotIndex;

        /// <summary>Per-level position offset: X right on screen, Y up, Z toward the hero.</summary>
        public Vector3 PositionOffset => _positionOffset;
    }
}
