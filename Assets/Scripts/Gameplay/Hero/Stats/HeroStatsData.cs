using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Ultimate;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Stats
{
    /// <summary>
    /// Stats data specific to the player-controlled hero.
    /// Adds the hero's ultimate rules on top of the shared entity stats.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HeroStatsData",
        menuName = "Mighty Heroes/Gameplay/Stats/Hero Stats")]
    public sealed class HeroStatsData : EntityStatsData
    {
        // FIELDS

        [Header("Hero Stats")]
        [Tooltip("Ultimate rules of this hero (affinity, gauge, AP cost, targeting). Leave empty for no ultimate.")]
        [SerializeField] private UltimateData _ultimateData;

        // PROPERTIES

        /// <summary>
        /// Ultimate rules of this hero, or null when the hero has no ultimate.
        /// </summary>
        public UltimateData UltimateData => _ultimateData;
    }
}
