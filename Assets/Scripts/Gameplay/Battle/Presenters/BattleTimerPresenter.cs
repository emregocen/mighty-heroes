using MightyHeroes.Battle.BattleTimer;
using MightyHeroes.Battle.BattleTimer.Payloads;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.UI.Battle.Views;
using System;

namespace MightyHeroes.UI.Battle.Presenters
{
    /// <summary>
    /// Bridges BattleTimer events to BattleTimerView.
    ///
    /// On construction the view is shown with the timer's full duration so the player
    /// sees the countdown value before the timer actually starts ticking. The view
    /// stays visible through pause states and only hides when the battle ends.
    ///
    /// Listens to Started, RemainingSecondsChanged, Stopped, and Expired.
    /// Owns no state of its own.
    /// </summary>
    public sealed class BattleTimerPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly BattleTimerView _view;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Constructs the presenter, displays the timer's initial value, and
        /// registers all event handlers.
        /// </summary>
        public BattleTimerPresenter(EventHub eventHub, BattleTimerView view, int initialSeconds)
        {
            _eventHub = eventHub;
            _view = view;

            _view.Show(initialSeconds);

            _eventHub.BattleTimerEvents.StartedEvent.Register(OnStarted);
            _eventHub.BattleTimerEvents.RemainingSecondsChangedEvent.Register(OnRemainingChanged);
            _eventHub.BattleTimerEvents.StoppedEvent.Register(OnStopped);
            _eventHub.BattleTimerEvents.ExpiredEvent.Register(OnExpired);
        }

        // PUBLIC METHODS

        /// <summary>Unregisters all event handlers. Safe to call multiple times.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.BattleTimerEvents.StartedEvent.Unregister(OnStarted);
            _eventHub.BattleTimerEvents.RemainingSecondsChangedEvent.Unregister(OnRemainingChanged);
            _eventHub.BattleTimerEvents.StoppedEvent.Unregister(OnStopped);
            _eventHub.BattleTimerEvents.ExpiredEvent.Unregister(OnExpired);

            _isDisposed = true;
        }

        // EVENT HANDLERS

        private void OnStarted(BattleTimerStartedPayload payload)
        {
            _view.SetRemainingSeconds(payload.RemainingSeconds);
        }

        private void OnRemainingChanged(BattleTimerRemainingChangedPayload payload)
        {
            _view.SetRemainingSeconds(payload.RemainingSeconds);
            _view.SetWarningActive(payload.IsInWarningZone);
        }

        private void OnStopped(BattleTimerStoppedPayload payload)
        {
            _view.Hide();
        }

        private void OnExpired(BattleTimerExpiredPayload payload)
        {
            _view.SetRemainingSeconds(0);
        }
    }
}