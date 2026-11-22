using System.Collections;
using DG.Tweening;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero.Payloads;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle
{
    /// <summary>
    /// Manages the victory UI sequence.
    /// Listens for HeroVictoryIdleStartedEvent and coordinates
    /// the fade-out of battle UI, puzzle slide-out, and victory canvas fade-in.
    /// Attach to a persistent GameObject in the Battle scene.
    /// </summary>
    public sealed class VictoryUIController : MonoBehaviour
    {
        // CONSTANTS
        private const float PuzzleSlideOutY = -12f;
        private const float FadeDuration = 0.6f;
        private const float SlideDuration = 0.5f;
        private const float DelayBeforeFadeIn = 0.3f;

        // FIELDS
        [Header("Canvas")]
        [SerializeField] private CanvasGroup _battleUICanvasGroup;
        [SerializeField] private CanvasGroup _victoryCanvasGroup;

        [Header("Puzzle")]
        [SerializeField] private Transform _puzzleManagerTransform;

        [Header("Victory UI Elements")]
        [SerializeField] private Button _nextButton;

        private EventHub _eventHub;

        // UNITY CALLBACKS

        private void Awake()
        {
            _victoryCanvasGroup.alpha = 0f;
            _victoryCanvasGroup.interactable = false;
            _victoryCanvasGroup.blocksRaycasts = false;

            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            _nextButton.onClick.RemoveListener(OnNextButtonClicked);
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

            GameLogger.Log(LogSourceType.BattleSystem, "VictoryUIController initialized.");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.HeroEvents.HeroVictoryIdleStartedEvent
                .Register(OnVictoryIdleStarted);
        }

        private void UnregisterEvents()
        {
            _eventHub?.HeroEvents.HeroVictoryIdleStartedEvent
                .Unregister(OnVictoryIdleStarted);
        }

        /// <summary>
        /// Fades out battle UI and slides out puzzle simultaneously,
        /// then fades in victory canvas after a short delay.
        /// </summary>
        private IEnumerator PlayVictorySequence()
        {
            // Battle UI fade out ve Puzzle slide out aynı anda
            _battleUICanvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.OutQuad);

            _puzzleManagerTransform.DOLocalMoveY(PuzzleSlideOutY, SlideDuration)
                .SetEase(Ease.InBack);

            yield return new WaitForSeconds(DelayBeforeFadeIn);

            // Victory Canvas fade in
            _victoryCanvasGroup.DOFade(1f, FadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _victoryCanvasGroup.interactable = true;
                    _victoryCanvasGroup.blocksRaycasts = true;

                    GameLogger.Log(LogSourceType.BattleSystem,
                        "VictoryUIController: Victory canvas visible.");
                });
        }

        // EVENT HANDLERS

        private void OnVictoryIdleStarted(HeroVictoryIdleStartedPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                "VictoryUIController: Victory idle started. Beginning UI sequence.");

            StartCoroutine(PlayVictorySequence());
        }

        private void OnNextButtonClicked()
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                "VictoryUIController: Next button clicked.");

            // Şimdilik sahneyi reload et, ileride level sistemi buraya bağlanır
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}