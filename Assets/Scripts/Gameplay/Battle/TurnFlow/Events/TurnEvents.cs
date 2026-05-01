using MightyHeroes.Core.EventSystem;
using MightyHeroes.Battle.TurnFlow.Payloads;

namespace MightyHeroes.Battle.TurnFlow.Events
{
    /// <summary>
    /// Provides access to all turn-related game events.
    /// Acts as a semantic wrapper over the global event registry.
    /// </summary>
    public sealed class TurnEvents : EventGroupBase
    {
        // 3) CONSTRUCTOR

        /// <summary>
        /// Creates the turn events group using the shared event registry.
        /// </summary>
        public TurnEvents(IEventRegistry registry) : base(registry)
        {
        }

        // 2) PROPERTIES

        /// <summary>
        /// Raised when a new turn cycle starts.
        /// </summary>
        public GameEvent<TurnStartedPayload> TurnStartedEvent =>
            Registry.GetEvent<TurnStartedPayload>();

        /// <summary>
        /// Raised when the hero phase of the turn begins.
        /// </summary>
        public GameEvent<TurnHeroPhaseStartedPayload> TurnHeroPhaseStartedEvent =>
            Registry.GetEvent<TurnHeroPhaseStartedPayload>();

        /// <summary>
        /// Raised when the companion phase of the turn begins.
        /// </summary>
        public GameEvent<TurnCompanionPhaseStartedPayload> TurnCompanionPhaseStartedEvent =>
            Registry.GetEvent<TurnCompanionPhaseStartedPayload>();

        /// <summary>
        /// Raised when the enemy phase of the turn begins.
        /// </summary>
        public GameEvent<TurnEnemyPhaseStartedPayload> TurnEnemyPhaseStartedEvent =>
            Registry.GetEvent<TurnEnemyPhaseStartedPayload>();

        /// <summary>
        /// Raised when the a single enemy phase of the turn begins.
        /// </summary>
        public GameEvent<TurnEnemyStartedPayload> TurnEnemyStartedEvent =>
            Registry.GetEvent<TurnEnemyStartedPayload>();

        /// <summary>
        /// Raised when the entire turn flow has completed.
        /// </summary>
        public GameEvent<TurnEndedPayload> TurnEndedEvent =>
            Registry.GetEvent<TurnEndedPayload>();

        /// <summary>
        /// Raised when all enemies are defeated and the battle ends in victory.
        /// Consumed by UI, reward, and scene transition systems.
        /// </summary>
        public GameEvent<TurnBattleVictoryPayload> TurnBattleVictoryEvent =>
            Registry.GetEvent<TurnBattleVictoryPayload>();

    }
}