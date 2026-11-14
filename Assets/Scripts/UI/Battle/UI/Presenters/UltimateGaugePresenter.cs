using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Ultimate;
using MightyHeroes.Gameplay.Ultimate.Payloads;
using MightyHeroes.UI.Battle.Views;

namespace MightyHeroes.UI.Battle.Presenters
{
    /// <summary>
    /// Bridges the hero's UltimateGauge to UltimateGaugeView.
    ///
    /// Shows the gauge with its current value on construction and updates it on
    /// every UltimateGaugeChangedEvent for the hero. When the hero has no
    /// ultimate (no UltimateData), the view is hidden and nothing is subscribed.
    ///
    /// Constructed and disposed by BattleBootstrapper, scoped to one battle.
    /// </summary>
    public sealed class UltimateGaugePresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly UltimateGaugeView _view;
        private readonly int _heroEntityId;
        private readonly bool _isSubscribed;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter and shows the initial gauge state.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="view">Gauge view on the HUD.</param>
        /// <param name="heroEntityId">Hero runtime id; events of other entities are ignored.</param>
        /// <param name="gauge">Hero ultimate gauge, or null when the hero has no ultimate.</param>
        public UltimateGaugePresenter(EventHub eventHub, UltimateGaugeView view, int heroEntityId, UltimateGauge gauge)
        {
            _eventHub = eventHub;
            _view = view;
            _heroEntityId = heroEntityId;

            if (gauge == null)
            {
                _view.SetVisible(false);
                return;
            }

            _view.SetValue(gauge.CurrentValue, gauge.Capacity, animate: false);
            _view.SetVisible(true);

            _eventHub.HeroEvents.UltimateGaugeChangedEvent.Register(OnUltimateGaugeChanged);
            _isSubscribed = true;
        }

        // PUBLIC METHODS

        /// <summary>Unregisters event listeners. Must be called when the battle ends.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_isSubscribed)
                _eventHub.HeroEvents.UltimateGaugeChangedEvent.Unregister(OnUltimateGaugeChanged);

            _isDisposed = true;
        }

        // EVENT HANDLERS

        private void OnUltimateGaugeChanged(UltimateGaugeChangedPayload payload)
        {
            if (payload.EntityId != _heroEntityId)
                return;

            _view.SetValue(payload.CurrentValue, payload.Capacity, animate: true);
        }
    }
}
