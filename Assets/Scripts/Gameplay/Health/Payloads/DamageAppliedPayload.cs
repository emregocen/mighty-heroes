using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Health.Types;

namespace MightyHeroes.Gameplay.Health.Payloads
{
    /// <summary>
    /// Raised when damage is applied to a target.
    /// Includes shield absorption and final effective values.
    /// </summary>
    public sealed class DamageAppliedPayload
    {
        /// <summary>
        /// Attacker entity id. Can be 0 if the source is environmental or unknown.
        /// </summary>
        public int SourceEntityId { get; }

        /// <summary>
        /// Target entity id that received the damage.
        /// </summary>
        public int TargetEntityId { get; }

        /// <summary>
        /// Requested damage before any shield processing.
        /// </summary>
        public int RawDamage { get; }

        /// <summary>
        /// Damage type used for shield bypass rules.
        /// </summary>
        public DamageType DamageType { get; }

        /// <summary>
        /// Damage absorbed by shield.
        /// </summary>
        public int AbsorbedByShield { get; }

        /// <summary>
        /// Damage applied to health after shield absorption.
        /// </summary>
        public int AppliedToHealth { get; }

        /// <summary>
        /// Target health before this damage application.
        /// </summary>
        public int HealthBefore { get; }

        /// <summary>
        /// Target health after this damage application.
        /// </summary>
        public int HealthAfter { get; }

        /// <summary>
        /// Target shield before this damage application.
        /// </summary>
        public int ShieldBefore { get; }

        /// <summary>
        /// Target shield after this damage application.
        /// </summary>
        public int ShieldAfter { get; }

        /// <summary>
        /// Hit direction for reaction/animation context.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Creates a damage applied payload.
        /// </summary>
        public DamageAppliedPayload(
            int sourceEntityId,
            int targetEntityId,
            int rawDamage,
            DamageType damageType,
            int absorbedByShield,
            int appliedToHealth,
            int healthBefore,
            int healthAfter,
            int shieldBefore,
            int shieldAfter,
            HitDirection hitDirection)
        {
            SourceEntityId = sourceEntityId;
            TargetEntityId = targetEntityId;
            RawDamage = rawDamage;
            DamageType = damageType;
            AbsorbedByShield = absorbedByShield;
            AppliedToHealth = appliedToHealth;
            HealthBefore = healthBefore;
            HealthAfter = healthAfter;
            ShieldBefore = shieldBefore;
            ShieldAfter = shieldAfter;
            HitDirection = hitDirection;
        }
    }
}