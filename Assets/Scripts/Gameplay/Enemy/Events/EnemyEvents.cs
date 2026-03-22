using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Enemy.Payloads;

namespace MightyHeroes.Gameplay.Enemy.Events
{
    /// <summary>
    /// Provides access to all enemy-related gameplay events.
    /// Used to signal enemy lifecycle and reaction milestones to other systems.
    /// </summary>
    public sealed class EnemyEvents : EventGroupBase
    {
        /// <summary>
        /// Creates the enemy events group using the shared event registry.
        /// </summary>
        /// <param name="registry">Shared event registry.</param>
        public EnemyEvents(IEventRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Raised at the hit frame of an enemy attack animation.
        /// </summary>
        public GameEvent<EnemyAttackHitPayload> EnemyAttackHitEvent =>
            Registry.GetEvent<EnemyAttackHitPayload>();

        /// <summary>
        /// Raised when a single enemy completes its action phase.
        /// </summary>
        public GameEvent<EnemyTurnEndedPayload> EnemyTurnEndedEvent =>
            Registry.GetEvent<EnemyTurnEndedPayload>();

        /// <summary>
        /// Raised when an enemy hit reaction animation
        /// has fully completed.
        ///
        /// This event is critical for synchronizing
        /// hero combo flow with enemy reactions.
        /// </summary>
        public GameEvent<EnemyHitReactionEndedPayload> EnemyHitReactionEndedEvent =>
            Registry.GetEvent<EnemyHitReactionEndedPayload>();
    }
}