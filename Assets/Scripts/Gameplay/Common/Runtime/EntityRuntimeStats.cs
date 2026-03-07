
using MightyHeroes.Gameplay.Common.Stats;

namespace MightyHeroes.Gameplay.Common.Runtime
{
    /// <summary>
    /// Holds mutable, battle-time statistics for an entity.
    /// Initialized from EntityStatsData and modified during combat.
    /// This class contains no Unity dependencies.
    /// </summary>
    public sealed class EntityRuntimeStats
    {
        // 1) FIELDS

        private readonly EntityStatsData _baseStats;

        private int _currentHealth;
        private int _currentActionPoints;

        // 2) PROPERTIES

        /// <summary>
        /// Base immutable stats used as reference values.
        /// </summary>
        public EntityStatsData BaseStats => _baseStats;

        /// <summary>
        /// Current health value during battle.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Current action points available in the active turn.
        /// </summary>
        public int CurrentActionPoints => _currentActionPoints;

        /// <summary>
        /// Indicates whether the entity is alive.
        /// </summary>
        public bool IsAlive => _currentHealth > 0;

        // 3) CONSTRUCTOR

        /// <summary>
        /// Creates runtime stats initialized from static stats data.
        /// </summary>
        /// <param name="baseStats">Static stats data reference.</param>
        public EntityRuntimeStats(EntityStatsData baseStats)
        {
            _baseStats = baseStats;

            ResetHealth();
            ResetActionPoints();
        }

        // 4) PUBLIC METHODS

        /// <summary>
        /// Resets health to its maximum value.
        /// Typically called at battle start.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _baseStats.MaxHealth;
        }

        /// <summary>
        /// Resets action points to base value.
        /// Typically called at turn start.
        /// </summary>
        public void ResetActionPoints()
        {
            _currentActionPoints = _baseStats.BaseActionPoints;
        }

        /// <summary>
        /// Consumes action points if sufficient amount is available.
        /// </summary>
        /// <param name="amount">Amount of action points to consume.</param>
        /// <returns>True if consumption succeeded; otherwise false.</returns>
        public bool TryConsumeActionPoints(int amount)
        {
            if (_currentActionPoints < amount)
            {
                return false;
            }

            _currentActionPoints -= amount;
            return true;
        }

        /// <summary>
        /// Applies damage after defense mitigation.
        /// </summary>
        /// <param name="rawDamage">Incoming raw damage value.</param>
        public void ApplyDamage(int rawDamage)
        {
            int mitigatedDamage = rawDamage - _baseStats.BaseDefense;

            if (mitigatedDamage < 1)
            {
                mitigatedDamage = 1;
            }

            _currentHealth -= mitigatedDamage;

            if (_currentHealth < 0)
            {
                _currentHealth = 0;
            }
        }

        /// <summary>
        /// Restores health up to the maximum value.
        /// </summary>
        /// <param name="amount">Amount of health to restore.</param>
        public void Heal(int amount)
        {
            _currentHealth += amount;

            if (_currentHealth > _baseStats.MaxHealth)
            {
                _currentHealth = _baseStats.MaxHealth;
            }
        }
    }
}