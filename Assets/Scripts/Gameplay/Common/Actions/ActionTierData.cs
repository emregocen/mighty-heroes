using UnityEngine;

namespace MightyHeroes.Gameplay.Common.Actions
{
    /// <summary>
    /// Design-time configuration that maps puzzle match counts to action tiers
    /// and holds the per-tier effect values for support actions.
    ///
    /// Tier rules:
    /// - Tier 1: any valid match below Tier2MinMatch (the puzzle accepts 2+ tiles).
    /// - Tier 2: match count between Tier2MinMatch and Tier3MinMatch - 1.
    /// - Tier 3: match count of Tier3MinMatch or more.
    ///
    /// Per-tier arrays are indexed by tier - 1 (index 0 = Tier 1).
    /// All values are placeholders until the balancing pass.
    /// This asset contains data only; tier resolution lives in ActionResolverBase.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionTierData", menuName = "Mighty Heroes/Gameplay/Action Tier Data")]
    public sealed class ActionTierData : ScriptableObject
    {
        // CONSTANTS

        /// <summary>Number of supported action tiers.</summary>
        public const int TierCount = 3;

        // FIELDS

        [Header("Match Thresholds")]
        [Tooltip("Minimum matched tile count that resolves to Tier 2.")]
        [SerializeField, Min(3)] private int _tier2MinMatch = 6;

        [Tooltip("Minimum matched tile count that resolves to Tier 3.")]
        [SerializeField, Min(4)] private int _tier3MinMatch = 9;

        [Header("Support Values Per Tier (index 0 = Tier 1)")]
        [Tooltip("Health restored by a Heal action at each tier.")]
        [SerializeField] private int[] _healAmounts = { 20, 35, 55 };

        [Tooltip("Shield granted by a Shield action at each tier.")]
        [SerializeField] private int[] _shieldAmounts = { 15, 30, 50 };

        [Tooltip("Attack bonus percent granted by a Buff action at each tier (25 = +25%).")]
        [SerializeField] private int[] _attackBuffPercents = { 25, 50, 100 };

        // PROPERTIES

        /// <summary>Minimum matched tile count that resolves to Tier 2.</summary>
        public int Tier2MinMatch => _tier2MinMatch;

        /// <summary>Minimum matched tile count that resolves to Tier 3.</summary>
        public int Tier3MinMatch => _tier3MinMatch;

        /// <summary>Heal amounts per tier, indexed by tier - 1.</summary>
        public int[] HealAmounts => _healAmounts;

        /// <summary>Shield amounts per tier, indexed by tier - 1.</summary>
        public int[] ShieldAmounts => _shieldAmounts;

        /// <summary>Attack buff percents per tier, indexed by tier - 1.</summary>
        public int[] AttackBuffPercents => _attackBuffPercents;

#if UNITY_EDITOR
        // UNITY CALLBACKS

        /// <summary>
        /// Editor-only validation: keeps thresholds ordered and warns when a
        /// per-tier array does not contain exactly TierCount entries.
        /// </summary>
        private void OnValidate()
        {
            if (_tier3MinMatch <= _tier2MinMatch)
                _tier3MinMatch = _tier2MinMatch + 1;

            WarnIfInvalidLength(_healAmounts, nameof(_healAmounts));
            WarnIfInvalidLength(_shieldAmounts, nameof(_shieldAmounts));
            WarnIfInvalidLength(_attackBuffPercents, nameof(_attackBuffPercents));
        }

        // PRIVATE METHODS

        /// <summary>Logs a warning when the given per-tier array length differs from TierCount.</summary>
        private void WarnIfInvalidLength(int[] values, string fieldName)
        {
            if (values == null || values.Length != TierCount)
                Debug.LogWarning($"ActionTierData.{fieldName} must contain exactly {TierCount} entries.", this);
        }
#endif
    }
}
