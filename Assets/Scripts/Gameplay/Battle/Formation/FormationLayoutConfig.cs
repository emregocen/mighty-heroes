using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Formation
{
    /// <summary>
    /// Tuning for placing enemies relative to the hero's battle position.
    ///
    /// The hero battle position's forward axis points at the enemies. Row 0
    /// stands FrontRowDistance ahead of the hero, every further row RowSpacing
    /// further. Enemies in a row are centered and spaced SlotSpacing apart, so
    /// a row of two is automatically tighter than a row of three. All enemies
    /// stand at the hero's height and face the hero.
    /// </summary>
    [CreateAssetMenu(fileName = "FormationLayoutConfig_Default", menuName = "Mighty Heroes/Battle/Formation Layout Config")]
    public sealed class FormationLayoutConfig : ScriptableObject
    {
        // FIELDS

        [Tooltip("Distance from the hero to the front row.")]
        [SerializeField, Min(0.5f)] private float _frontRowDistance = 5f;

        [Tooltip("Distance between consecutive rows.")]
        [SerializeField, Min(0.5f)] private float _rowSpacing = 5f;

        [Tooltip("Sideways distance between neighbouring enemies in a row.")]
        [SerializeField, Min(0.5f)] private float _slotSpacing = 3f;

        // PROPERTIES

        /// <summary>Distance from the hero to the front row.</summary>
        public float FrontRowDistance => _frontRowDistance;

        /// <summary>Distance between consecutive rows.</summary>
        public float RowSpacing => _rowSpacing;

        /// <summary>Sideways distance between neighbouring enemies in a row.</summary>
        public float SlotSpacing => _slotSpacing;
    }
}
