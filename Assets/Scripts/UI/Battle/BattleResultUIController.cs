using System.Collections;
using DG.Tweening;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.TurnFlow.Payloads;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Level;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle
{
    /// <summary>
    /// Shows the battle result screens and routes their buttons.
    /// Victory: the battle HUD fades out and the puzzle slides out as soon as
    /// the battle is won (TurnBattleVictoryEvent, with the hero's victory
    /// animation); on HeroVictoryIdleStartedEvent VictoryPanelView plays the star
    /// rating from LevelResultTracker; Collect Rewards opens RewardPanelView with
    /// random gold and XP; OK returns to the main menu.
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

        [Tooltip("Victory screen content: rating, stars and Collect Rewards.")]
        [SerializeField] private VictoryPanelView _victoryPanel;

        [Tooltip("Reward popup opened by Collect Rewards. OK returns to the main menu.")]
        [SerializeField] private RewardPanelView _rewardPanel;

        [Header("Rewards (placeholder)")]
        [Tooltip("Random gold range, inclusive.")]
        [SerializeField] private Vector2Int _goldRange = new Vector2Int(50, 150);

        [Tooltip("Random XP range, inclusive.")]
        [SerializeField] private Vector2Int _xpRange = new Vector2Int(20, 60);

        [Header("Defeat")]
        [SerializeField] private CanvasGroup _defeatCanvasGroup;

        [Tooltip("Reloads the current level.")]
        [SerializeField] private Button _retryButton;

        [SerializeField] private Button _defeatMainMenuButton;

        private EventHub _eventHub;
        private LevelCatalogData _levelCatalog;
        private int _currentLevelIndex;
        private LevelResultTracker _resultTracker;
        private bool _isResultShown;
        private bool _isBattleUIHidden;
        private bool _isLeaving;

        // UNITY CALLBACKS

        private void Awake()
        {
            HideCanvas(_victoryCanvasGroup);
            HideCanvas(_defeatCanvasGroup);

            EnsureRaycaster(_victoryCanvasGroup);
            EnsureRaycaster(_defeatCanvasGroup);

            if (_victoryPanel != null)
                _victoryPanel.CollectRewardsClicked += OnCollectRewardsClicked;

            if (_rewardPanel != null)
            {
                _rewardPanel.OkClicked += OnRewardOkClicked;
                _rewardPanel.gameObject.SetActive(false);
            }

            AddListener(_retryButton, OnRetryClicked);
            AddListener(_defeatMainMenuButton, OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            if (_victoryPanel != null)
                _victoryPanel.CollectRewardsClicked -= OnCollectRewardsClicked;

            if (_rewardPanel != null)
                _rewardPanel.OkClicked -= OnRewardOkClicked;

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
        /// <param name="resultTracker">Provides the turns and stars of the win.</param>
        public void Initialize(EventHub eventHub, LevelCatalogData levelCatalog, int currentLevelIndex, LevelResultTracker resultTracker)
        {
            _eventHub = eventHub;
            _levelCatalog = levelCatalog;
            _currentLevelIndex = Mathf.Max(0, currentLevelIndex);
            _resultTracker = resultTracker;

            RegisterEvents();

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleResultUIController initialized. Level={_currentLevelIndex}");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.TurnEvents.TurnBattleVictoryEvent.Register(OnBattleVictory);
            _eventHub.HeroEvents.HeroVictoryIdleStartedEvent.Register(OnVictoryIdleStarted);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Register(OnBattleDefeated);
        }

        private void UnregisterEvents()
        {
            if (_eventHub == null)
                return;

            _eventHub.TurnEvents.TurnBattleVictoryEvent.Unregister(OnBattleVictory);
            _eventHub.HeroEvents.HeroVictoryIdleStartedEvent.Unregister(OnVictoryIdleStarted);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Unregister(OnBattleDefeated);
        }

        /// <summary>
        /// Fades out the battle UI and slides the puzzle out together, then fades
        /// in the result canvas after a short delay and makes it interactable.
        /// </summary>
        private IEnumerator PlayRevealSequence(CanvasGroup resultCanvas)
        {
            HideBattleUI();

            yield return new WaitForSeconds(DelayBeforeFadeIn);

            resultCanvas.DOFade(1f, FadeDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                resultCanvas.interactable = true;
                resultCanvas.blocksRaycasts = true;
            });
        }

        /// <summary>
        /// Fades out the battle HUD and slides the puzzle out, once per battle.
        /// On victory this starts with the hero's victory animation, before the
        /// result screen; on defeat it runs with the result screen.
        /// </summary>
        private void HideBattleUI()
        {
            if (_isBattleUIHidden)
                return;

            _isBattleUIHidden = true;

            if (_battleUICanvasGroup != null)
            {
                _battleUICanvasGroup.interactable = false;
                _battleUICanvasGroup.blocksRaycasts = false;
                _battleUICanvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.OutQuad);
            }

            if (_puzzleManagerTransform != null)
                _puzzleManagerTransform.DOLocalMoveY(PuzzleSlideOutY, SlideDuration).SetEase(Ease.InBack);
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

        /// <summary>
        /// Buttons on a canvas only receive clicks through a GraphicRaycaster.
        /// Adds one (and warns) when the result canvas is missing it.
        /// </summary>
        private static void EnsureRaycaster(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
                return;

            Canvas canvas = canvasGroup.GetComponentInParent<Canvas>();

            if (canvas == null || canvas.GetComponent<GraphicRaycaster>() != null)
                return;

            canvas.gameObject.AddComponent<GraphicRaycaster>();
            GameLogger.Warning(LogSourceType.UI, $"BattleResultUIController: '{canvas.name}' had no GraphicRaycaster, added one at runtime. Add it to the prefab.");
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

        private void OnBattleVictory(TurnBattleVictoryPayload payload)
        {
            HideBattleUI();
        }

        private void OnVictoryIdleStarted(HeroVictoryIdleStartedPayload payload)
        {
            if (_isResultShown)
                return;

            LevelResult result = _resultTracker != null && _resultTracker.HasResult ? _resultTracker.LastResult : new LevelResult(1, 1);

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleResultUIController: victory, showing result screen. Turns={result.TurnsTaken} Stars={result.Stars}");

            if (_victoryPanel != null)
                _victoryPanel.Play(result, DelayBeforeFadeIn + FadeDuration * 0.5f);

            ShowResult(_victoryCanvasGroup);
        }

        private void OnBattleDefeated(TurnBattleDefeatedPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem, $"BattleResultUIController: defeat, showing result screen. RemainingEnemies={payload.RemainingEnemyCount}");

            ShowResult(_defeatCanvasGroup);
        }

        private void OnCollectRewardsClicked()
        {
            if (_rewardPanel == null)
            {
                OnMainMenuClicked();
                return;
            }

            int gold = Random.Range(_goldRange.x, _goldRange.y + 1);
            int xp = Random.Range(_xpRange.x, _xpRange.y + 1);

            _rewardPanel.Show(gold, xp);
        }

        private void OnRewardOkClicked()
        {
            OnMainMenuClicked();
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
