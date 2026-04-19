using System.Collections.Generic;
using UnityEngine;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Battle.TurnFlow;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat;
using MightyHeroes.Gameplay.Enemy.AI;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Hero.StateMachine;
using MightyHeroes.Gameplay.Puzzle;
using MightyHeroes.Core.BattleSystem;
using MightyHeroes.UI.Battle.Presenters;

namespace MightyHeroes.Core.Battle
{
    /// <summary>
    /// Composition root for all battle-scoped systems.
    ///
    /// Responsibilities:
    /// - Creates and wires all battle-lifetime services.
    /// - Initializes BattleFormation with formation layout from Inspector config.
    /// - Listens to EntityDiedEvent to keep TurnContext and BattleFormation in sync.
    /// - Disposes all systems on scene destroy to prevent memory leaks.
    ///
    /// Initialization order (Awake):
    /// 1. EventHub       — must exist before any subscriber
    /// 2. BattleFormation — created before hero so HeroStateMachine can receive reference
    /// 3. Hero           — requires BattleFormation for IsTargetAlive check
    /// 4. Enemies        — initialized and placed into formation
    /// 5. TurnSystem     — needs IFormation from BattleFormation
    /// 6. CombatSystem   — needs IBattleField from BattleFormation
    /// 7. PuzzleSystem   — receives EventHub reference
    /// 8. Death listener — registered last to avoid callbacks during setup
    /// </summary>
    public sealed class BattleBootstrapper : MonoBehaviour
    {
        // FIELDS — Inspector

        [SerializeField] private TurnSystemController _turnSystemController;
        [SerializeField] private PuzzleController _puzzleController;
        [SerializeField] private BattleUIPresenter _battleUIPresenter;
        [SerializeField] private HeroComponent _heroComponent;
        [SerializeField] private EnemyFormationConfig[] _enemyFormation;

        // FIELDS — Runtime

        private EventHub _eventHub;
        private TurnContext _turnContext;
        private TurnStateMachine _turnStateMachine;
        private HeroStateMachine _heroStateMachine;
        private BattleFormation _battleFormation;
        private CombatSystem _combatSystem;
        private BattleIdGenerator _idGenerator;

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
            InitializeTurnSystem();
            InitializeCombatSystem();
            InitializePuzzleSystem();
            InitializeBattleHud();
            RegisterEntityDeathListener();
        }

        private void Start()
        {
            if (_turnStateMachine == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleBootstrapper.Start aborted: TurnStateMachine is null.");
                return;
            }

            _turnStateMachine.Start();
        }

        private void OnDestroy()
        {
            UnregisterEntityDeathListener();
            DisposeCombatSystem();
            DisposeEnemyComponents();
            DisposeEnemyStateMachines();
            DisposeHeroStateMachine();
            DisposeHeroComponent();

            GameLogger.Log(LogSourceType.BattleSystem, "BattleBootstrapper.OnDestroy complete.");
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

            // Initialize binds the state machine, assigns EntityId, sets up health and event subscriptions.
            // StartStateMachine is called separately so the bootstrapper owns the full boot sequence
            // for all entity types uniformly: create FSM → Initialize → StartStateMachine.
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

                // Initialize binds the state machine, assigns EntityId, sets up health and event subscriptions.
                // StartStateMachine is called separately — same boot pattern as hero.
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

            // BattleFormation implements IFormation — TurnEnemyState uses Lines
            // for front-to-back ordered enemy sequencing.
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

            // BattleFormation implements IBattleField — CombatSystem uses
            // GetCurrentTarget for hero attack resolution and FindEnemyById
            // for enemy attack routing.
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

        /// <summary>
        /// Registers the battle-level death listener so entity death events
        /// can update turn flow and battle completion state.
        /// </summary>
        private void RegisterEntityDeathListener()
        {
            _eventHub.HealthEvents.EntityDiedEvent.Register(OnEntityDied);
        }

        // PRIVATE METHODS — Dispose

        /// <summary>
        /// Unregisters the battle-level death listener during teardown
        /// to prevent callbacks after destruction.
        /// </summary>
        private void UnregisterEntityDeathListener()
        {
            _eventHub?.HealthEvents.EntityDiedEvent.Unregister(OnEntityDied);
        }

        /// <summary>
        /// Disposes the CombatSystem and clears its runtime reference.
        /// </summary>
        private void DisposeCombatSystem()
        {
            if (_combatSystem == null)
                return;

            _combatSystem.Dispose();
            _combatSystem = null;

            GameLogger.Log(LogSourceType.BattleSystem, "CombatSystem disposed.");
        }

        /// <summary>
        /// Disposes all created enemy state machines and clears the runtime collection.
        /// </summary>
        private void DisposeEnemyStateMachines()
        {
            foreach (EnemyStateMachine esm in _enemyStateMachines)
                esm?.Dispose();

            _enemyStateMachines.Clear();

            GameLogger.Log(LogSourceType.BattleSystem, "All enemy state machines disposed.");
        }

        /// <summary>
        /// Disposes the hero state machine and clears its runtime reference.
        /// </summary>
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

        // EVENT HANDLERS

        /// <summary>
        /// Called when any entity dies during battle.
        ///
        /// Enemy death:
        /// 1. Notify BattleFormation — reselects target if the dead enemy was selected.
        /// 2. Decrement TurnContext alive count.
        /// 3. End battle if no alive enemies remain.
        ///
        /// Hero death:
        /// End battle immediately.
        /// </summary>
        private void OnEntityDied(EntityDiedPayload payload)
        {
            if (_turnContext == null)
                return;

            if (_heroComponent != null && payload.EntityId == _heroComponent.EntityId)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    $"Hero died. EntityId={payload.EntityId}. Ending battle.");

                _turnContext.EndBattle();
                return;
            }

            bool isEnemy = false;

            foreach (EnemyFormationConfig config in _enemyFormation)
            {
                if (config.Enemy != null && config.Enemy.EntityId == payload.EntityId)
                {
                    isEnemy = true;
                    break;
                }
            }

            if (!isEnemy)
                return;

            // Notify formation before updating turn context
            // so target reselection happens while alive count is still accurate.
            _battleFormation.NotifyEnemyDefeated(payload.EntityId);
            _turnContext.UnregisterEnemy();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"Enemy died. EntityId={payload.EntityId} AliveCount={_turnContext.AliveEnemyCount}");

            if (_turnContext.AliveEnemyCount <= 0)
            {
                GameLogger.Log(LogSourceType.BattleSystem, "All enemies defeated. Ending battle.");
                _turnContext.EndBattle();
            }
        }
    }
}