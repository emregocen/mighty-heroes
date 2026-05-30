using MightyHeroes.Battle.BattleTimer.Payloads;
using MightyHeroes.Core.EventSystem;

namespace MightyHeroes.Battle.BattleTimer.Events
{
    /// <summary>
    /// Provides access to all battle-timer-related events.
    /// Used to signal timer lifecycle milestones (start, tick, stop, expiry)
    /// to UI presenters and battle flow control.
    /// </summary>
    public sealed class BattleTimerEvents : EventGroupBase
    {
        /// <summary>
        /// Creates the battle timer events group using the shared event registry.
        /// </summary>
        /// <param name="registry">Shared event registry instance.</param>
        public BattleTimerEvents(IEventRegistry registry)
            : base(registry)
        {
        }

        /// <summary>
        /// Raised once when the battle timer transitions from idle to running.
        /// Consumed by BattleTimerPresenter to display the initial countdown value.
        /// </summary>
        public GameEvent<BattleTimerStartedPayload> StartedEvent =>
            Registry.GetEvent<BattleTimerStartedPayload>();

        /// <summary>
        /// Raised whenever the displayed remaining seconds (floor value) change.
        /// Not raised every frame — only when the integer second changes.
        /// </summary>
        public GameEvent<BattleTimerRemainingChangedPayload> RemainingSecondsChangedEvent =>
            Registry.GetEvent<BattleTimerRemainingChangedPayload>();

        /// <summary>
        /// Raised when the timer is stopped externally before reaching zero
        /// (typically because the battle ended in victory or hero defeat).
        /// </summary>
        public GameEvent<BattleTimerStoppedPayload> StoppedEvent =>
            Registry.GetEvent<BattleTimerStoppedPayload>();

        /// <summary>
        /// Raised exactly once when the timer reaches zero with the battle still active.
        /// Consumed by BattleFlowController to trigger the defeat flow.
        /// </summary>
        public GameEvent<BattleTimerExpiredPayload> ExpiredEvent =>
            Registry.GetEvent<BattleTimerExpiredPayload>();
    }
}