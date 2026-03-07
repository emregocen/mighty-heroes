using MightyHeroes.Gameplay.Common.Stats;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Stats
{
    /// <summary>
    /// Stats data specific to the player-controlled hero.
    /// Defines puzzle-driven behavior and combo thresholds.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HeroStatsData",
        menuName = "Gameplay/Stats/Hero Stats")]
    public sealed class HeroStatsData : EntityStatsData
    {
        // 1) FIELDS

        [Header("Hero Stats")]
        [SerializeField] private int _comboThresholdForUltimate;

        // 2) PROPERTIES

        /// <summary>
        /// Required combo count to unlock the ultimate ability.
        /// </summary>
        public int ComboThresholdForUltimate => _comboThresholdForUltimate;
    }
}