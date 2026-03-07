using System;
using System.Collections;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Puzzle.Core;
using MightyHeroes.Gameplay.Puzzle.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.Gameplay.Puzzle
{
    /// <summary>
    ///     Handles the game flow
    /// </summary>
    public class PuzzleController: SingletonBehaviour<PuzzleController>
    {
        [SerializeField] private Button returnButton;

        /// <summary>
        ///     Reference to the game grid
        /// </summary>
        [SerializeField] private GameGrid grid;

        /// <summary>
        ///     The number of moves the player has left
        /// </summary>
        [SerializeField] private int movesAvailable = 120;

        public bool IsPlayable = true;

        /// <summary>
        ///     The score of the player
        /// </summary>
        public static int Score { get; private set; }

        /// <summary>
        ///     Gets or sets the  number of moves the player has left
        /// </summary>
        public int MovesAvailable
        {
            get => movesAvailable;
            set => movesAvailable = value;
        }

        public EventHub EventHub { get; private set; }

        /// <summary>
        ///     Starts and processes the game flow
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            //this.returnButton.onClick.AddListener(OnBackButtonClick);

            InitializeGame();

            //Wait for the game to be executed completely
            yield return StartCoroutine(RunGame());

            //Wait for the game to finish
            yield return StartCoroutine(EndGame());
        }

        /// <summary>
        ///     Check for escape button
        /// </summary>
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape)) OnBackButtonClick();
        }

        /// <summary>
        ///     Start listening to relevant events
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnElementsDespawned.AddListener(OnElementsDespawned);
        }

        //Stop listening from all events
        private void OnDisable()
        {
            GameEvents.OnElementsDespawned.RemoveListener(OnElementsDespawned);
        }

        /// <summary>
        ///     Returns to the menu scene
        /// </summary>
        protected void OnBackButtonClick()
        {
            SceneLoadingManager.Instance.LoadScene(SceneNames.Menu);
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
            Score = 0;

            grid.SetUpGrid();
        }

        /// <summary>
        ///     Runs the game loop
        /// </summary>
        /// <returns></returns>
        public IEnumerator RunGame()
        {
            //Game Loop
            while (MovesAvailable > 0)
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
        ///     Loads the game over scene
        /// </summary>
        /// <returns></returns>
        public IEnumerator EndGame()
        {
            yield return new WaitForSeconds(0.5f);

            SceneLoadingManager.Instance.LoadScene(SceneNames.GameOver);
        }

        public IEnumerator WaitGame()
        {
            yield return new WaitUntil(() => IsPlayable);
        }

        /// <summary>
        ///     Gets invoked when the user has finished its move and
        ///     the selected elements are despawned
        /// </summary>
        /// <param name="count"></param>
        private void OnElementsDespawned(ActionType elementType, int count)
        {
            //Update score
            var oldScore = Score;
            Score = oldScore + count * (count - 1);

            //Invoke score changed event
            GameEvents.OnScoreChanged.Invoke(oldScore, Score);
        }
    }
}