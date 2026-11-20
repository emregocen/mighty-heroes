using System.Collections;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Puzzle.Core;
using MightyHeroes.Gameplay.Puzzle.Grid;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle
{
    /// <summary>
    ///     Handles the game flow
    /// </summary>
    public class PuzzleController: SingletonBehaviour<PuzzleController>
    {
        /// <summary>
        ///     Reference to the game grid
        /// </summary>
        [SerializeField] private GameGrid grid;

        public bool IsPlayable = true;

        public EventHub EventHub { get; private set; }

        /// <summary>
        ///     Sets up the grid and runs the puzzle loop for the lifetime of the battle.
        ///     The loop only advances while IsPlayable is true (hero input window).
        /// </summary>
        private IEnumerator Start()
        {
            InitializeGame();

            yield return StartCoroutine(RunGame());
        }

        public void Initialize(EventHub hub)
        {
            EventHub = hub;
        }


        /// <summary>
        ///     Initilaizes the game and the grid
        /// </summary>
        public void InitializeGame()
        {
            grid.SetUpGrid();
        }

        /// <summary>
        ///     Runs the puzzle loop. It has no move limit; it stops when the
        ///     battle scene unloads and this object is destroyed.
        /// </summary>
        public IEnumerator RunGame()
        {
            while (true)
            {
                yield return WaitGame();

                //Wait for the Player to select elements
                yield return grid.WaitForSelection();

                //Despawn selected elements
                yield return grid.DespawnSelection();

                //Wait for the grid elements to finish movement
                yield return grid.WaitForMovement();

                //Respawn despawned elements
                yield return grid.RespawnElements();
            }
        }

        /// <summary>
        ///     Waits until the puzzle is playable (hero input window open).
        /// </summary>
        public IEnumerator WaitGame()
        {
            yield return new WaitUntil(() => IsPlayable);
        }
    }
}