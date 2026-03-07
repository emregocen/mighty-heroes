using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MightyHeroes.Gameplay.Common.Stats
{
    /// <summary>
    /// Base stats data shared by all battle entities.
    /// Contains only static, design-time combat values.
    /// This data must not be mutated at runtime.
    /// </summary>
    public abstract class EntityStatsData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _entityKey;

        [Header("Base Stats")]
        [SerializeField] private int _maxHealth;
        [SerializeField] private int _maxShield;
        [SerializeField] private int _baseAttack;
        [SerializeField] private int _baseDefense;
        [SerializeField] private int _baseActionPoints;

        /// <summary>
        /// Unique string key used for persistence, analytics, and live-ops.
        /// Must be globally unique across all EntityStatsData assets.
        /// Must not be used for gameplay logic or branching.
        /// </summary>
        public string EntityKey => _entityKey;

        /// <summary>
        /// Maximum health value at battle start.
        /// </summary>
        public int MaxHealth => _maxHealth;

        /// <summary>
        /// Maximum shield capacity at battle start.
        /// </summary>
        public int MaxShield => _maxShield;

        /// <summary>
        /// Base attack power before any modifiers are applied.
        /// </summary>
        public int BaseAttack => _baseAttack;

        /// <summary>
        /// Base defense value used for damage mitigation.
        /// </summary>
        public int BaseDefense => _baseDefense;

        /// <summary>
        /// Base action points available at the start of a turn.
        /// </summary>
        public int BaseActionPoints => _baseActionPoints;

#if UNITY_EDITOR

        /// <summary>
        /// Validates that the EntityKey is unique across all EntityStatsData assets.
        /// This check runs only in the Unity Editor and never at runtime.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_entityKey))
            {
                return;
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:EntityStatsData");

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EntityStatsData other = AssetDatabase.LoadAssetAtPath<EntityStatsData>(assetPath);

                if (other == null || other == this)
                {
                    continue;
                }

                if (other._entityKey == _entityKey)
                {
                    Debug.LogError(
                        $"Duplicate EntityKey detected: '{_entityKey}'.\nConflicting asset: {assetPath}",
                        this
                    );
                    break;
                }
            }
        }
#endif
    }
}