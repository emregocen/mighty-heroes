using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     Extension class of the game grid, containing some helper methods
    /// </summary>
    public static class GameGridExtensions
    {
        /// <summary>
        ///     Gets the element of the grid in the given column and row
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static GameGridElement GetElement(this GameGrid grid, int column, int row)
        {
            return grid.Elements[row * grid.ColumnCount + column];
        }

        /// <summary>
        ///     Gets the world position of the given position in the grid
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="cellSpacing"></param>
        /// <param name="cellSize"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Vector2 GridToWorldPosition(this GameGrid grid, float cellSpacing, float cellSize, int column,
            int row)
        {
            /*
            float x = grid.StartCellPosition.x + column * (cellSize + cellSpacing);
            float y = grid.StartCellPosition.y + row    * (cellSize + cellSpacing);

            return new Vector2(x, y);
            */

            //return grid.StartCellPosition + grid.CellSize * new Vector2(column, row);

            /*
            float step = cellSize + cellSpacing;
            return grid.StartCellPosition + new Vector2(column * step, row * step);
            */

            var step = cellSize + cellSpacing;
            var start = grid.StartCellPosition;
            return new Vector3(
                start.x + column * step,
                start.y + row * step,
                0f
            );
        }

        /// <summary>
        ///     Gets the world position of the given position in the grid
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Vector2 GridToWorldPosition(this GameGrid grid, int column, int row)
        {
            return grid.GridToWorldPosition(grid.CellSpacing, grid.CellSize, column, row);
            //return grid.StartCellPosition + grid.CellSize * new Vector2(column, row);
        }

        /// <summary>
        ///     Gets the world position of the given position in the grid
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public static Vector2 GridToWorldPosition(this GameGrid grid, Vector2Int gridPos)
        {
            return grid.GridToWorldPosition(gridPos.x, gridPos.y);
        }

        /// <summary>
        ///     Gets the index of the element at the given grid position
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public static int GridPositionToIndex(this GameGrid grid, Vector2Int gridPos)
        {
            return grid.ColumnCount * gridPos.y + gridPos.x;
        }
    }
}