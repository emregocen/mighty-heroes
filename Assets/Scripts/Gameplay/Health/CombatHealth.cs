using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Health.Types;

namespace MightyHeroes.Gameplay.Health
{
    /// <summary>
    /// Plain C# runtime health and shield model for a single combat entity.
    ///
    /// Owns current and maximum values for both health and shield.
    /// Raises EventHub events so external systems (UI, state machines, analytics)
    /// can react to health state changes without direct coupling.
    ///
    /// This class does NOT:
    /// - Control animations or state machine transitions.
    /// - Know about turn flow or combat rules.
    /// - Apply damage formulas — damage is already resolved by CombatResolver
    ///   before ApplyDamage is called.
    ///
    /// Event contract:
    /// - DamageAppliedEvent  : raised once per ApplyDamage call. Contains full
    ///                         breakdown (raw, absorbed, applied). Intended for
    ///                         damage number popups, combat logs, and analytics.
    /// - HealthChangedEvent  : raised when current health value changes for ANY reason
    ///                         (damage, heal, revive, stat clamp). Intended for
    ///                         health bar UI and general health listeners.
    /// - ShieldChangedEvent  : raised when current shield value changes.
    /// - EntityDiedEvent     : raised once when health reaches zero.
    /// - EntityRevivedEvent  : raised when a dead entity is revived.
    /// - LifestealAppliedEvent: raised when lifesteal healing is calculated and applied.
    ///
    /// Shield initialization:
    /// Current shield starts at 0 regardless of maxShield.
    /// Shield must be added explicitly via AddShield — it is not granted automatically.
    /// This allows buff systems to control when shield becomes active.
    ///
    /// HitDirection note:
    /// ApplyDamage accepts HitDirection only to forward it into DamageAppliedPayload
    /// and EntityDiedPayload for animation context. Health logic itself is direction-agnostic.
    /// This parameter will be decoupled from CombatHealth in a future refactor.
    /// </summary>
    public sealed class CombatHealth
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly int _entityId;

        private int _maxHealth;
        private int _currentHealth;
        private int _maxShield;
        private int _currentShield;
        private bool _isAlive;

        // PROPERTIES

        /// <summary>Owning entity runtime id.</summary>
        public int EntityId => _entityId;

        /// <summary>Maximum health value.</summary>
        public int MaxHealth => _maxHealth;

        /// <summary>Current health value. Always between 0 and MaxHealth.</summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>Maximum shield capacity.</summary>
        public int MaxShield => _maxShield;

        /// <summary>
        /// Current shield value. Starts at 0 — must be granted via AddShield.
        /// Always between 0 and MaxShield.
        /// </summary>
        public int CurrentShield => _currentShield;

