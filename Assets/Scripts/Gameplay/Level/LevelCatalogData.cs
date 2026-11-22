using System.Collections.Generic;
using UnityEngine;

namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Ordered list of the playable levels. The index in this list is the
    /// level index used by LevelSession and LevelProgressStore.
    /// Later chapters can be grouped into separate catalogs.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalogData_Main", menuName = "Mighty Heroes/Content/Level Catalog Data")]
    public sealed class LevelCatalogData : ScriptableObject
    {
        // FIELDS

        [SerializeField] private List<LevelData> _levels = new List<LevelData>();

        // PROPERTIES

        /// <summary>Levels in play order.</summary>
        public IReadOnlyList<LevelData> Levels => _levels;

        /// <summary>Number of levels in the catalog.</summary>
        public int Count => _levels.Count;
    }
}
