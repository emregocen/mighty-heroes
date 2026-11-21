using System;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Environment
{
    /// <summary>
    /// One place in an environment scene where a battle can be fought, for
    /// example the dungeon entrance or the great hall. A level picks a site by
    /// its SiteId.
    ///
    /// Holds the hero points (intro start and battle position), the enemy
    /// anchors per formation row and slot, and optionally the environment
    /// sections that must stay active for this site (others can be disabled
    /// for performance).
    ///
    /// Enemy rows: row 0 is the front row; slots in a row are ordered left to
    /// right as seen from the battle camera, matching LevelEnemySlot.SlotIndex.
    /// The hero battle position's forward (blue) axis should face the enemies.
    /// </summary>
    public sealed class BattleSite : MonoBehaviour
    {
        // FIELDS

        [Tooltip("Id used by LevelData to pick this site, e.g. 'Entrance'.")]
        [SerializeField] private string _siteId = "Site";

        [Header("Hero")]
        [Tooltip("Where the hero starts the intro walk.")]
        [SerializeField] private Transform _heroIntroStartPoint;

        [Tooltip("Where the hero fights. Its forward axis should face the enemies.")]
        [SerializeField] private Transform _heroBattlePosition;

        [Header("Enemies")]
        [Tooltip("Rows front to back. Each row lists its slot anchors left to right on screen.")]
        [SerializeField] private EnemyRow[] _enemyRows = Array.Empty<EnemyRow>();

        [Header("Sections (optional)")]
        [Tooltip("Environment sections that must be active for this site. When empty, sections are not changed.")]
        [SerializeField] private GameObject[] _activeSections = Array.Empty<GameObject>();

        // PROPERTIES

        /// <summary>Id used by LevelData to pick this site.</summary>
        public string SiteId => _siteId;

        /// <summary>Where the hero starts the intro walk.</summary>
        public Transform HeroIntroStartPoint => _heroIntroStartPoint;

        /// <summary>Where the hero fights.</summary>
        public Transform HeroBattlePosition => _heroBattlePosition;

        /// <summary>Sections that must be active for this site; empty means no change.</summary>
        public GameObject[] ActiveSections => _activeSections;

        // PUBLIC METHODS

        /// <summary>
        /// Returns the enemy anchor for the given row and slot, or false when it does not exist.
        /// </summary>
        /// <param name="rowIndex">Formation row. 0 = front row.</param>
        /// <param name="slotIndex">Slot within the row. 0 = leftmost on screen.</param>
        /// <param name="anchor">The anchor transform when found.</param>
        public bool TryGetEnemyAnchor(int rowIndex, int slotIndex, out Transform anchor)
        {
            anchor = null;

            if (rowIndex < 0 || rowIndex >= _enemyRows.Length)
                return false;

            Transform[] slots = _enemyRows[rowIndex].Slots;

            if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
                return false;

            anchor = slots[slotIndex];
            return anchor != null;
        }

        // NESTED TYPES

        /// <summary>Slot anchors of one formation row, left to right on screen.</summary>
        [Serializable]
        private struct EnemyRow
        {
            [Tooltip("Slot anchors left to right on screen.")]
            public Transform[] Slots;
        }
    }
}
