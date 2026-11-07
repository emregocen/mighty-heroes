using System;
using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Design-time tuning for combat "feel" feedback driven by hit events:
    /// camera shake, hit VFX and hit-stop.
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

        // NESTED TYPES

        /// <summary>
        /// One hit VFX setup: every prefab in the list plays together at the
        /// same position with the same uniform scale.
        /// </summary>
        [Serializable]
        public sealed class HitVfxSet
        {
            /// <summary>Particle prefabs played together for this hit.</summary>
            public GameObject[] Prefabs;

            /// <summary>Uniform scale applied to every prefab in the set.</summary>
            [Min(0.01f)] public float Scale = 1f;
        }

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

        [Header("Hit-Stop - Hero Attacks")]
        [Tooltip("Animator freeze per tier on the final hit of a hero attack, in seconds (index 0 = Tier 1). Zero disables hit-stop for that tier.")]
        [SerializeField] private float[] _heroHitStopDurations = { 0f, 0f, 0.1f };

        [Tooltip("Freeze used instead when the final hit also kills the target. Only applies to tiers whose hit-stop is enabled.")]
        [SerializeField, Min(0f)] private float _killingHitStopDuration = 0.2f;

        [Header("Hit VFX - Hero Attacks")]
        [Tooltip("VFX per tier for the final hit of a hero attack (index 0 = Tier 1). Non-final combo hits use the tier below.")]
        [SerializeField] private HitVfxSet[] _heroHitVfxTiers = new HitVfxSet[TierCount];

        [Header("Hit VFX - Enemy Attacks")]
        [Tooltip("VFX for every enemy hit that lands on the hero.")]
        [SerializeField] private HitVfxSet _enemyHitVfx = new HitVfxSet();

        [Header("Hit VFX - Placement")]
        [Tooltip("World-space height above the target's root where hit VFX spawn (roughly chest height).")]
        [SerializeField] private float _hitVfxHeightOffset = 1.1f;

        [Tooltip("World distance hit VFX are pulled from the target's center toward the camera, so they render in front of the body instead of inside it.")]
        [SerializeField, Min(0f)] private float _hitVfxTowardCameraOffset = 0.6f;

        [Tooltip("Seconds before a hit VFX instance is stopped and returned to its pool.")]
        [SerializeField, Min(0.1f)] private float _hitVfxLifetime = 1.5f;

        // PROPERTIES

        /// <summary>Final-hit impulse force per tier, indexed by tier - 1.</summary>
        public float[] HeroHitShakeForces => _heroHitShakeForces;

        /// <summary>Multiplier for non-final combo hits.</summary>
        public float NonFinalHitShakeMultiplier => _nonFinalHitShakeMultiplier;

        /// <summary>Multiplier for hits that kill their target.</summary>
        public float KillingHitShakeMultiplier => _killingHitShakeMultiplier;

        /// <summary>Impulse force for enemy hits on the hero.</summary>
        public float EnemyHitShakeForce => _enemyHitShakeForce;

        /// <summary>Final-hit animator freeze per tier in seconds, indexed by tier - 1.</summary>
        public float[] HeroHitStopDurations => _heroHitStopDurations;

        /// <summary>Freeze used when the final hit kills the target (enabled tiers only).</summary>
        public float KillingHitStopDuration => _killingHitStopDuration;

        /// <summary>Hero hit VFX per tier, indexed by tier - 1.</summary>
        public HitVfxSet[] HeroHitVfxTiers => _heroHitVfxTiers;

        /// <summary>VFX for enemy hits on the hero.</summary>
        public HitVfxSet EnemyHitVfx => _enemyHitVfx;

        /// <summary>World-space height above the target root for hit VFX.</summary>
        public float HitVfxHeightOffset => _hitVfxHeightOffset;

        /// <summary>World distance hit VFX are pulled toward the camera from the target's center.</summary>
        public float HitVfxTowardCameraOffset => _hitVfxTowardCameraOffset;

        /// <summary>Seconds before a hit VFX instance returns to its pool.</summary>
        public float HitVfxLifetime => _hitVfxLifetime;

#if UNITY_EDITOR
        // UNITY CALLBACKS

        /// <summary>Editor-only validation of per-tier array lengths.</summary>
        private void OnValidate()
        {
            if (_heroHitShakeForces == null || _heroHitShakeForces.Length != TierCount)
                Debug.LogWarning($"CombatFeedbackData._heroHitShakeForces must contain exactly {TierCount} entries.", this);

            if (_heroHitStopDurations == null || _heroHitStopDurations.Length != TierCount)
                Debug.LogWarning($"CombatFeedbackData._heroHitStopDurations must contain exactly {TierCount} entries.", this);

            if (_heroHitVfxTiers == null || _heroHitVfxTiers.Length != TierCount)
                Debug.LogWarning($"CombatFeedbackData._heroHitVfxTiers must contain exactly {TierCount} entries.", this);
        }
#endif
    }
}
