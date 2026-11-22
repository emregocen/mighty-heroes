using System.Collections.Generic;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Environment;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Battle.Intro;
using MightyHeroes.Gameplay.Battle.Targeting;
using MightyHeroes.Gameplay.Battle.Timer;
using MightyHeroes.Gameplay.Battle.TurnFlow;
using MightyHeroes.Gameplay.Combat;
using MightyHeroes.Gameplay.Combat.Feedback;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Enemy;
using MightyHeroes.Gameplay.Enemy.AI;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using MightyHeroes.Gameplay.FloatingText;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Hero.Presentation;
using MightyHeroes.Gameplay.Hero.StateMachine;
using MightyHeroes.Gameplay.Level;
using MightyHeroes.Gameplay.Puzzle;
using MightyHeroes.Gameplay.Ultimate;
using MightyHeroes.UI.Battle;
using MightyHeroes.UI.Battle.Presenters;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Composition root for all battle-scoped systems.
    ///
    /// Responsibilities:
    /// - Creates and wires all battle-lifetime services.
    /// - Initializes BattleFormation with formation layout from Inspector config.
    /// - Hands battle-end orchestration (entity death, timer expiry, victory/defeat
    ///   transitions) to BattleFlowController. Bootstrapper itself owns no runtime
    ///   gameplay decisions.
    /// - Disposes all systems on scene destroy to prevent memory leaks.
    ///
    /// Initialization order (Awake):
    /// 1. EventHub               — must exist before any subscriber
    /// 2. BattleFormation        — created before hero so HeroStateMachine can receive reference
    /// 3. Hero                   — requires BattleFormation for IsTargetAlive check
    /// 4. Enemies                — initialized and placed into formation
    /// 5. TurnSystem             — needs IFormation from BattleFormation
    /// 6. CombatSystem           — needs IBattleField from BattleFormation
    /// 7. PuzzleSystem           — receives EventHub reference
    /// 8. HUD / UI controllers   — receive EventHub for subscriptions
    /// 9. BattleTimer            — optional, only if BattleTimerConfig is assigned
    /// 10. BattleFlowController  — created last so it sees a fully wired EventHub
    /// </summary>
    public sealed class BattleBootstrapper : MonoBehaviour
    {
        // FIELDS — Inspector

        [Header("Systems")]
        [SerializeField] private TurnSystemController _turnSystemController;
        [SerializeField] private PuzzleController _puzzleController;

        [Header("UI")]
        [SerializeField] private BattleUIPresenter _battleUIPresenter;
        [SerializeField] private VictoryUIController _victoryUIController;
        [SerializeField] private DefeatUIController _defeatUIController;

        [Header("Camera")]
        [SerializeField] private VictoryCameraController _victoryCameraController;

        [Header("Input")]
        [SerializeField] private HeroTargetInputController _heroTargetInputController;

        [Header("Hero")]
        [SerializeField] private HeroComponent _heroComponent;
        [SerializeField] private HeroVictoryRotator _heroVictoryRotator;
        [SerializeField] private HeroTargetFocusController _heroTargetFocusController;
        [SerializeField] private ActionTierData _actionTierData;

        [Header("Level")]
        [Tooltip("Levels in play order. Required: every battle is built from a level.")]
        [SerializeField] private LevelCatalogData _levelCatalog;

        [Tooltip("Level played when the battle scene is started directly (no level selected from the menu), e.g. from the Editor.")]
        [SerializeField, Min(0)] private int _fallbackLevelIndex;

        [Tooltip("Default enemy formation distances. A level can override it.")]
        [SerializeField] private FormationLayoutConfig _defaultFormationLayout;


        [Header("Battle Timer")]
        [Tooltip("Optional. When empty, no timer is created for this battle.")]
        [SerializeField] private BattleTimerConfig _battleTimerConfig;

        [Tooltip("UI view that renders the remaining seconds. Required only if a timer config is set.")]
        [SerializeField] private BattleTimerView _battleTimerView;

        [Header("Intro")]
        [Tooltip("Optional. When empty, battle starts immediately.")]
        [SerializeField] private BattleIntroConfig _battleIntroConfig;
        [SerializeField] private BattleIntroController _battleIntroController;

        [Header("Combat Feedback")]
        [Tooltip("Tier-based camera shake and hit VFX tuning. Leave empty to disable combat feedback.")]
        [SerializeField] private CombatFeedbackData _combatFeedbackData;
        [Tooltip("Impulse source wrapper used for camera shake. Optional. The battle camera needs a CinemachineImpulseListener.")]
        [SerializeField] private CinemachineImpulseShaker _cameraShaker;
        [Tooltip("Pooled player for hit VFX. Optional.")]
        [SerializeField] private PooledHitVfxPlayer _hitVfxPlayer;
        [Tooltip("Side kill-cam and slow motion on high-tier killing hits. Optional.")]
        [SerializeField] private KillCamController _killCamController;
        [Tooltip("Pulled-back wide camera while the hero ultimate plays. Optional.")]
        [SerializeField] private UltimateCameraController _ultimateCameraController;

        [Header("Targeting")]
        [Tooltip("World-space marker shown above the currently selected enemy. Optional.")]
        [SerializeField] private TargetMarkerView _targetMarkerView;

        [Header("Floating Text")]
        [SerializeField] private FloatingTextConfig _floatingTextConfig;
        [Tooltip("Screen-space text that follows an entity's world position (damage, heal, shield, buff, K.O.).")]
        [SerializeField] private FloatingTextAnchoredView _floatingTextAnchoredPrefab;
        [Tooltip("Fixed screen-space text (combo callouts).")]
        [SerializeField] private FloatingTextScreenView _floatingTextScreenPrefab;
        [Tooltip("Full-screen RectTransform on the HUD canvas that parents both floating text pools.")]
        [SerializeField] private Transform _floatingTextScreenParent;
        [Tooltip("World-space height above the entity root where floating texts start.")]
        [SerializeField] private float _floatingTextHeightOffset = 1.6f;

        // FIELDS — Runtime

        private EventHub _eventHub;
        private TurnContext _turnContext;
        private TurnStateMachine _turnStateMachine;
        private HeroStateMachine _heroStateMachine;
        private BattleFormation _battleFormation;
        private CombatSystem _combatSystem;
        private BattleIdGenerator _idGenerator;

        private BattleTimer _battleTimer;
        private BattleTimerRunner _battleTimerRunner;
        private BattleTimerPresenter _battleTimerPresenter;
        private BattleFlowController _battleFlowController;

        private readonly List<EnemyStateMachine> _enemyStateMachines = new List<EnemyStateMachine>();

        private EnemyFormationConfig[] _enemyFormation = System.Array.Empty<EnemyFormationConfig>();
        private BattleEnvironment _battleEnvironment;
        private BattleSite _battleSite;

        // PROPERTIES

        /// <summary>Level being played, or null when the scene-placed fallback formation is used.</summary>
        public LevelData CurrentLevel { get; private set; }

        /// <summary>Catalog index of the level being played, or -1 when no level is used.</summary>
        public int CurrentLevelIndex { get; private set; } = -1;

        private FloatingTextService _floatingTextService;
        private FloatingTextPresenter _floatingTextPresenter;
        private StatusIconPresenter _statusIconPresenter;
        private UltimateGaugePresenter _ultimateGaugePresenter;
        private UltimateTileHighlightPresenter _ultimateTileHighlightPresenter;
        private CombatFeedbackPresenter _combatFeedbackPresenter;
        private TargetMarkerPresenter _targetMarkerPresenter;

        // UNITY CALLBACKS

        private void Awake()
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleBootstrapper.Awake InstanceId={GetEntityId()}");

            _idGenerator = new BattleIdGenerator();

            CreateEventHub();
            CreateBattleFormation();
            ResolveCurrentLevel();
            ResolveBattleSite();
            SpawnLevelEnemies();
            InitializeHero();
            InitializeEnemies();
            InitializeTargetInput();
            InitializeHeroTargetFocus();
            InitializeTargetMarker();
            InitializeBattleIntro();
            InitializeTurnSystem();
            InitializeCombatSystem();
            InitializePuzzleSystem();
            InitializeBattleHud();
            InitializeVictoryUI();
            InitializeDefeatUI();
            InitializeVictoryCamera();
            InitializeVictoryRotator();
            InitializeBattleTimer();
            InitializeFloatingText();
            InitializeStatusIcons();
            InitializeUltimateGauge();
            InitializeCombatFeedback();
            InitializeBattleFlowController();
        }

        private void Start()
        {
            if (_turnStateMachine == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.Start aborted: TurnStateMachine is null.");
                return;
            }

            RefreshAllEntityHuds();

            _turnStateMachine.Start();

            if (_battleIntroController != null)
            {
                _battleIntroController.Play(
                    onSwordDrawn: EnterHeroBattleStance,
                    onComplete: () =>
                    {
                        _turnStateMachine.SwitchToStartState();
                        _battleTimer?.NotifyIntroCompleted();
                    });
            }
            else
            {
                EnterHeroBattleStance();
                _turnStateMachine.SwitchToStartState();
                _battleTimer?.NotifyIntroCompleted();
            }
        }

        private void OnDestroy()
        {
            DisposeBattleFlowController();
            DisposeFloatingText();
            DisposeStatusIcons();
            DisposeUltimateGauge();
            DisposeCombatFeedback();
            DisposeTargetMarker();
            DisposeBattleTimer();
            DisposeCombatSystem();
            DisposeEnemyComponents();
            DisposeEnemyStateMachines();
            DisposeHeroStateMachine();
            DisposeHeroComponent();

            GameLogger.Log(LogSourceType.BattleSystem, "BattleBootstrapper.OnDestroy complete.");
        }

        /// <summary>
        /// Re-pushes initial HUD values for all entities after all MonoBehaviour
        /// Awake() calls have completed. Prevents HealthBarView.Awake's ResetView
        /// from overwriting values set during Bootstrapper.Awake initialization.
        /// </summary>
        private void RefreshAllEntityHuds()
        {
            _heroComponent?.RefreshHud();

            if (_enemyFormation == null)
                return;

            foreach (EnemyFormationConfig config in _enemyFormation)
            {
                config.Enemy?.RefreshHud();
            }
        }

        // PRIVATE METHODS — Level

        /// <summary>
        /// Picks the level to play: the one selected in LevelSession, otherwise
        /// FallbackLevelIndex. Without a catalog the battle has no enemies.
        /// </summary>
        private void ResolveCurrentLevel()
        {
            if (_levelCatalog == null || _levelCatalog.Count == 0)
            {
                GameLogger.Error(LogSourceType.BattleSystem, "BattleBootstrapper: Level Catalog is missing or empty; the battle has no level.");
                return;
            }

            int index = LevelSession.SelectedLevelIndex >= 0 ? LevelSession.SelectedLevelIndex : _fallbackLevelIndex;
            index = Mathf.Clamp(index, 0, _levelCatalog.Count - 1);

            CurrentLevelIndex = index;
            CurrentLevel = _levelCatalog.Levels[index];

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleBootstrapper: playing level {index} '{(CurrentLevel != null ? CurrentLevel.DisplayName : "null")}'.");
        }

        /// <summary>
        /// Finds the environment scene's BattleEnvironment and the level's battle
        /// site, activates the site's sections and places the hero at the site's
        /// battle position facing the enemies (the intro then moves him to the
        /// site's intro position).
        /// The environment scene must already be loaded: BattleLoader loads it
        /// before BattleScene; in the Editor open it together with BattleScene.
        /// </summary>
        private void ResolveBattleSite()
        {
            if (CurrentLevel == null || string.IsNullOrEmpty(CurrentLevel.EnvironmentSceneName))
                return;

            _battleEnvironment = FindFirstObjectByType<BattleEnvironment>();

            if (_battleEnvironment == null)
            {
                GameLogger.Error(LogSourceType.BattleSystem, $"BattleBootstrapper: environment '{CurrentLevel.EnvironmentSceneName}' is not loaded. Start from the main menu, or open it together with BattleScene in the Editor.");
                return;
            }

            if (!_battleEnvironment.TryGetSite(CurrentLevel.BattleSiteId, out _battleSite))
            {
                GameLogger.Error(LogSourceType.BattleSystem, $"BattleBootstrapper: battle site '{CurrentLevel.BattleSiteId}' not found in '{CurrentLevel.EnvironmentSceneName}'.");
                return;
            }

            _battleEnvironment.ActivateSite(_battleSite);

            if (_heroComponent != null)
                _heroComponent.transform.SetPositionAndRotation(_battleSite.HeroBattlePosition, _battleSite.HeroBattleRotation);

            if (_battleIntroController != null)
                _battleIntroController.SetHeroPositions(_battleSite.HeroIntroPosition, _battleSite.HeroBattlePosition, _battleSite.HeroBattleRotation);

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleBootstrapper: battle site '{_battleSite.SiteId}'.");
        }

        /// <summary>
        /// Spawns the current level's enemies laid out relative to the hero
        /// battle position (FormationLayoutConfig: front row distance, row
        /// spacing, centered slots), facing the hero at the hero's height, then
        /// applies each EnemyData's stats and fills the runtime formation list
        /// with the spawned instances.
        /// </summary>
        private void SpawnLevelEnemies()
        {
            if (CurrentLevel == null)
                return;

            if (_battleSite == null)
            {
                GameLogger.Error(LogSourceType.BattleSystem, "BattleBootstrapper.SpawnLevelEnemies: no battle site, level enemies are not spawned.");
                return;
            }

            FormationLayoutConfig layout = _defaultFormationLayout;

            if (layout == null)
            {
                GameLogger.Error(LogSourceType.BattleSystem, "BattleBootstrapper.SpawnLevelEnemies: no FormationLayoutConfig assigned.");
                return;
            }

            List<LevelEnemySlot> slots = new List<LevelEnemySlot>(CurrentLevel.Enemies);
            slots.Sort((a, b) => a.RowIndex != b.RowIndex ? a.RowIndex.CompareTo(b.RowIndex) : a.SlotIndex.CompareTo(b.SlotIndex));

            List<EnemyFormationConfig> spawned = new List<EnemyFormationConfig>(slots.Count);
            int rowStart = 0;

            while (rowStart < slots.Count)
            {
                int rowIndex = slots[rowStart].RowIndex;
                int rowEnd = rowStart;

                while (rowEnd < slots.Count && slots[rowEnd].RowIndex == rowIndex)
                    rowEnd++;

                int rowCount = rowEnd - rowStart;

                for (int i = 0; i < rowCount; i++)
                {
                    LevelEnemySlot slot = slots[rowStart + i];
                    EnemyComponent enemy = SpawnFormationEnemy(slot, layout, i, rowCount);

                    if (enemy != null)
                        spawned.Add(new EnemyFormationConfig { Enemy = enemy, RowIndex = rowIndex, ColumnIndex = i });
                }

                rowStart = rowEnd;
            }

            _enemyFormation = spawned.ToArray();
        }

        /// <summary>
        /// Spawns one enemy at its computed formation position: rows ahead of
        /// the hero along the battle site's forward axis, the row centered
        /// sideways (the hero's right is right on screen), plus the slot's
        /// optional offset. Returns null when the enemy or prefab is missing.
        /// </summary>
        private EnemyComponent SpawnFormationEnemy(LevelEnemySlot slot, FormationLayoutConfig layout, int orderInRow, int rowCount)
        {
            if (slot.Enemy == null || slot.Enemy.Prefab == null)
            {
                GameLogger.Error(LogSourceType.BattleSystem, $"Level '{CurrentLevel.DisplayName}': enemy or prefab missing at row {slot.RowIndex} slot {slot.SlotIndex}.");
                return null;
            }

            Vector3 heroPosition = _battleSite.HeroBattlePosition;
            Vector3 forward = _battleSite.Forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            float distance = layout.FrontRowDistance + slot.RowIndex * layout.RowSpacing;
            float sideways = (orderInRow - (rowCount - 1) * 0.5f) * layout.SlotSpacing;
            Vector3 offset = slot.PositionOffset;

            Vector3 position = heroPosition + forward * (distance - offset.z) + right * (sideways + offset.x) + Vector3.up * offset.y;
            Vector3 toHero = Vector3.ProjectOnPlane(heroPosition - position, Vector3.up);
            Quaternion rotation = toHero.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(toHero) : Quaternion.LookRotation(-forward);

            EnemyComponent enemy = Instantiate(slot.Enemy.Prefab, position, rotation);
            enemy.name = $"{slot.Enemy.DisplayName} [{slot.RowIndex},{orderInRow}]";
            enemy.SetStatsData(slot.Enemy.Stats);

            return enemy;
        }

        // PRIVATE METHODS — Initialization

        /// <summary>
        /// Creates the battle-scoped EventHub instance used for decoupled communication
        /// between battle systems, actors, and presentation components.
        /// </summary>
        private void CreateEventHub()
        {
            _eventHub = new EventHub();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"EventHub created. Hash={_eventHub.GetHashCode()}");
        }

        /// <summary>
        /// Creates the BattleFormation instance.
        /// Population happens in InitializeEnemies after components are initialized.
        /// Must run before InitializeHero so the reference is available for injection.
        /// </summary>
        private void CreateBattleFormation()
        {
            _battleFormation = new BattleFormation();
        }

        /// <summary>
        /// Initializes the hero component and its state machine,
        /// assigns a runtime entity identifier, and starts the hero FSM.
        /// </summary>
        private void InitializeHero()
        {
            if (_heroComponent == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeHero: HeroComponent reference is missing.");
                return;
            }

            if (_actionTierData == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeHero: ActionTierData reference is missing.");
                return;
            }

            HeroActionResolver heroActionResolver = new HeroActionResolver(_actionTierData);

            _heroStateMachine = new HeroStateMachine(
                _eventHub,
                _heroComponent,
                heroActionResolver);

            _heroComponent.Initialize(_heroStateMachine, _idGenerator.Generate());
            _heroComponent.StartStateMachine();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"Hero initialized. EntityId={_heroComponent.EntityId}");
        }

        /// <summary>
        /// Initializes all enemy components, builds formation entries,
        /// populates BattleFormation, and registers enemies into TurnContext.
        /// </summary>
        private void InitializeEnemies()
        {
            _turnContext = new TurnContext();

            if (_enemyFormation == null || _enemyFormation.Length == 0)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeEnemies: No enemies found in formation config.");
                return;
            }

            List<EnemyFormationEntry> formationEntries =
                new List<EnemyFormationEntry>(_enemyFormation.Length);

            foreach (EnemyFormationConfig config in _enemyFormation)
            {
                if (config.Enemy == null)
                    continue;

                EnemyActionResolver actionResolver = new EnemyActionResolver();

                EnemyStateMachine enemyStateMachine = new EnemyStateMachine(
                    _eventHub,
                    config.Enemy,
                    actionResolver,
                    _heroComponent.EntityId);

                config.Enemy.Initialize(enemyStateMachine, _idGenerator.Generate());
                config.Enemy.StartStateMachine();

                _enemyStateMachines.Add(enemyStateMachine);
                _turnContext.RegisterEnemy();

                formationEntries.Add(
                    new EnemyFormationEntry(config.Enemy, config.RowIndex, config.ColumnIndex));

                GameLogger.Log(LogSourceType.BattleSystem,
                    $"Enemy initialized. EntityId={config.Enemy.EntityId} Line={config.RowIndex} Slot={config.ColumnIndex}");
            }

            _battleFormation.Initialize(formationEntries);
        }

        /// <summary>Initializes hero target selection input.</summary>
        private void InitializeTargetInput()
        {
            if (_heroTargetInputController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeTargetInput skipped: HeroTargetInputController reference is missing.");
                return;
            }

            _heroTargetInputController.Initialize(_battleFormation, _heroComponent);
        }

        /// <summary>Initializes hero target facing presentation controller.</summary>
        private void InitializeHeroTargetFocus()
        {
            if (_heroTargetFocusController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeHeroTargetFocus skipped: HeroTargetFocusController reference is missing.");
                return;
            }

            if (_battleIntroController != null && _battleIntroConfig != null)
            {
                _heroTargetFocusController.NotifyIntroStarted();
                _battleIntroController.TauntStarted += _heroTargetFocusController.NotifyIntroWalkEnded;
            }

            _heroTargetFocusController.Initialize(_battleFormation, _eventHub);

            GameLogger.Log(LogSourceType.BattleSystem,
                "BattleBootstrapper.InitializeHeroTargetFocus completed.");
        }

        /// <summary>
        /// Initializes the battle intro controller with the config if assigned.
        /// When no config is assigned, the controller will skip the intro sequence
        /// immediately when Play is called from Start.
        /// </summary>
        private void InitializeBattleIntro()
        {
            if (_battleIntroController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeBattleIntro: no controller assigned, skipping.");
                return;
            }

            _battleIntroController.Initialize(_battleIntroConfig);

            GameLogger.Log(LogSourceType.BattleSystem,
                _battleIntroConfig != null
                    ? $"BattleIntro initialized."
                    : "BattleIntro initialized with no config — will skip on Play.");
        }

        /// <summary>
        /// Creates and initializes the turn flow state machine,
        /// then binds it to the TurnSystemController for scene-level orchestration.
        /// </summary>
        private void InitializeTurnSystem()
        {
            if (_turnSystemController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeTurnSystem: TurnSystemController reference is missing.");
                return;
            }

            _turnStateMachine = new TurnStateMachine(
                _eventHub,
                _turnContext,
                _battleFormation);

            _turnSystemController.Initialize(_turnStateMachine);

            GameLogger.Log(LogSourceType.BattleSystem,
                $"TurnSystem initialized. AliveEnemyCount={_turnContext.AliveEnemyCount}");
        }

        /// <summary>
        /// Creates the CombatSystem and wires hero-target resolution
        /// through the current battle formation state.
        /// </summary>
        private void InitializeCombatSystem()
        {
            if (_heroComponent == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeCombatSystem: HeroComponent reference is missing.");
                return;
            }

            _combatSystem = new CombatSystem(
                _eventHub,
                new CombatResolver(),
                _heroComponent,
                _battleFormation);

            GameLogger.Log(LogSourceType.BattleSystem, "CombatSystem initialized.");
        }

        /// <summary>
        /// Initializes the puzzle controller with the shared battle EventHub
        /// so puzzle results can publish gameplay events into the combat flow,
        /// and applies the level's board size and tile types. Runs in Awake,
        /// before PuzzleController.Start builds the grid.
        /// </summary>
        private void InitializePuzzleSystem()
        {
            if (_puzzleController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializePuzzleSystem: PuzzleController reference is missing.");
                return;
            }

            _puzzleController.Initialize(_eventHub);

            if (CurrentLevel != null && _puzzleController.Grid != null)
                _puzzleController.Grid.ConfigureBoard(CurrentLevel.BoardRows, CurrentLevel.BoardColumns, CurrentLevel.TileTypes);
        }

        /// <summary>
        /// Initializes scene-level HUD elements.
        /// Entity-owned HUD (health bars) is initialized by entity components themselves.
        /// </summary>
        private void InitializeBattleHud()
        {
            _battleUIPresenter.Initialize(_eventHub);
        }

        /// <summary>Initializes the victory UI controller with the battle-scoped EventHub.</summary>
        private void InitializeVictoryUI()
        {
            if (_victoryUIController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper: VictoryUIController reference is missing.");
                return;
            }

            _victoryUIController.Initialize(_eventHub);
        }

        /// <summary>Initializes the defeat UI controller with the battle-scoped EventHub.</summary>
        private void InitializeDefeatUI()
        {
            if (_defeatUIController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper: DefeatUIController reference is missing.");
                return;
            }

            _defeatUIController.Initialize(_eventHub);
        }

        /// <summary>Initializes the victory camera controller with the battle-scoped EventHub.</summary>
        private void InitializeVictoryCamera()
        {
            if (_victoryCameraController == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper: VictoryCameraController reference is missing.");
                return;
            }

            _victoryCameraController.Initialize(_eventHub);
        }

        /// <summary>Initializes the hero victory rotator with the battle-scoped EventHub.</summary>
        private void InitializeVictoryRotator()
        {
            if (_heroVictoryRotator == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper: HeroVictoryRotator reference is missing.");
                return;
            }

            _heroVictoryRotator.Initialize(_eventHub);
        }

        /// <summary>
        /// Creates the battle timer and its runner if a config is assigned.
        /// When no config is assigned, no timer system is instantiated and the
        /// view is hidden if present. The timer is created in idle state and
        /// will start on the first puzzle match.
        /// </summary>
        private void InitializeBattleTimer()
        {
            if (_battleTimerConfig == null)
            {
                if (_battleTimerView != null)
                    _battleTimerView.Hide();

                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeBattleTimer: no config assigned, timer disabled.");
                return;
            }

            _battleTimer = new BattleTimer(_eventHub, _battleTimerConfig);

            GameObject runnerObject = new GameObject("[BattleTimerRunner]");
            runnerObject.transform.SetParent(transform, worldPositionStays: false);
            _battleTimerRunner = runnerObject.AddComponent<BattleTimerRunner>();
            _battleTimerRunner.Bind(_battleTimer);

            if (_battleTimerView != null)
            {
                _battleTimerPresenter = new BattleTimerPresenter(
                    _eventHub,
                    _battleTimerView,
                    _battleTimer.RemainingSeconds);
            }
            else
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeBattleTimer: config present but view reference is missing.");
            }

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer initialized. Duration={_battleTimerConfig.DurationSeconds}");
        }

        /// <summary>
        /// Creates the BattleFlowController which owns entity-death and timer-expiry
        /// orchestration. Must be created after TurnStateMachine, BattleFormation,
        /// BattleTimer (if any) and HeroComponent are all wired.
        /// </summary>
        private void InitializeBattleFlowController()
        {
            _battleFlowController = new BattleFlowController(
                _eventHub,
                _turnContext,
                _turnStateMachine,
                _battleFormation,
                _heroComponent,
                _battleTimer);

            GameLogger.Log(LogSourceType.BattleSystem, "BattleFlowController initialized.");
        }

        /// <summary>
        /// Initializes the floating text service and presenter. Both prefabs,
        /// the screen parent and the config must be assigned for this to
        /// activate; if any is missing, floating text is disabled for this battle.
        /// </summary>
        private void InitializeFloatingText()
        {
            if (_floatingTextConfig == null || _floatingTextAnchoredPrefab == null || _floatingTextScreenPrefab == null || _floatingTextScreenParent == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeFloatingText: missing config, prefab or parent, skipping.");
                return;
            }

            _floatingTextService = new FloatingTextService(_floatingTextConfig, _floatingTextAnchoredPrefab, _floatingTextScreenPrefab, _floatingTextScreenParent);

            var anchorProvider = new BattleFormationAnchorProvider(_heroComponent, _battleFormation, _floatingTextHeightOffset);

            _floatingTextPresenter = new FloatingTextPresenter(_eventHub, _floatingTextService, anchorProvider);

            GameLogger.Log(LogSourceType.BattleSystem, "FloatingText initialized.");
        }

        /// <summary>
        /// Creates the presenter that keeps the target marker on the selected enemy.
        /// Skipped when no marker view is assigned. Must run after InitializeEnemies.
        /// </summary>
        private void InitializeTargetMarker()
        {
            if (_targetMarkerView == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeTargetMarker: no marker view assigned, skipping.");
                return;
            }

            _targetMarkerPresenter = new TargetMarkerPresenter(_eventHub, _battleFormation, _targetMarkerView);
        }

        /// <summary>
        /// Puts the hero into its battle-ready sword pose. Runs when the intro draws
        /// the sword, or at battle start when no intro controller is assigned.
        /// </summary>
        private void EnterHeroBattleStance()
        {
            if (_heroComponent != null)
                _heroComponent.EnterBattleStance();

            if (_heroTargetFocusController != null)
                _heroTargetFocusController.NotifyIntroEnded();
        }

        /// <summary>
        /// Creates the ultimate gauge presenter for the hero's HUD bar (skipped
        /// when the hero has no gauge view; the presenter hides the view itself
        /// when the hero has no ultimate) and the affinity tile highlight
        /// presenter (skipped when the hero has no ultimate or there is no grid).
        /// Must run after InitializeHero.
        /// </summary>
        private void InitializeUltimateGauge()
        {
            if (_heroComponent == null)
                return;

            if (_heroComponent.UltimateGauge != null && _puzzleController != null && _puzzleController.Grid != null)
                _ultimateTileHighlightPresenter = new UltimateTileHighlightPresenter(_eventHub, _puzzleController.Grid, _heroComponent.EntityId, _heroComponent.UltimateGauge);

            if (_heroComponent.UltimateGaugeView == null)
                return;

            _ultimateGaugePresenter = new UltimateGaugePresenter(_eventHub, _heroComponent.UltimateGaugeView, _heroComponent.EntityId, _heroComponent.UltimateGauge);
        }

        /// <summary>
        /// Creates the status icon presenter and registers the hero and every
        /// formation enemy. Entities without a StatusIconBarView are skipped.
        /// Must run after InitializeHero and InitializeEnemies so entity ids exist.
        /// </summary>
        private void InitializeStatusIcons()
        {
            _statusIconPresenter = new StatusIconPresenter(_eventHub);

            if (_heroComponent != null)
                _statusIconPresenter.RegisterView(_heroComponent.EntityId, _heroComponent.StatusIconBarView);

            if (_enemyFormation == null)
                return;

            foreach (EnemyFormationConfig config in _enemyFormation)
            {
                if (config.Enemy != null)
                    _statusIconPresenter.RegisterView(config.Enemy.EntityId, config.Enemy.StatusIconBarView);
            }
        }

        /// <summary>
        /// Creates the combat feedback presenter (camera shake, hit VFX).
        /// Requires the data asset and the hero; shaker and VFX player are each
        /// optional and simply disable their part of the feedback when missing.
        /// Must run after InitializeHero and InitializeEnemies so entity ids exist.
        /// </summary>
        private void InitializeCombatFeedback()
        {
            if (_combatFeedbackData == null || _heroComponent == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.InitializeCombatFeedback: missing data or hero, skipping.");
                return;
            }

            var entityLocator = new BattleEntityLocator(_heroComponent, _enemyFormation);

            _combatFeedbackPresenter = new CombatFeedbackPresenter(_eventHub, _combatFeedbackData, _cameraShaker, _hitVfxPlayer, entityLocator, _heroComponent.EntityId);

            if (_killCamController != null)
            {
                _killCamController.Initialize(_eventHub, _combatFeedbackData, entityLocator);
                _killCamController.SetDebugPreviewSource(() => _heroComponent.EntityId, () => _battleFormation.CurrentTarget != null ? _battleFormation.CurrentTarget.EntityId : -1);
            }

            if (_ultimateCameraController != null)
                _ultimateCameraController.Initialize(_eventHub);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int tier3MatchCount = _actionTierData != null ? _actionTierData.Tier3MinMatch : 9;
            gameObject.AddComponent<MightyHeroes.Gameplay.Battle.DevTools.BattleDebugHotkeys>().Initialize(_eventHub, _heroComponent, _combatFeedbackPresenter, tier3MatchCount);
#endif
        }

        // PRIVATE METHODS — Disposal

        /// <summary>Disposes the BattleFlowController to unregister its event handlers.</summary>
        private void DisposeBattleFlowController()
        {
            if (_battleFlowController == null)
                return;

            _battleFlowController.Dispose();
            _battleFlowController = null;

            GameLogger.Log(LogSourceType.BattleSystem, "BattleFlowController disposed.");
        }

        /// <summary>
        /// Disposes the battle timer, its presenter, and destroys the runner GameObject.
        /// Safe to call when no timer was created.
        /// </summary>
        private void DisposeBattleTimer()
        {
            _battleTimerPresenter?.Dispose();
            _battleTimerPresenter = null;

            _battleTimer?.Dispose();
            _battleTimer = null;

            if (_battleTimerRunner != null)
            {
                Destroy(_battleTimerRunner.gameObject);
                _battleTimerRunner = null;
            }

            GameLogger.Log(LogSourceType.BattleSystem, "BattleTimer disposed.");
        }

        /// <summary>Disposes the CombatSystem and clears its runtime reference.</summary>
        private void DisposeCombatSystem()
        {
            if (_combatSystem == null)
                return;

            _combatSystem.Dispose();
            _combatSystem = null;

            GameLogger.Log(LogSourceType.BattleSystem, "CombatSystem disposed.");
        }

        /// <summary>Disposes all created enemy state machines and clears the runtime collection.</summary>
        private void DisposeEnemyStateMachines()
        {
            foreach (EnemyStateMachine esm in _enemyStateMachines)
                esm?.Dispose();

            _enemyStateMachines.Clear();

            GameLogger.Log(LogSourceType.BattleSystem, "All enemy state machines disposed.");
        }

        /// <summary>Disposes the hero state machine and clears its runtime reference.</summary>
        private void DisposeHeroStateMachine()
        {
            if (_heroStateMachine == null)
                return;

            _heroStateMachine.Dispose();
            _heroStateMachine = null;

            GameLogger.Log(LogSourceType.BattleSystem, "HeroStateMachine disposed.");
        }

        /// <summary>
        /// Disposes HeroComponent to unregister its EventHub subscriptions.
        /// Must be called after DisposeHeroStateMachine so the FSM is already
        /// torn down before the component cleans up its own listeners.
        /// </summary>
        private void DisposeHeroComponent()
        {
            if (_heroComponent == null)
                return;

            _heroComponent.Dispose();

            GameLogger.Log(LogSourceType.BattleSystem, "HeroComponent disposed.");
        }

        /// <summary>
        /// Disposes all EnemyComponent instances to unregister their EventHub subscriptions.
        /// Must be called after DisposeEnemyStateMachines so FSMs are torn down first.
        /// </summary>
        private void DisposeEnemyComponents()
        {
            if (_enemyFormation == null)
                return;

            foreach (EnemyFormationConfig config in _enemyFormation)
            {
                if (config.Enemy != null)
                    config.Enemy.Dispose();
            }

            GameLogger.Log(LogSourceType.BattleSystem, "All enemy components disposed.");
        }

        private void DisposeFloatingText()
        {
            _floatingTextPresenter?.Dispose();
            _floatingTextPresenter = null;
            _floatingTextService = null;
        }

        /// <summary>Disposes the target marker presenter to unregister its event handlers.</summary>
        private void DisposeTargetMarker()
        {
            _targetMarkerPresenter?.Dispose();
            _targetMarkerPresenter = null;
        }

        /// <summary>Disposes the combat feedback presenter to unregister its event handlers.</summary>
        private void DisposeCombatFeedback()
        {
            _combatFeedbackPresenter?.Dispose();
            _combatFeedbackPresenter = null;
        }

        /// <summary>Disposes the ultimate gauge and tile highlight presenters to unregister their event handlers.</summary>
        private void DisposeUltimateGauge()
        {
            _ultimateGaugePresenter?.Dispose();
            _ultimateGaugePresenter = null;

            _ultimateTileHighlightPresenter?.Dispose();
            _ultimateTileHighlightPresenter = null;
        }

        /// <summary>Disposes the status icon presenter to unregister its event handlers.</summary>
        private void DisposeStatusIcons()
        {
            _statusIconPresenter?.Dispose();
            _statusIconPresenter = null;
        }
    }
}