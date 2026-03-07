using MightyHeroes.Gameplay.Common.Entity;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Companion.Stats;
using UnityEngine;

namespace MightyHeroes.Gameplay.Companion
{
    /// <summary>
    /// MonoBehaviour representing a companion entity.
    /// </summary>
    public sealed class CompanionComponent : EntityComponent
    {
        [Header("Companion Stats")]
        [SerializeField] private CompanionStatsData _companionStatsData;

        /// <summary>
        /// Provides companion-specific stats data.
        /// </summary>
        protected override EntityStatsData GetStatsData()
        {
            return _companionStatsData;
        }

        /// <summary>
        /// Initializes runtime stats and performs any additional setup for the companion.
        /// </summary>
        protected override void OnInitialized()
        {
            // Additional initialization logic for companion can be added here. 
        } 
    }
}