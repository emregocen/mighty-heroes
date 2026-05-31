using System.Collections.Generic;
using MightyHeroes.Battle.BattleTimer;
using MightyHeroes.Battle.TurnFlow;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat;
using MightyHeroes.Gameplay.Enemy.AI;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Hero.Input;
using MightyHeroes.Gameplay.Hero.Presentation;
using MightyHeroes.Gameplay.Hero.StateMachine;
using MightyHeroes.Gameplay.Puzzle;
using MightyHeroes.UI.Battle;
using MightyHeroes.UI.Battle.Presenters;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;

namespace MightyHeroes.Core.Battle
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

        [Header("Enemy Formation")]
        [SerializeField] private EnemyFormationConfig[] _enemyFormation;

        [Header("Battle Timer")]
        [Tooltip("Optional. When empty, no timer is created for this battle.")]
        [SerializeField] private BattleTimerConfig _battleTimerConfig;

        [Tooltip("UI view that renders the remaining seconds. Required only if a timer config is set.")]
        [SerializeField] private BattleTimerView _battleTimerView;

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

        // UNITY CALLBACKS

        private void Awake()
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleBootstrapper.Awake InstanceId={GetEntityId()}");

            _idGenerator = new BattleIdGenerator();

            CreateEventHub();
            CreateBattleFormation();
            InitializeHero();
            InitializeEnemies();
            InitializeTargetInput();
            InitializeHeroTargetFocus();
            InitializeTurnSystem();
            InitializeCombatSystem();
            InitializePuzzleSystem();
            InitializeBattleHud();
            InitializeVictoryUI();
            InitializeDefeatUI();
            InitializeVictoryCamera();
            InitializeVictoryRotator();
            InitializeBattleTimer();
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
        }

        private void OnDestroy()
        {
            DisposeBattleFlowController();
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

            HeroActionResolver heroActionResolver = new HeroActionResolver();

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

            _heroTargetFocusController.Initialize(_battleFormation, _eventHub);

            GameLogger.Log(LogSourceType.BattleSystem,
                "BattleBootstrapper.InitializeHeroTargetFocus completed.");
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
        /// so puzzle results can publish gameplay events into the combat flow.
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
    }
}