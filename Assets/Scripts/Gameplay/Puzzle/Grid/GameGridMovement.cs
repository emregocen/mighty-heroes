using System.Collections;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     Performs the animated movement of elements in the game grid
    /// </summary>
    public class GameGridMovement
    {
        private readonly GameGrid grid;

        public GameGridMovement(GameGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        ///     Moves all elements downwards if there are empty cells below them and
        ///     waits for their movement to be finished
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForMovement()
        {
            MoveElements();

            while (!IsMovementDone()) yield return new WaitForSeconds(0.05f);

            yield break;
        }

        /// <summary>
        ///     Returns true if no element is moving, false otherwise
        /// </summary>
        /// <returns></returns>
        private bool IsMovementDone()
        {
            foreach (var element in grid.Elements)
                if (element.IsSpawned && element.IsMoving)
                    return false;

            return true;
        }

        /// <summary>
        ///     Moves elements downwards to fill gaps between cells
        /// </summary>
        private void MoveElements()
        {
            //Run from bottom to top through all rows
            for (var y = 0; y < grid.RowCount; y++)
            for (var x = 0; x < grid.ColumnCount; x++)
                ProcessCell(x, y);
        }

        /// <summary>
        ///     Checks for the cell in the given row and column, whether it is not spawned.
        ///     If thats the case, the next spawned element above it, will move to its position.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        private void ProcessCell(int column, int row)
        {
            var element = grid.GetElement(column, row);

            //Is the element not spawned? 
            //=> Cell is empty and elements above should move downwards
            if (!element.IsSpawned)
                //Move the next element above it down
                for (var i = row + 1; i < grid.RowCount; i++)
                {
                    var next = grid.GetElement(column, i);

                    if (next != null && next.IsSpawned && !next.IsMoving)
                    {
                        MoveElement(new Vector2Int(column, i), new Vector2Int(column, row));

                        return;
                    }
                }
        }

        private void MoveElement(Vector2Int oldPos, Vector2Int newPos)
        {
            /* düzeltme
            //Catch the elements of the two grid positions
            GameGridElement element1 = this.grid.GetElement(oldPos.x, oldPos.y);
            GameGridElement element2 = this.grid.GetElement(newPos.x, newPos.y);

            //Switch them in the grid
            this.grid.Elements[this.grid.GridPositionToIndex(oldPos)] = element2;
            this.grid.Elements[this.grid.GridPositionToIndex(newPos)] = element1;

            //Update world positions
            element2.transform.position = this.grid.GridToWorldPosition(oldPos);
            element1.Move(this.grid.GridToWorldPosition(newPos), 0.4f);
            */
            var elementFrom = grid.GetElement(oldPos.x, oldPos.y); // üstteki, taşınacak
            var elementTo = grid.GetElement(newPos.x, newPos.y); // alttaki, boş

            // Switch in list
            grid.Elements[grid.GridPositionToIndex(oldPos)] = elementTo;
            grid.Elements[grid.GridPositionToIndex(newPos)] = elementFrom;

            // Update world positions:
            if (elementTo != null)
                elementTo.transform.position = grid.GridToWorldPosition(oldPos);
            if (elementFrom != null)
                elementFrom.Move(grid.GridToWorldPosition(newPos), 0.4f);
        }
    }
}