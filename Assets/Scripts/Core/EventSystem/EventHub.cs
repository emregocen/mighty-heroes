using MightyHeroes.Battle.BattleTimer.Events;
using MightyHeroes.Battle.TurnFlow.Events;
using MightyHeroes.Gameplay.Combat.Events;
using MightyHeroes.Gameplay.Enemy.Events;
using MightyHeroes.Gameplay.Health.Events;
using MightyHeroes.Gameplay.Hero.Events;
using MightyHeroes.Gameplay.Puzzle.Events;

namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Concrete implementation of the global event hub.
    /// Owns the event registry and exposes strongly-typed event groups.
    /// Acts as the single composition point for all game-wide events.
    /// </summary>
    public sealed class EventHub : IEventHub
    {
        /// <summary>
        /// Internal registry that stores and caches all game events.
        /// </summary>
        private readonly EventRegistry _registry;

        /// <summary>
        /// Provides access to all turn-related events.
        /// Used by the turn state machine to publish turn phase changes.
        /// </summary>
        public TurnEvents TurnEvents { get; }

        /// <summary>
        /// Provides access to all puzzle-related events.
        /// Used to publish and listen to puzzle interaction results,
        /// such as tile matches and puzzle resolution outcomes.
        /// </summary>
        public PuzzleEvents PuzzleEvents { get; }

        /// <summary>
        /// Provides access to all hero-related gameplay events.
        /// Used for hero lifecycle signals such as turn completion.
        /// </summary>
        public HeroEvents HeroEvents { get; }

        /// <summary>
        /// Provides access to all enemy-related gameplay events.
        /// Used for enemy lifecycle signals such as taking damage.
        /// </summary>
        public EnemyEvents EnemyEvents { get; }

        /// <summary>
        /// Provides access to all health-related gameplay events.
        /// Used for health/shield changes, damage application, death and revive signals.
        /// </summary>
        public HealthEvents HealthEvents { get; }

        /// <summary>
        /// Provides access to all combat-resolution-related events.
        /// Used for communicating combat hit and damage resolution outcomes.
        /// </summary>
        public CombatEvents CombatEvents { get; }

        /// <summary>
        /// Provides access to all battle-timer-related events.
        /// Used to publish timer start, tick, stop and expiry to UI and flow control.
        /// </summary>
        public BattleTimerEvents BattleTimerEvents { get; }

        /// <summary>
        /// Creates the global event hub and initializes all event groups.
        /// </summary>
        public EventHub()
        {
            _registry = new EventRegistry();

            // Core flow events
            TurnEvents = new TurnEvents(_registry);

            // Puzzle interaction events
            PuzzleEvents = new PuzzleEvents(_registry);

            // Hero lifecycle and gameplay events
            HeroEvents = new HeroEvents(_registry);

            // Enemy lifecycle and gameplay events
            EnemyEvents = new EnemyEvents(_registry);

            // Health system events
            HealthEvents = new HealthEvents(_registry);

            // Combat resolution events
            CombatEvents = new CombatEvents(_registry);

            // Battle timer events
            BattleTimerEvents = new BattleTimerEvents(_registry);
        }

        /// <summary>
        /// Returns the global game event instance associated
        /// with the given payload type.
        /// This method is rarely used directly; event groups
        /// should be preferred for semantic clarity.
        /// </summary>
        public GameEvent<TPayload> GetEvent<TPayload>()
        {
            return _registry.GetEvent<TPayload>();
        }
    }
}