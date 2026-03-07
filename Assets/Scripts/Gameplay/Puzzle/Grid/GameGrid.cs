using System.Collections.Generic;
using MightyHeroes.Gameplay.Puzzle.Core;
using MightyHeroes.Gameplay.Puzzle.Extensions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     The grid holding all elements/bubbles
    /// </summary>
    public class GameGrid : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private SelectionLine selectionLine;
        [SerializeField] private GameGridElement gridElementPrefab;

        [SerializeField] private int rowCount = 8;
        [SerializeField] private int columnCount = 8;
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private float cellSpacing = 0.1f;

        [SerializeField] private List<GameGridElementType> elementTypes = new();
        [SerializeField] public List<GameObject> despawnEffects = new();

        private GameGridInput gridInput;
        private GameGridMovement gridMovement;
        private GameGridSpawning gridSpawning;

        /// <summary>
        ///     Gets the list of all elements in the grid
        /// </summary>
        public List<GameGridElement> Elements { get; } = new();

        /// <summary>
        ///     Gets the list of available colors
        /// </summary>
        public List<GameGridElementType> ElementTypes => elementTypes;

        /// <summary>
        ///     Returns the center (worldposition) of the most left cell on the bottom
        /// </summary>
        //public Vector2 StartCellPosition => this.GridCenter - this.CellSize * new Vector2(this.ColumnCount - 1, this.RowCount - 1) / 2.0f;
        /*
        public Vector2 StartCellPosition
        {
            get
            {
                float step = this.cellSize + this.cellSpacing;
                // Gridin toplam genişliği ve yüksekliği:
                float totalWidth  = (this.ColumnCount - 1) * step;
                float totalHeight = (this.RowCount - 1) * step;
                // GridCenter’dan yarıya kaydır:
                return this.GridCenter - new Vector2(totalWidth, totalHeight) / 2.0f;
            }
        }
        */
        public Vector2 StartCellPosition
        {
            get
            {
                var step = cellSize + cellSpacing;

                // tüm gridin genişlik / yükseklik yarısı
                var halfWidth = (ColumnCount - 1) * step / 2f;
                var halfHeight = (RowCount - 1) * step / 2f;

                return GridCenter - new Vector2(halfWidth, halfHeight);
            }
        }

        /// <summary>
        ///     Returns the size of a single cell
        /// </summary>
        public float CellSize => cellSize;

        public float CellSpacing => cellSpacing;

        /// <summary>
        ///     Returns the number of rows (=vertical length)
        /// </summary>
        internal int RowCount => rowCount;

        /// <summary>
        ///     Returns the number of columns (=horizontal length)
        /// </summary>
        internal int ColumnCount => columnCount;

        /// <summary>
        ///     Returns the center poistion of the grid
        /// </summary>
        internal Vector2 GridCenter => transform.position;

        /// <summary>
        ///     Returns the Transform containing all grid elements
        /// </summary>
        internal Transform GridContainer => transform;

        internal Camera PuzzleCamera => camera;

        private void Awake()
        {
            /*
            RowCount = this.rowCount;
            ColumnCount = this.columnCount;
            GridCenter = transform.position;
            GridContainer = transform;
            */

            gridInput = new GameGridInput(this, selectionLine, camera);
            gridMovement = new GameGridMovement(this);
            gridSpawning = new GameGridSpawning(this);
        }

        /// <summary>
        ///     Creates all elements of the grid and sets them up
        /// </summary>
        public void SetUpGrid()
        {
            for (var y = 0; y < RowCount; y++)
            for (var x = 0; x < ColumnCount; x++)
            {
                var element = Instantiate(gridElementPrefab);

                element.transform.localScale = Vector2.one * cellSize;
                element.transform.position = this.GridToWorldPosition(cellSpacing, cellSize, x, y);
                element.transform.SetParent(GridContainer.transform, true);
                element.gameObject.layer = gameObject.layer;

                //TODO: Tipleri ve buradaki sistemi tekrar düşün!
                var randomType = elementTypes.GetRandom();
                element.Color = randomType.Color;
                element.TileType = randomType.TileType;

                element.SetCellSize(cellSize);

                Elements.Add(element);
            }
        }

        /// <summary>
        ///     Returns a coroutine waiting until all grid elements are not moving
        /// </summary>
        /// <returns></returns>
        public Coroutine WaitForMovement()
        {
            return StartCoroutine(gridMovement.WaitForMovement());
        }

        /// <summary>
        ///     Returns a coroutine waiting until the user has made a selection
        /// </summary>
        /// <returns></returns>
        public Coroutine WaitForSelection()
        {
            return StartCoroutine(gridInput.WaitForSelection());
        }

        /// <summary>
        ///     Returns a coroutine respawning all despawned elements and waiting until they are
        ///     finished
        /// </summary>
        /// <returns></returns>
        public Coroutine RespawnElements()
        {
            return StartCoroutine(gridSpawning.RespawnElements());
        }

        /// <summary>
        ///     Returns a coroutine despawning the selected elements and waiting until they are finished
        /// </summary>
        /// <returns></returns>
        public Coroutine DespawnSelection()
        {
            return StartCoroutine(gridSpawning.Despawn(gridInput.SelectedElements));
        }

        public Vector2Int IndexOf(GameGridElement element)
        {
            var index = Elements.IndexOf(element);
            var row = index / ColumnCount;
            var col = index % ColumnCount;
            return new Vector2Int(col, row);
        }
    }
}