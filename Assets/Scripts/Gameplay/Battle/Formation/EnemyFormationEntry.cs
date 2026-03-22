using MightyHeroes.Gameplay.Enemy;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Immutable data bag used during BattleBootstrapper initialization
    /// to place an enemy component into a specific formation slot.
    ///
    /// LineIndex defines the row (0 = front). SlotIndex defines the left-to-right
    /// position within that row (0 = leftmost). Both values are explicit so
    /// formation layout is independent of Inspector array ordering.
    ///
    /// Created after enemy components are initialized so EntityId is already assigned.
    /// </summary>
    public readonly struct EnemyFormationEntry
    {
        /// <summary>
        /// Initialized enemy component to place in the formation.
        /// </summary>
        public readonly EnemyComponent Enemy;

        /// <summary>
        /// Row index in formation. 0 = front row. Higher values = back rows.
        /// </summary>
        public readonly int LineIndex;

        /// <summary>
        /// Position within the row. 0 = leftmost slot.
        /// </summary>
        public readonly int SlotIndex;

        /// <summary>
        /// Creates a new formation entry.
        /// </summary>
        /// <param name="enemy">Initialized enemy component.</param>
        /// <param name="lineIndex">Row index. 0 = front row.</param>
        /// <param name="slotIndex">Position within the row. 0 = leftmost.</param>
        public EnemyFormationEntry(EnemyComponent enemy, int lineIndex, int slotIndex)
        {
            Enemy = enemy;
            LineIndex = lineIndex;
            SlotIndex = slotIndex;
        }
    }
}