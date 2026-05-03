using System.Collections;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero
{
    /// <summary>
    /// Rotates the hero 180 degrees during the victory turn phase.
    /// Listens for HeroVictoryTurnStartedEvent and smoothly rotates
    /// the hero transform to match the turn animation duration.
    /// Attach to the hero GameObject.
    /// </summary>
    public sealed class HeroVictoryRotator : MonoBehaviour
    {
        // CONSTANTS
        private const float RotationDegrees = 180f;

        // FIELDS
        [SerializeField]
        [Tooltip("Must match the duration of the VictoryTurn animation clip.")]
        private float _turnDuration = 1.26f;

        private EventHub _eventHub;
        private Coroutine _rotationCoroutine;

        // UNITY CALLBACKS

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Initializes the rotator with the battle-scoped EventHub.
        /// Called by BattleBootstrapper during scene setup.
        /// </summary>
        public void Initialize(EventHub eventHub)
        {
            _eventHub = eventHub;
            RegisterEvents();

            GameLogger.Log(LogSourceType.Hero, "HeroVictoryRotator initialized.");
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

        /// <summary>
        /// Rotates the hero 180 degrees over the turn animation duration.
        /// Uses SmoothStep for natural ease-in/ease-out feel.
        /// </summary>
        private IEnumerator RotateCoroutine()
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.Euler(0f, RotationDegrees, 0f);
            float elapsed = 0f;

            while (elapsed < _turnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _turnDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
                yield return null;
            }

            transform.rotation = targetRotation;

            GameLogger.Log(LogSourceType.Hero, "HeroVictoryRotator: 180 degree rotation complete.");
        }

        // EVENT HANDLERS

        private void OnVictoryTurnStarted(HeroVictoryTurnStartedPayload payload)
        {
            if (_rotationCoroutine != null)
                StopCoroutine(_rotationCoroutine);

            _rotationCoroutine = StartCoroutine(RotateCoroutine());

            GameLogger.Log(LogSourceType.Hero, "HeroVictoryRotator: Rotation started.");
        }
    }
}