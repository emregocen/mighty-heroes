using System;
using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    [Serializable]
    public struct GameGridElementType
    {
        [SerializeField] public Color Color;

        [SerializeField] public ActionType TileType;
    }
}