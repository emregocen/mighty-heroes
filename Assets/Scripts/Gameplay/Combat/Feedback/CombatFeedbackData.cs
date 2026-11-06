using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Design-time tuning for combat "feel" feedback driven by hit events.
    ///
    /// Per-tier arrays are indexed by tier - 1 (index 0 = Tier 1). For hero
    /// attacks the tier equals the combo step count (HeroActionExecution
    /// ActionIntensity), so a 9+ match (Tier 3) produces the strongest feedback.
    ///
    /// All values are placeholders until the balancing pass. Data only; the
    /// logic that reads it lives in CombatFeedbackPresenter.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatFeedbackData", menuName = "Mighty Heroes/Gameplay/Combat Feedback Data")]
    public sealed class CombatFeedbackData : ScriptableObject
    {
        // CONSTANTS

        /// <summary>Number of supported action tiers.</summary>
        public const int TierCount = 3;

        // FIELDS

        [Header("Camera Shake - Hero Attacks")]
        [Tooltip("Impulse force per tier for the final hit of a hero attack (index 0 = Tier 1).")]
        [SerializeField] private float[] _heroHitShakeForces = { 0.2f, 0.45f, 0.9f };

        [Tooltip("Multiplier applied to non-final combo hits so the last hit lands hardest.")]
        [SerializeField, Range(0f, 1f)] private float _nonFinalHitShakeMultiplier = 0.5f;

        [Tooltip("Multiplier applied when the hit kills its target.")]
        [SerializeField, Min(1f)] private float _killingHitShakeMultiplier = 1.3f;

        [Header("Camera Shake - Enemy Attacks")]
        [Tooltip("Impulse force for every enemy hit that lands on the hero.")]
        [SerializeField, Min(0f)] private float _enemyHitShakeForce = 0.3f;

        // PROPERTIES

        /// <summary>Final-hit impulse force per tier, indexed by tier - 1.</summary>
        public float[] HeroHitShakeForces => _heroHitShakeForces;

        /// <summary>Multiplier for non-final combo hits.</summary>
        public float NonFinalHitShakeMultiplier => _nonFinalHitShakeMultiplier;

        /// <summary>Multiplier for hits that kill their target.</summary>
        public float KillingHitShakeMultiplier => _killingHitShakeMultiplier;

        /// <summary>Impulse force for enemy hits on the hero.</summary>
        public float EnemyHitShakeForce => _enemyHitShakeForce;

#if UNITY_EDITOR
        // UNITY CALLBACKS

        /// <summary>Editor-only validation of per-tier array length.</summary>
        private void OnValidate()
        {
            if (_heroHitShakeForces == null || _heroHitShakeForces.Length != TierCount)
                Debug.LogWarning($"CombatFeedbackData._heroHitShakeForces must contain exactly {TierCount} entries.", this);
        }
#endif
    }
}
