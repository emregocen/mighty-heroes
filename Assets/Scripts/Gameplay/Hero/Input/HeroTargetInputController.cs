using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Formation;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Input
{
    /// <summary>
    /// Handles player-controlled hero target selection input.
    /// Q selects the previous enemy in the active row.
    /// E selects the next enemy in the active row.
    /// </summary>
    public sealed class HeroTargetInputController : MonoBehaviour
    {
        // FIELDS

        private BattleFormation _battleFormation;
        private HeroComponent _heroComponent;

        private bool _isInitialized;

        // UNITY CALLBACKS

        private void Update()
        {
            if (!CanReadInput())
                return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                SelectPreviousTarget();

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                SelectNextTarget();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Initializes target selection input dependencies.
        /// </summary>
        /// <param name="battleFormation">
        /// Battle formation that owns current target selection.
        /// </param>
        /// <param name="heroComponent">
        /// Hero component that exposes whether target selection is currently allowed.
        /// </param>
        public void Initialize(BattleFormation battleFormation, HeroComponent heroComponent)
        {
            _battleFormation = battleFormation;
            _heroComponent = heroComponent;

            _isInitialized =
                _battleFormation != null &&
                _heroComponent != null;

            if (!_isInitialized)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "HeroTargetInputController.Initialize failed: missing dependencies.");
                return;
            }

            GameLogger.Log(LogSourceType.BattleSystem,
                "HeroTargetInputController initialized.");
        }

        // PRIVATE METHODS

        /// <summary>
        /// Returns true when target selection input should be processed.
        /// </summary>
        private bool CanReadInput()
        {
            if (!_isInitialized)
                return false;

            return _heroComponent.CanChangeTarget;
        }

        /// <summary>
        /// Selects the previous alive enemy in the active formation row.
        /// </summary>
        private void SelectPreviousTarget()
        {
            _battleFormation.SelectPreviousTarget();
        }

        /// <summary>
        /// Selects the next alive enemy in the active formation row.
        /// </summary>
        private void SelectNextTarget()
        {
            _battleFormation.SelectNextTarget();
        }
    }
}