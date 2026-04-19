using MightyHeroes.Gameplay.Enemy;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Inspector-assignable configuration for placing a single enemy into the formation.
    /// LineIndex and SlotIndex together define the exact slot — array order is irrelevant.
    /// </summary>
    [System.Serializable]
    public struct EnemyFormationConfig
    {
        /// <summary>
        /// Enemy component reference assigned in Inspector.
        /// </summary>
        public EnemyComponent Enemy;

        /// <summary>
        /// Row index in formation. 0 = front row. Higher values = back rows.
        /// </summary>
        [Range(0, 5)]
        public int RowIndex;

        /// <summary>
        /// Position within the row. 0 = leftmost slot.
        /// </summary>
        [Range(0, 9)]
        public int ColumnIndex;
    }
}