using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero.Payloads;
using Unity.Cinemachine;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Manages the camera transition during the victory sequence.
    /// Listens for HeroVictoryTurnStartedEvent and raises the priority
    /// of CM_VictoryCamera above CM_BattleCamera so CinemachineBrain
    /// blends to the close-up shot automatically.
    /// </summary>
    public sealed class VictoryCameraController : MonoBehaviour
    {
        // CONSTANTS
        private const int PriorityActive = 20;
        private const int PriorityInactive = 9;

        // FIELDS
        [SerializeField] private CinemachineCamera _victoryCamera;

        private EventHub _eventHub;

        // UNITY CALLBACKS

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Initializes the controller with the battle-scoped EventHub.
        /// Called by BattleBootstrapper during scene setup.
        /// </summary>
        public void Initialize(EventHub eventHub)
        {
            _eventHub = eventHub;
            RegisterEvents();

            GameLogger.Log(LogSourceType.BattleSystem, "VictoryCameraController initialized.");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.HeroEvents.HeroVictoryTurnStartedEvent
                .Register(OnVictoryTurnStarted);
        }

        private void UnregisterEvents()
        {
            _eventHub?.HeroEvents.HeroVictoryTurnStartedEvent
                .Unregister(OnVictoryTurnStarted);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Activates the victory camera by raising its priority above the battle camera.
        /// CinemachineBrain handles the blend automatically.
        /// </summary>
        private void OnVictoryTurnStarted(HeroVictoryTurnStartedPayload payload)
        {
            if (_victoryCamera == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "VictoryCameraController: VictoryCamera reference is missing.");
                return;
            }

            _victoryCamera.Priority = PriorityActive;

            GameLogger.Log(LogSourceType.BattleSystem,
                "VictoryCameraController: Victory camera activated.");
        }
    }
}