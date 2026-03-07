using System.Collections.Generic;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Read-only formation interface exposed to TurnSystem.
    /// Provides line structure for turn sequencing without exposing
    /// target selection or combat resolution details.
    ///
    /// TurnEnemyState uses this to build the front-to-back ordered
    /// enemy action sequence at the start of each enemy phase.
    ///
    /// Kept separate from IBattleField so CombatSystem remains
    /// unaware of line layout.
    /// </summary>
    public interface IFormation
    {
        /// <summary>
        /// All battle lines ordered front to back.
        /// Index 0 = front row. Higher indices = back rows.
        /// </summary>
        IReadOnlyList<BattleLine> Lines { get; }
    }
}