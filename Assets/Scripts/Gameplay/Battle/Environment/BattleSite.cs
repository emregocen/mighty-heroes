using System;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Environment
{
    /// <summary>
    /// One place in an environment scene where a battle can be fought, for
    /// example the dungeon entrance or the great hall. A level picks a site by
    /// its SiteId.
    ///
    /// Holds the hero points (intro start and battle position) and optionally
    /// the environment sections that must stay active for this site (others can
    /// be disabled for performance). Enemies are not placed here: the battle
    /// setup lays them out relative to the hero battle position with a
    /// FormationLayoutConfig, so the battle position's forward (blue) axis must
    /// face where the enemies should stand, on flat ground.
    /// </summary>
    public sealed class BattleSite : MonoBehaviour
    {
        // FIELDS

        [Tooltip("Id used by LevelData to pick this site, e.g. 'Entrance'.")]
        [SerializeField] private string _siteId = "Site";

        [Header("Hero")]
        [Tooltip("Where the hero starts the intro walk.")]
        [SerializeField] private Transform _heroIntroStartPoint;

        [Tooltip("Where the hero fights. Its forward axis points at the enemies; enemies are laid out from here.")]
        [SerializeField] private Transform _heroBattlePosition;

        [Header("Sections (optional)")]
        [Tooltip("Environment sections that must be active for this site. When empty, sections are not changed.")]
        [SerializeField] private GameObject[] _activeSections = Array.Empty<GameObject>();

        // PROPERTIES

        /// <summary>Id used by LevelData to pick this site.</summary>
        public string SiteId => _siteId;

        /// <summary>Where the hero starts the intro walk.</summary>
        public Transform HeroIntroStartPoint => _heroIntroStartPoint;

        /// <summary>Where the hero fights; its forward axis points at the enemies.</summary>
        public Transform HeroBattlePosition => _heroBattlePosition;

        /// <summary>Sections that must be active for this site; empty means no change.</summary>
        public GameObject[] ActiveSections => _activeSections;
    }
}
