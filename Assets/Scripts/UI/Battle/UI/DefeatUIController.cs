using System.Collections;
using DG.Tweening;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow.Payloads;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle
{
    /// <summary>
    /// Manages the defeat UI sequence.
    /// Listens for BattleDefeatPayload and coordinates
    /// the fade-out of battle UI, puzzle slide-out, and defeat canvas fade-in.
    /// Attach to a persistent GameObject in the Battle scene.
    /// </summary>
    public sealed class DefeatUIController : MonoBehaviour
    {
        // CONSTANTS
        private const float PuzzleSlideOutY = -12f;
        private const float FadeDuration = 0.6f;
        private const float SlideDuration = 0.5f;
        private const float DelayBeforeFadeIn = 0.3f;

        // FIELDS
        [Header("Canvas")]
        [SerializeField] private CanvasGroup _battleUICanvasGroup;
        [SerializeField] private CanvasGroup _defeatCanvasGroup;

        [Header("Puzzle")]
        [SerializeField] private Transform _puzzleManagerTransform;

        [Header("Defeat UI Elements")]
        [SerializeField] private Button _retryButton;

        private EventHub _eventHub;

        // UNITY CALLBACKS

        private void Awake()
        {
            _defeatCanvasGroup.alpha = 0f;
            _defeatCanvasGroup.interactable = false;
            _defeatCanvasGroup.blocksRaycasts = false;

            _retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            _retryButton.onClick.RemoveListener(OnRetryButtonClicked);
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

            GameLogger.Log(LogSourceType.BattleSystem, "DefeatUIController initialized.");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Register(OnBattleDefeated);
        }

        private void UnregisterEvents()
        {
            _eventHub?.TurnEvents.TurnBattleDefeatedEvent.Unregister(OnBattleDefeated);
        }

        /// <summary>
        /// Fades out battle UI and slides out puzzle simultaneously,
        /// then fades in defeat canvas after a short delay.
        /// </summary>
        private IEnumerator PlayDefeatSequence()
        {
            _battleUICanvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.OutQuad);

            _puzzleManagerTransform.DOLocalMoveY(PuzzleSlideOutY, SlideDuration)
                .SetEase(Ease.InBack);

            yield return new WaitForSeconds(DelayBeforeFadeIn);

            _defeatCanvasGroup.DOFade(1f, FadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _defeatCanvasGroup.interactable = true;
                    _defeatCanvasGroup.blocksRaycasts = true;

                    GameLogger.Log(LogSourceType.BattleSystem,
                        "DefeatUIController: Defeat canvas visible.");
                });
        }

        // EVENT HANDLERS

        private void OnBattleDefeated(TurnBattleDefeatedPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                $"DefeatUIController: Battle defeated. RemainingEnemies={payload.RemainingEnemyCount}");

            StartCoroutine(PlayDefeatSequence());
        }

        private void OnRetryButtonClicked()
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                "DefeatUIController: Retry button clicked.");

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}