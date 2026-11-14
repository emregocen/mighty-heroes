using System;
using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    /// Inspector definition of one tile type: its tint color, the action it
    /// triggers and the icon drawn on top of the tile body.
    /// Matching is done by Color only; the icon fields are visual.
    /// </summary>
    [Serializable]
    public struct GameGridElementType
    {
        /// <summary>Tint applied to the grayscale tile body. Also the match key.</summary>
        [SerializeField] public Color Color;

        /// <summary>Hero action triggered when tiles of this type are matched.</summary>
        [SerializeField] public ActionType TileType;

        /// <summary>Icon sprite drawn in the middle of the tile. Leave empty for no icon.</summary>
        [Tooltip("Icon drawn in the middle of the tile. Leave empty for no icon.")]
        [SerializeField] public Sprite Icon;

        /// <summary>Tint of the icon. Alpha must be above zero for the icon to be visible.</summary>
        [Tooltip("Icon tint. Alpha must be above zero.")]
        [SerializeField] public Color IconColor;

        /// <summary>Optional shadow / outline sprite drawn under the icon (for example a Synty _Underlay sprite).</summary>
        [Tooltip("Optional shadow sprite drawn under the icon, e.g. the matching Synty _Underlay sprite. Leave empty for no shadow.")]
        [SerializeField] public Sprite IconShadow;

        /// <summary>Tint of the icon shadow.</summary>
        [Tooltip("Icon shadow tint. Alpha must be above zero.")]
        [SerializeField] public Color IconShadowColor;
    }
}
