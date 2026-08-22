using MightyHeroes.Gameplay.Common.Stats;
using UnityEngine;

namespace MightyHeroes.Gameplay.Companion.Stats
{
    /// <summary>
    /// Stats data specific to a companion entity.
    ///
    /// Defines base combat parameters such as health,
    /// action points, and attack power.
    ///
    /// This data is static and shared across all instances
    /// using the same enemy configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CompanionStatsData",
        menuName = "Mighty Heroes/Gameplay/Stats/Companion Stats")]
    public sealed class CompanionStatsData : EntityStatsData
    {

    }
}