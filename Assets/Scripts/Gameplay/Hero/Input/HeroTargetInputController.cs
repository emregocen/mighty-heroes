using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Enemy;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Input
{
    /// <summary>
    /// Handles player-controlled hero target selection input.
    ///
    /// Keyboard: Q selects the previous enemy, E the next enemy in the front line.
    /// Touch: the optional TargetArrowsView raises the same two requests from
    /// arrow buttons at the left and right middle of the screen.
    ///
    /// Both inputs go through the same rule: target changes are accepted only
    /// while the hero allows it (CanChangeTarget) and the front line has more
    /// than one alive enemy. The arrows are shown only in that window.
    /// </summary>
    public sealed class HeroTargetInputController : MonoBehaviour
    {
        // CONSTANTS

        private const int MinAliveEnemiesForSelection = 2;

        // FIELDS

        [Tooltip("Optional on-screen arrows for touch target selection.")]
        [SerializeField] private TargetArrowsView _targetArrowsView;

        private BattleFormation _battleFormation;
        private HeroComponent _heroComponent;

        private bool _isInitialized;
        private bool _areArrowsVisible;

        // UNITY CALLBACKS

        private void Update()
        {
            if (!_isInitialized)
                return;

            bool canSelect = CanSelectTarget();

            UpdateArrowVisibility(canSelect);

            if (!canSelect)
                return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                SelectPreviousTarget();

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                SelectNextTarget();
        }

        private void OnDestroy()
        {
            if (_targetArrowsView == null)
                return;

            _targetArrowsView.PreviousRequested -= OnPreviousRequested;
            _targetArrowsView.NextRequested -= OnNextRequested;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Initializes target selection input dependencies.
        /// </summary>
        /// <param name="battleFormation">Battle formation that owns current target selection.</param>
        /// <param name="heroComponent">Hero component that exposes whether target selection is currently allowed.</param>
        public void Initialize(BattleFormation battleFormation, HeroComponent heroComponent)
        {
            _battleFormation = battleFormation;
            _heroComponent = heroComponent;

            _isInitialized = _battleFormation != null && _heroComponent != null;

            if (!_isInitialized)
            {
                GameLogger.Error(LogSourceType.BattleSystem, "HeroTargetInputController.Initialize failed: missing dependencies.");
                return;
            }

            if (_targetArrowsView != null)
            {
                _targetArrowsView.PreviousRequested += OnPreviousRequested;
                _targetArrowsView.NextRequested += OnNextRequested;
                _targetArrowsView.SetVisible(false);
            }

            GameLogger.Log(LogSourceType.BattleSystem, "HeroTargetInputController initialized.");
        }

        // PRIVATE METHODS

        /// <summary>
        /// Returns true when the hero allows a target change and there is more
        /// than one alive enemy in the front line to choose from.
        /// </summary>
        private bool CanSelectTarget()
        {
            if (!_heroComponent.CanChangeTarget)
                return false;

            return CountAliveFrontLineEnemies() >= MinAliveEnemiesForSelection;
        }

        /// <summary>Counts alive enemies in the current front line.</summary>
        private int CountAliveFrontLineEnemies()
        {
            BattleLine frontLine = _battleFormation.FrontLine;

            if (frontLine == null)
                return 0;

            int count = 0;

            foreach (EnemyComponent enemy in frontLine.Enemies)
            {
                if (enemy != null && enemy.IsAlive)
                    count++;
            }

            return count;
        }

        /// <summary>Pushes arrow visibility to the view only when it changes.</summary>
        private void UpdateArrowVisibility(bool isVisible)
        {
            if (_targetArrowsView == null || isVisible == _areArrowsVisible)
                return;

            _areArrowsVisible = isVisible;
            _targetArrowsView.SetVisible(isVisible);
        }

        /// <summary>Selects the previous alive enemy in the active formation row.</summary>
        private void SelectPreviousTarget()
        {
            _battleFormation.SelectPreviousTarget();
        }

        /// <summary>Selects the next alive enemy in the active formation row.</summary>
        private void SelectNextTarget()
        {
            _battleFormation.SelectNextTarget();
        }

        // EVENT HANDLERS

        private void OnPreviousRequested()
        {
            if (_isInitialized && CanSelectTarget())
                SelectPreviousTarget();
        }

        private void OnNextRequested()
        {
            if (_isInitialized && CanSelectTarget())
                SelectNextTarget();
        }
    }
}
