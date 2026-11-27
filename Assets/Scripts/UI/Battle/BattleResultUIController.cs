using System.Collections;
using DG.Tweening;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.TurnFlow.Payloads;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Level;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle
{
    /// <summary>
    /// Shows the battle result screens and routes their buttons.
    /// Victory (on HeroVictoryIdleStartedEvent): Next Battle and Main Menu;
    /// Next Battle is hidden on the last level of the catalog.
    /// Defeat (on TurnBattleDefeatedEvent): Retry and Main Menu.
    /// Both screens share one reveal: battle UI fades out, the puzzle slides
    /// down, then the result canvas fades in. Scene changes go through
    /// BattleLoader, so the fade and the level's environment scene are used.
    /// Initialized by BattleBootstrapper.
    /// </summary>
    public sealed class BattleResultUIController : MonoBehaviour
    {
        // CONSTANTS

        private const float PuzzleSlideOutY = -12f;
        private const float FadeDuration = 0.6f;
        private const float SlideDuration = 0.5f;
        private const float DelayBeforeFadeIn = 0.3f;

        // FIELDS

        [Header("Battle")]
        [Tooltip("Battle HUD, faded out when a result screen opens.")]
        [SerializeField] private CanvasGroup _battleUICanvasGroup;

        [Tooltip("Puzzle root, slid down when a result screen opens.")]
        [SerializeField] private Transform _puzzleManagerTransform;

        [Header("Victory")]
        [SerializeField] private CanvasGroup _victoryCanvasGroup;

        [Tooltip("Loads the next level. Hidden on the last level.")]
        [SerializeField] private Button _nextBattleButton;

        [SerializeField] private Button _victoryMainMenuButton;

        [Header("Defeat")]
        [SerializeField] private CanvasGroup _defeatCanvasGroup;

        [Tooltip("Reloads the current level.")]
        [SerializeField] private Button _retryButton;

        [SerializeField] private Button _defeatMainMenuButton;

        private EventHub _eventHub;
        private LevelCatalogData _levelCatalog;
        private int _currentLevelIndex;
        private bool _isResultShown;
        private bool _isLeaving;

        // PROPERTIES

        /// <summary>True when the current level is the last one in the catalog.</summary>
        private bool IsLastLevel => _levelCatalog == null || _currentLevelIndex >= _levelCatalog.Count - 1;

        // UNITY CALLBACKS

        private void Awake()
        {
            HideCanvas(_victoryCanvasGroup);
            HideCanvas(_defeatCanvasGroup);

            AddListener(_nextBattleButton, OnNextBattleClicked);
            AddListener(_victoryMainMenuButton, OnMainMenuClicked);
            AddListener(_retryButton, OnRetryClicked);
            AddListener(_defeatMainMenuButton, OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            RemoveListener(_nextBattleButton, OnNextBattleClicked);
            RemoveListener(_victoryMainMenuButton, OnMainMenuClicked);
            RemoveListener(_retryButton, OnRetryClicked);
            RemoveListener(_defeatMainMenuButton, OnMainMenuClicked);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Binds the controller to the battle and the level being played.
        /// Called by BattleBootstrapper during scene setup.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="levelCatalog">Catalog the current level belongs to.</param>
        /// <param name="currentLevelIndex">Index of the current level in the catalog.</param>
        public void Initialize(EventHub eventHub, LevelCatalogData levelCatalog, int currentLevelIndex)
        {
            _eventHub = eventHub;
            _levelCatalog = levelCatalog;
            _currentLevelIndex = Mathf.Max(0, currentLevelIndex);

            RegisterEvents();

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleResultUIController initialized. Level={_currentLevelIndex} IsLast={IsLastLevel}");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.HeroEvents.HeroVictoryIdleStartedEvent.Register(OnVictoryIdleStarted);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Register(OnBattleDefeated);
        }

        private void UnregisterEvents()
        {
            if (_eventHub == null)
                return;

            _eventHub.HeroEvents.HeroVictoryIdleStartedEvent.Unregister(OnVictoryIdleStarted);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Unregister(OnBattleDefeated);
        }

        /// <summary>
        /// Fades out the battle UI and slides the puzzle out together, then fades
        /// in the result canvas after a short delay and makes it interactable.
        /// </summary>
        private IEnumerator PlayRevealSequence(CanvasGroup resultCanvas)
        {
            if (_battleUICanvasGroup != null)
                _battleUICanvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.OutQuad);

            if (_puzzleManagerTransform != null)
                _puzzleManagerTransform.DOLocalMoveY(PuzzleSlideOutY, SlideDuration).SetEase(Ease.InBack);

            yield return new WaitForSeconds(DelayBeforeFadeIn);

            resultCanvas.DOFade(1f, FadeDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                resultCanvas.interactable = true;
                resultCanvas.blocksRaycasts = true;
            });
        }

        /// <summary>Shows a result screen once per battle.</summary>
        private void ShowResult(CanvasGroup resultCanvas)
        {
            if (_isResultShown || resultCanvas == null)
                return;

            _isResultShown = true;
            StartCoroutine(PlayRevealSequence(resultCanvas));
        }

        /// <summary>Blocks further button presses once a scene change has started.</summary>
        private bool TryBeginLeaving()
        {
            if (_isLeaving)
                return false;

            _isLeaving = true;
            return true;
        }

        private static void HideCanvas(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.RemoveListener(action);
        }

        // EVENT HANDLERS

        private void OnVictoryIdleStarted(HeroVictoryIdleStartedPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem, "BattleResultUIController: victory, showing result screen.");

            if (_nextBattleButton != null)
                _nextBattleButton.gameObject.SetActive(!IsLastLevel);

            ShowResult(_victoryCanvasGroup);
        }

        private void OnBattleDefeated(TurnBattleDefeatedPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem, $"BattleResultUIController: defeat, showing result screen. RemainingEnemies={payload.RemainingEnemyCount}");

            ShowResult(_defeatCanvasGroup);
        }

        private void OnNextBattleClicked()
        {
            if (!TryBeginLeaving())
                return;

            BattleLoader.LoadLevel(_levelCatalog, _currentLevelIndex + 1);
        }

        private void OnRetryClicked()
        {
            if (!TryBeginLeaving())
                return;

            BattleLoader.LoadLevel(_levelCatalog, _currentLevelIndex);
        }

        private void OnMainMenuClicked()
        {
            if (!TryBeginLeaving())
                return;

            BattleLoader.LoadMainMenu();
        }
    }
}
