using System.Collections.Generic;
using MightyHeroes.Gameplay.Common.Actions;
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

        [Tooltip("Grayscale tile body sprites. One is picked at random for every spawned tile. Empty keeps the prefab sprite.")]
        [SerializeField] private List<Sprite> _bodyVariants = new();

        [Tooltip("When a level sets fewer columns than the scene grid, tiles grow to keep the board width, up to this multiple of the scene cell size.")]
        [SerializeField, Min(1f)] private float _maxCellScale = 1.25f;

        private readonly List<GameGridElementType> _activeElementTypes = new();

        private ActionType _highlightedTileType = ActionType.None;

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
        public List<GameGridElementType> ElementTypes => _activeElementTypes.Count > 0 ? _activeElementTypes : elementTypes;

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
        /// Applies a level's board settings. Must be called before SetUpGrid.
        ///
        /// Rows and columns replace the scene values. The scene grid's width is
        /// the reference: cell size and spacing are scaled so the new column
        /// count fills the same width, capped at MaxCellScale so a narrow board
        /// does not get oversized tiles. Tile types limit which element types
        /// spawn; colors and icons still come from the element type list.
        /// Types without an element type entry are ignored; if none match,
        /// all element types stay active.
        /// </summary>
        /// <param name="rows">Board row count.</param>
        /// <param name="columns">Board column count.</param>
        /// <param name="tileTypes">Tile types that can spawn, or null for all.</param>
        public void ConfigureBoard(int rows, int columns, IReadOnlyList<ActionType> tileTypes)
        {
            float step = cellSize + cellSpacing;
            float referenceWidth = (columnCount - 1) * step + cellSize;

            rowCount = Mathf.Max(1, rows);
            columnCount = Mathf.Max(1, columns);

            float newWidth = (columnCount - 1) * step + cellSize;
            float scale = Mathf.Min(referenceWidth / newWidth, _maxCellScale);

            cellSize *= scale;
            cellSpacing *= scale;

            _activeElementTypes.Clear();

            if (tileTypes == null)
                return;

            foreach (GameGridElementType elementType in elementTypes)
            {
                foreach (ActionType tileType in tileTypes)
                {
                    if (elementType.TileType == tileType)
                    {
                        _activeElementTypes.Add(elementType);
                        break;
                    }
                }
            }
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
                ApplyRandomAppearance(element);

                element.SetCellSize(cellSize);

                Elements.Add(element);
            }
        }

        /// <summary>
        /// Highlights (pulsing glow) every tile of the given type, including tiles
        /// spawned later. ActionType.None clears the highlight.
        /// </summary>
        /// <param name="tileType">Tile type to highlight, or None.</param>
        public void SetHighlightedTileType(ActionType tileType)
        {
            _highlightedTileType = tileType;

            foreach (GameGridElement element in Elements)
                element.SetHighlighted(tileType != ActionType.None && element.TileType == tileType);
        }

        /// <summary>
        /// Gives the element a random tile type (color, action, icon) and a
        /// random body variant. Used on the initial grid setup and on respawn.
        /// </summary>
        /// <param name="element">Element to set up.</param>
        public void ApplyRandomAppearance(GameGridElement element)
        {
            element.ApplyType(ElementTypes.GetRandom());
            element.SetHighlighted(_highlightedTileType != ActionType.None && element.TileType == _highlightedTileType);

            if (_bodyVariants.Count > 0)
                element.SetBodySprite(_bodyVariants.GetRandom());
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