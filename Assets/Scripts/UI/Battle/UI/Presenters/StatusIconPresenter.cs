using System;
using System.Collections.Generic;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.UI.Battle.Views;

namespace MightyHeroes.UI.Battle.Presenters
{
    /// <summary>
    /// Bridges combat modifier events to per-entity StatusIconBarView instances.
    ///
    /// Entity components only expose their view; they never refresh it. This
    /// presenter owns the entityId to view mapping and is the single place that
    /// decides which icon is visible, keeping presentation out of gameplay code.
    ///
    /// Shield is intentionally not an icon: the shield bar is always visible for
    /// entities with shield capacity, so an icon would duplicate it.
    ///
    /// Constructed and disposed by BattleBootstrapper, scoped to a single battle.
    /// </summary>
    public sealed class StatusIconPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly Dictionary<int, StatusIconBarView> _views = new Dictionary<int, StatusIconBarView>();
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter and subscribes to modifier and death events.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        public StatusIconPresenter(EventHub eventHub)
        {
            _eventHub = eventHub;

            _eventHub.CombatEvents.AttackBuffChangedEvent.Register(OnAttackBuffChanged);
            _eventHub.HealthEvents.EntityDiedEvent.Register(OnEntityDied);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Registers an entity's status icon view. Null views are ignored so
        /// entities without a status bar need no special handling.
        /// </summary>
        /// <param name="entityId">Runtime entity id.</param>
        /// <param name="view">Status icon view owned by the entity, or null.</param>
        public void RegisterView(int entityId, StatusIconBarView view)
        {
            if (view == null)
                return;

            _views[entityId] = view;
            view.ClearAll();
        }

        /// <summary>
        /// Unregisters all event listeners. Must be called when the battle ends.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.CombatEvents.AttackBuffChangedEvent.Unregister(OnAttackBuffChanged);
            _eventHub.HealthEvents.EntityDiedEvent.Unregister(OnEntityDied);
            _views.Clear();
            _isDisposed = true;
        }

        // EVENT HANDLERS

        private void OnAttackBuffChanged(AttackBuffChangedPayload payload)
        {
            if (_views.TryGetValue(payload.EntityId, out StatusIconBarView view))
                view.SetAttackBuff(payload.CurrentPercent);
        }

        private void OnEntityDied(EntityDiedPayload payload)
        {
            if (_views.TryGetValue(payload.EntityId, out StatusIconBarView view))
                view.ClearAll();
        }
    }
}