        /// <summary>True while the entity has health above zero.</summary>
        public bool IsAlive => _isAlive;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new health model. Current health is set to maxHealth.
        /// Current shield starts at 0 and must be granted explicitly via AddShield.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub for raising health events.</param>
        /// <param name="entityId">Owner entity runtime id.</param>
        /// <param name="maxHealth">Starting maximum health. Clamped to minimum 1.</param>
        /// <param name="maxShield">Maximum shield capacity. Clamped to minimum 0.</param>
        public CombatHealth(EventHub eventHub, int entityId, int maxHealth, int maxShield)
        {
            _eventHub = eventHub;
            _entityId = entityId;

            if (maxHealth < 1)
                maxHealth = 1;

            if (maxShield < 0)
                maxShield = 0;

            _maxHealth = maxHealth;
            _currentHealth = maxHealth;

            _maxShield = maxShield;
            _currentShield = 0;

            _isAlive = true;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Updates the maximum health value.
        /// If current health exceeds the new maximum, it is clamped down
        /// and HealthChangedEvent is raised with reason StatChange.
        /// </summary>
        /// <param name="maxHealth">New maximum health. Clamped to minimum 1.</param>
        public void SetMaxHealth(int maxHealth)
        {
            if (maxHealth < 1)
                maxHealth = 1;

            _maxHealth = maxHealth;

            if (_currentHealth > _maxHealth)
            {
                int previous = _currentHealth;
                _currentHealth = _maxHealth;
                RaiseHealthChanged(previous, _currentHealth, HealthChangeReason.StatChange, 0);
            }
        }

        /// <summary>
        /// Updates the maximum shield capacity.
        /// If current shield exceeds the new maximum, it is clamped down
        /// and ShieldChangedEvent is raised.
        /// </summary>
        /// <param name="maxShield">New maximum shield capacity. Clamped to minimum 0.</param>
        public void SetMaxShield(int maxShield)
        {
            if (maxShield < 0)
                maxShield = 0;

            _maxShield = maxShield;

            if (_currentShield > _maxShield)
            {
                int previous = _currentShield;
                _currentShield = _maxShield;
                RaiseShieldChanged(previous, _currentShield, 0);
            }
        }

        /// <summary>
        /// Adds shield up to the maximum shield capacity.
        /// Does nothing if the entity is dead or amount is zero or negative.
        /// Raises ShieldChangedEvent if the value changes.
        /// </summary>
        /// <param name="amount">Shield amount to add.</param>
        /// <param name="sourceEntityId">Entity granting the shield. 0 if unknown.</param>
        public void AddShield(int amount, int sourceEntityId)
        {
            if (!_isAlive || amount <= 0)
                return;

            int previous = _currentShield;
            _currentShield = System.Math.Min(_currentShield + amount, _maxShield);

            if (_currentShield != previous)
                RaiseShieldChanged(previous, _currentShield, sourceEntityId);
        }

        /// <summary>
        /// Removes shield down to zero.
        /// Raises ShieldChangedEvent if the value changes.
        /// </summary>
        /// <param name="amount">Shield amount to remove.</param>
        /// <param name="sourceEntityId">Entity removing the shield. 0 if unknown.</param>
        public void RemoveShield(int amount, int sourceEntityId)
        {
            if (amount <= 0)
                return;

            int previous = _currentShield;
            _currentShield = System.Math.Max(_currentShield - amount, 0);

            if (_currentShield != previous)
                RaiseShieldChanged(previous, _currentShield, sourceEntityId);
        }

        /// <summary>
        /// Applies healing to this entity up to MaxHealth. Overheal is not supported.
        /// Does nothing if the entity is dead or amount is zero or negative.
        /// Raises HealthChangedEvent if the value changes.
        /// </summary>
        /// <param name="amount">Heal amount.</param>
        /// <param name="sourceEntityId">Entity providing the heal. 0 if unknown.</param>
        public void Heal(int amount, int sourceEntityId)
        {
            if (!_isAlive || amount <= 0)
                return;

            int previous = _currentHealth;
            _currentHealth = System.Math.Min(_currentHealth + amount, _maxHealth);

            if (_currentHealth != previous)
                RaiseHealthChanged(previous, _currentHealth, HealthChangeReason.Heal, sourceEntityId);
        }

        /// <summary>
        /// Applies resolved combat damage to this entity.
        ///
        /// Processing order:
        /// 1. Shield absorbs damage first (unless DamageType.True).
        /// 2. Remaining damage is applied to health.
        /// 3. DamageAppliedEvent is raised with full breakdown.
        /// 4. ShieldChangedEvent is raised if shield value changed.
        /// 5. HealthChangedEvent is raised if health value changed.
        /// 6. EntityDiedEvent is raised if health reached zero.
        /// 7. Lifesteal healing is applied to the source if lifestealPercent > 0.
        ///
        /// Does nothing if the entity is already dead or amount is zero or negative.
        /// </summary>
        /// <param name="amount">Resolved damage amount from CombatResolver.</param>
        /// <param name="damageType">Damage type. True damage bypasses shields.</param>
        /// <param name="hitDirection">Direction context forwarded to event payloads for animation.</param>
        /// <param name="sourceEntityId">Attacking entity id. 0 if unknown.</param>
        /// <param name="lifestealPercent">Fraction of applied health damage returned as healing to source. 0 disables lifesteal.</param>
        public void ApplyDamage(
            int amount,
            DamageType damageType,
            HitDirection hitDirection,
            int sourceEntityId,
            float lifestealPercent)
        {
            if (!_isAlive || amount <= 0)
                return;

            int healthBefore = _currentHealth;
            int shieldBefore = _currentShield;

            int absorbedByShield = 0;
            int remainingDamage = amount;

            // Shield absorbs damage first unless this is true damage.
            if (damageType != DamageType.True && _currentShield > 0)
            {
                absorbedByShield = System.Math.Min(remainingDamage, _currentShield);
                _currentShield -= absorbedByShield;
                remainingDamage -= absorbedByShield;
            }

            int appliedToHealth = System.Math.Max(remainingDamage, 0);

            if (appliedToHealth > 0)
                _currentHealth = System.Math.Max(_currentHealth - appliedToHealth, 0);

            int healthAfter = _currentHealth;
            int shieldAfter = _currentShield;

            // Always raise DamageAppliedEvent — contains full breakdown for combat logs and popups.
            RaiseDamageApplied(
                sourceEntityId, _entityId,
                amount, damageType,
                absorbedByShield, appliedToHealth,
                healthBefore, healthAfter,
                shieldBefore, shieldAfter,
                hitDirection);

            // Raise change events only when values actually changed.
            if (shieldAfter != shieldBefore)
                RaiseShieldChanged(shieldBefore, shieldAfter, sourceEntityId);

            if (healthAfter != healthBefore)
                RaiseHealthChanged(healthBefore, healthAfter, HealthChangeReason.Damage, sourceEntityId);

            if (healthAfter <= 0)
                HandleDeath(sourceEntityId, hitDirection);

            ApplyLifestealIfAny(sourceEntityId, appliedToHealth, lifestealPercent);
        }

        /// <summary>
        /// Revives a dead entity and restores the given health amount.
        /// Does nothing if the entity is already alive or healthAmount is zero or negative.
        /// Raises HealthChangedEvent and EntityRevivedEvent.
        /// </summary>
        /// <param name="healthAmount">Health to restore on revive. Clamped to MaxHealth.</param>
        /// <param name="sourceEntityId">Entity performing the revive. 0 if unknown.</param>
        public void Revive(int healthAmount, int sourceEntityId)
        {
            if (_isAlive || healthAmount <= 0)
                return;

            int previous = _currentHealth;
            _currentHealth = System.Math.Min(healthAmount, _maxHealth);
            _isAlive = true;

            RaiseHealthChanged(previous, _currentHealth, HealthChangeReason.Revive, sourceEntityId);
            RaiseEntityRevived(_entityId, _currentHealth, sourceEntityId);
        }

        // PRIVATE METHODS

        /// <summary>
        /// Marks the entity as dead and raises EntityDiedEvent.
        /// Guard prevents double-death if called more than once.
        /// </summary>
        private void HandleDeath(int killerEntityId, HitDirection hitDirection)
        {
            if (!_isAlive)
                return;

            _isAlive = false;
            RaiseEntityDied(_entityId, killerEntityId, hitDirection);
        }

        /// <summary>
        /// Calculates and applies lifesteal healing to the source entity.
        /// Raises LifestealAppliedEvent so the source entity's health model
        /// can react and heal itself.
        /// </summary>
        private void ApplyLifestealIfAny(int sourceEntityId, int appliedToHealth, float lifestealPercent)
        {
            if (sourceEntityId <= 0 || appliedToHealth <= 0 || lifestealPercent <= 0f)
                return;

            int healAmount = (int)(appliedToHealth * lifestealPercent);

            if (healAmount <= 0)
                return;

            _eventHub.HealthEvents.LifestealAppliedEvent.Raise(
                new LifestealAppliedPayload(sourceEntityId, _entityId, healAmount));
        }

        private void RaiseDamageApplied(
            int sourceEntityId, int targetEntityId,
            int rawDamage, DamageType damageType,
            int absorbedByShield, int appliedToHealth,
            int healthBefore, int healthAfter,
            int shieldBefore, int shieldAfter,
            HitDirection hitDirection)
        {
            _eventHub.HealthEvents.DamageAppliedEvent.Raise(
                new DamageAppliedPayload(
                    sourceEntityId, targetEntityId,
                    rawDamage, damageType,
                    absorbedByShield, appliedToHealth,
                    healthBefore, healthAfter,
                    shieldBefore, shieldAfter,
                    hitDirection));
        }

        private void RaiseHealthChanged(int previous, int current, HealthChangeReason reason, int sourceEntityId)
        {
            _eventHub.HealthEvents.HealthChangedEvent.Raise(
                new HealthChangedPayload(_entityId, previous, current, _maxHealth, reason, sourceEntityId));
        }

        private void RaiseShieldChanged(int previous, int current, int sourceEntityId)
        {
            _eventHub.HealthEvents.ShieldChangedEvent.Raise(
                new ShieldChangedPayload(_entityId, previous, current, _maxShield, sourceEntityId));
        }

        private void RaiseEntityDied(int entityId, int killerEntityId, HitDirection hitDirection)
        {
            _eventHub.HealthEvents.EntityDiedEvent.Raise(
                new EntityDiedPayload(entityId, killerEntityId, hitDirection));
        }

        private void RaiseEntityRevived(int entityId, int currentHealth, int sourceEntityId)
        {
            _eventHub.HealthEvents.EntityRevivedEvent.Raise(
                new EntityRevivedPayload(entityId, currentHealth, sourceEntityId));
        }
    }
}