using System;
using System.Collections.Generic;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Health.Types;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Bridges combat/health events to FloatingTextService calls.
    ///
    /// Damage and death are correlated so Overkill can be computed: when an
    /// entity dies, this presenter checks whether the last hit's health-applied
    /// damage exceeded OverkillHealthMultiplier times the health the entity had
    /// immediately before that hit. K.O. and Overkill can both show for the
    /// same death.
    ///
    /// Requires a way to resolve an entity's world position for spawning text
    /// above it — done via IFloatingTextAnchorProvider so this class does not
    /// depend on HeroComponent/EnemyComponent directly.
    /// </summary>
    public sealed class FloatingTextPresenter : IDisposable
    {
        // CONSTANTS

        private const float OverkillHealthMultiplier = 2f;

        // FIELDS

        private readonly EventHub _eventHub;
        private readonly FloatingTextService _service;
        private readonly IFloatingTextAnchorProvider _anchorProvider;

        // Tracks the health an entity had immediately before its most recent
        // damage-applied hit, and how much health that hit removed, so
        // OnEntityDied can determine whether that specific hit was overkill.
        private readonly Dictionary<int, int> _healthBeforeLastHit = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _lastHealthDamage = new Dictionary<int, int>();

        private bool _isDisposed;

        // CONSTRUCTOR

        public FloatingTextPresenter(EventHub eventHub, FloatingTextService service, IFloatingTextAnchorProvider anchorProvider)
        {
            _eventHub = eventHub;
            _service = service;
            _anchorProvider = anchorProvider;

            RegisterEvents();
        }

        // PUBLIC METHODS

        public void Dispose()
        {
            if (_isDisposed)
                return;

            UnregisterEvents();
            _isDisposed = true;
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.HealthEvents.DamageAppliedEvent.Register(OnDamageApplied);
            _eventHub.HealthEvents.HealthChangedEvent.Register(OnHealthChanged);
            _eventHub.HealthEvents.EntityDiedEvent.Register(OnEntityDied);
        }

        private void UnregisterEvents()
        {
            _eventHub.HealthEvents.DamageAppliedEvent.Unregister(OnDamageApplied);
            _eventHub.HealthEvents.HealthChangedEvent.Unregister(OnHealthChanged);
            _eventHub.HealthEvents.EntityDiedEvent.Unregister(OnEntityDied);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Shows the damage number using AppliedToHealth (post-shield, the
        /// amount that actually came off health — matches what the health bar
        /// visibly loses). Also records pre-hit health and this hit's health
        /// damage so a subsequent death can be checked for overkill.
        /// </summary>
private void OnDamageApplied(DamageAppliedPayload payload)
{
    if (payload.AppliedToHealth <= 0)
        return;

    Vector3 anchor = _anchorProvider.GetAnchorPosition(payload.TargetEntityId);
    _service.ShowDamage(payload.TargetEntityId, anchor, payload.AppliedToHealth);

    _healthBeforeLastHit[payload.TargetEntityId] = payload.HealthBefore;
    _lastHealthDamage[payload.TargetEntityId] = payload.AppliedToHealth;
}

        /// <summary>
        /// Shows a heal number when a health increase with reason Heal is
        /// raised. Damage-caused decreases are already covered by
        /// OnDamageApplied and are ignored here to avoid double text.
        /// </summary>
private void OnHealthChanged(HealthChangedPayload payload)
{
    if (payload.Reason != HealthChangeReason.Heal)
        return;

    int delta = payload.CurrentHealth - payload.PreviousHealth;

    if (delta <= 0)
        return;

    Vector3 anchor = _anchorProvider.GetAnchorPosition(payload.EntityId);
    _service.ShowHeal(payload.EntityId, anchor, delta);
}

        /// <summary>
        /// Shows K.O., and additionally Overkill if the last recorded hit's
        /// health damage exceeded OverkillHealthMultiplier times the health
        /// the entity had immediately before that hit.
        /// </summary>
private void OnEntityDied(EntityDiedPayload payload)
{
    Vector3 anchor = _anchorProvider.GetAnchorPosition(payload.EntityId);

    _service.ShowKnockOut(payload.EntityId, anchor);

    if (_healthBeforeLastHit.TryGetValue(payload.EntityId, out int healthBefore) &&
        _lastHealthDamage.TryGetValue(payload.EntityId, out int healthDamage))
    {
        if (healthBefore > 0 && healthDamage >= healthBefore * OverkillHealthMultiplier)
        {
            _service.ShowOverkill(payload.EntityId, anchor);
        }
    }

    _healthBeforeLastHit.Remove(payload.EntityId);
    _lastHealthDamage.Remove(payload.EntityId);
}
    }
}