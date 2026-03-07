using MightyHeroes.Gameplay.Common.Stats;
using UnityEngine;

namespace MightyHeroes.Gameplay.Enemy.Stats
{
    /// <summary>
    /// Stats data specific to an enemy entity.
    ///
    /// Defines base combat parameters such as health,
    /// action points, and attack power.
    ///
    /// This data is static and shared across all instances
    /// using the same enemy configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyStatsData",
        menuName = "Gameplay/Stats/Enemy Stats")]
    public sealed class EnemyStatsData : EntityStatsData
    {

    }
}