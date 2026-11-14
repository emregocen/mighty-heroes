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

        /// <summary>
        /// Draws a drop shadow under the icon, like a UI Shadow component.
        /// Shadow color and distance are constants in GameGridElement.
        /// </summary>
        [Tooltip("Draws a drop shadow under the icon. Color and distance are fixed in GameGridElement.")]
        [SerializeField] public bool ShowIconShadow;
    }
}
