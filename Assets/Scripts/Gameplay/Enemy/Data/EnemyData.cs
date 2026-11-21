using MightyHeroes.Gameplay.Enemy.Stats;
using UnityEngine;

namespace MightyHeroes.Gameplay.Enemy.Data
{
    /// <summary>
    /// Content definition of one enemy type: the prefab that is spawned into a
    /// formation slot and the stats it fights with.
    ///
    /// The same prefab can be used by several EnemyData assets with different
    /// stats (for example a regular and a boss variant). Data only; spawning is
    /// done by the battle setup.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData_New", menuName = "Mighty Heroes/Content/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        // FIELDS

        [Tooltip("Name shown in UI and logs.")]
        [SerializeField] private string _displayName = "Enemy";

        [Tooltip("Enemy prefab with an EnemyComponent on its root.")]
        [SerializeField] private EnemyComponent _prefab;

        [Tooltip("Stats applied to the spawned enemy. Empty keeps the stats assigned on the prefab.")]
        [SerializeField] private EnemyStatsData _stats;

        // PROPERTIES

        /// <summary>Name shown in UI and logs.</summary>
        public string DisplayName => _displayName;

        /// <summary>Enemy prefab with an EnemyComponent on its root.</summary>
        public EnemyComponent Prefab => _prefab;

        /// <summary>Stats for the spawned enemy, or null to keep the prefab's stats.</summary>
        public EnemyStatsData Stats => _stats;
    }
}
