using MightyHeroes.Gameplay.Common.Runtime;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Combat.Interfaces;
using UnityEngine;
using MightyHeroes.Gameplay.Health;
using MightyHeroes.Core.EventSystem;

namespace MightyHeroes.Gameplay.Common.Entity
{
    /// <summary>
    /// Base MonoBehaviour for all combat-capable entities.
    /// Responsible only for runtime stat initialization and shared stat operations.
    /// </summary>
    public abstract class EntityComponent : MonoBehaviour, ICombatEntity
    {
        /// <summary>
        /// Runtime stats holding mutable battle-time values.
        /// </summary>
        protected EntityRuntimeStats _runtimeStats;

        /// <summary>
        /// Unique runtime identifier of this entity instance.
        /// </summary>
        private int _entityId;

        /// <summary>
        /// Gets the runtime entity identifier assigned by BattleManager.
        /// </summary>
        public int EntityId => _entityId;

        /// <summary>
        /// Runtime health model owned by this entity.
        /// </summary>
        private CombatHealth _health;

        /// <summary>
        /// Current health value used by combat systems and UI.
        /// </summary>
        public int CurrentHealth => _health != null ? _health.CurrentHealth : _runtimeStats != null ? _runtimeStats.CurrentHealth : 0;

        /// <summary>
        /// Current action points value sed by combat system
        /// </summary>
        public int CurrentActionPoints => _runtimeStats != null ? _runtimeStats.CurrentActionPoints : 0;

        /// <summary>
        /// Indicates whether the entity is alive.
        /// </summary>
        public bool IsAlive => _health != null ? _health.IsAlive : _runtimeStats != null && _runtimeStats.CurrentHealth > 0;

        /// <summary>
        /// Provides access to the runtime health model for derived components.
        /// </summary>
        protected CombatHealth Health => _health;

        /// <summary>
        /// Indicates whether the hero still has remaining action points.
        /// </summary>
        public bool HasRemainingActionPoints => _runtimeStats != null && _runtimeStats.CurrentActionPoints > 0;

        /// <summary>
        /// Provides access to runtime stats for derived classes.
        /// </summary>
        public EntityRuntimeStats RuntimeStats => _runtimeStats;

        /// <summary>World-space position of this entity, for floating text and similar presentation needs.</summary>
        public Vector3 Position => transform.position;
        
        protected virtual void Awake()
        {
            InitializeRuntimeStats();
        }

        /// <summary>
        /// Provides static stats data used to initialize runtime stats.
        /// Must be implemented by derived components.
        /// </summary>
        /// <returns>Static stats data asset.</returns>
        protected abstract EntityStatsData GetStatsData();

        /// <summary>
        /// Initializes the runtime health model for this entity.
        /// This method should be called by derived entity components after base initialization.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub instance.</param>
        /// <param name="maxHealth">Initial maximum health.</param>
        /// <param name="maxShield">Initial maximum shield capacity.</param>
        /// <param name="startingShield">
        /// Shield granted at battle start, clamped to maxShield. Granted with
        /// source id 0 so presentation can tell it apart from in-battle shield actions.
        /// </param>
        protected void InitializeHealth(EventHub eventHub, int maxHealth, int maxShield, int startingShield)
        {
            _health = new CombatHealth(eventHub, EntityId, maxHealth, maxShield);
            _health.AddShield(startingShield, sourceEntityId: 0);
        }

        /// <summary>
        /// Initializes runtime stats using static stats data.
        /// </summary>
        protected void InitializeRuntimeStats()
        {
            EntityStatsData statsData = GetStatsData();

            if (statsData == null)
            {
                Debug.LogError($"{name} returned null StatsData.", this);
                return;
            }

            _runtimeStats = new EntityRuntimeStats(statsData);
        }

        /// <summary>
        /// Derived classes must implement their custom initialization logic.
        /// Called AFTER EntityId is assigned.
        /// </summary>
        protected abstract void OnInitialized();

        /// <summary>
        /// Battle-scope initialization entry point.
        /// This method is controlled by the base class.
        /// </summary>
        public void Initialize(int entityId)
        {
            if (_entityId != 0)
            {
                Debug.LogError($"{name} already initialized.", this);
                return;
            }

            _entityId = entityId;

            OnInitialized();
        }

        /// <summary>
        /// Attempts to consume the specified amount of action points.
        /// </summary>
        /// <param name="amount">Action point cost.</param>
        /// <returns>True if action points were consumed successfully.</returns>
        public bool TryConsumeActionPoints(int amount)
        {
            return _runtimeStats != null && _runtimeStats.TryConsumeActionPoints(amount);
        }

        /// <summary>
        /// Resets action points at the beginning of a new turn.
        /// </summary>
        public void ResetActionPoints()
        {
            _runtimeStats?.ResetActionPoints();
        }
    }
}