using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Puzzle.Payloads;

namespace MightyHeroes.Gameplay.Hero.Actions
{
    /// <summary>
    /// Resolves puzzle results into executable hero actions.
    ///
    /// ActionType comes pre-assigned from the puzzle's tile configuration
    /// (designer-set per grid element type in the Inspector). Attack and
    /// MagicAttack are both attack actions with different DamageType, so a
    /// designer can reassign which color means what without any code change.
    ///
    /// Match count is converted into an action tier (1..3) via ActionTierData.
    /// The tier is carried as HeroActionExecution.ActionIntensity: for attacks
    /// it equals the combo step count. For support actions the tier selects
    /// the per-tier effect value, resolved here into EffectValue so states
    /// never read balance data directly.
    ///
    /// ActionTypes with no implemented system yet (Buff, Special,
    /// Skip, None) resolve to HeroActionExecution.None(); no AP is spent
    /// and no animation plays for those matches.
    /// </summary>
    public sealed class HeroActionResolver : ActionResolverBase<PuzzleTilesMatchedPayload, HeroActionExecution>
    {
        // FIELDS

        private readonly ActionTierData _tierData;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a hero action resolver bound to the given tier configuration.
        /// </summary>
        /// <param name="tierData">Match count thresholds and per-tier values. Must not be null.</param>
        public HeroActionResolver(ActionTierData tierData)
        {
            _tierData = tierData;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Resolves a puzzle match into a hero action execution.
        /// </summary>
        /// <param name="payload">Matched tiles payload raised by the puzzle.</param>
        /// <returns>Resolved hero action, or None when the match has no handler.</returns>
        public override HeroActionExecution Resolve(PuzzleTilesMatchedPayload payload)
        {
            if (payload == null)
                return HeroActionExecution.None();

            switch (payload.ActionType)
            {
                case ActionType.Attack:
                    return ResolveAttack(payload, DamageType.Physical);

                case ActionType.MagicAttack:
                    return ResolveAttack(payload, DamageType.Magical);

                case ActionType.Heal:
                    return ResolveSupport(payload, ActionType.Heal, _tierData.HealAmounts);

                case ActionType.Shield:
                    return ResolveSupport(payload, ActionType.Shield, _tierData.ShieldAmounts);

                default:
                    GameLogger.Log(LogSourceType.Hero,
                        $"HeroActionResolver: ActionType={payload.ActionType} has no handler yet, ignoring match (no AP spent).");
                    return HeroActionExecution.None();
            }
        }

        // PRIVATE METHODS

        /// <summary>Resolves an attack execution; tier becomes the combo step count.</summary>
        private HeroActionExecution ResolveAttack(PuzzleTilesMatchedPayload payload, DamageType damageType)
        {
            int attackVariant = ResolveAttackVariant(payload);
            int tier = ResolveTier(payload.MatchCount, _tierData);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroActionResolver: Attack MatchCount={payload.MatchCount} Tier={tier} DamageType={damageType}");

            return HeroActionExecution.Attack(attackVariant, tier, damageType);
        }

        /// <summary>
        /// Resolves a self-targeted support execution whose effect value is
        /// picked from the given per-tier array for the match tier.
        /// </summary>
        /// <param name="payload">Matched tiles payload.</param>
        /// <param name="supportType">Support action type (Heal, Shield, Buff).</param>
        /// <param name="tierValues">Per-tier effect values from ActionTierData.</param>
        private HeroActionExecution ResolveSupport(PuzzleTilesMatchedPayload payload, ActionType supportType, int[] tierValues)
        {
            int supportVariant = ResolveSupportVariant(payload);
            int tier = ResolveTier(payload.MatchCount, _tierData);
            int effectValue = GetTierValue(tierValues, tier);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroActionResolver: {supportType} MatchCount={payload.MatchCount} Tier={tier} Value={effectValue}");

            return HeroActionExecution.Support(supportType, supportVariant, tier, effectValue);
        }

        /// <summary>
        /// Returns the per-tier value for the given tier (index tier - 1).
        /// Clamps to the last entry if the array is shorter than expected
        /// (ActionTierData.OnValidate already warns about this in the Editor).
        /// </summary>
        private int GetTierValue(int[] values, int tier)
        {
            if (values == null || values.Length == 0)
                return 0;

            int index = tier - 1;

            if (index < 0)
                index = 0;

            if (index >= values.Length)
                index = values.Length - 1;

            return values[index];
        }

        /// <summary>
        /// Temporary rule: single default attack animation regardless of DamageType.
        /// Can later depend on weapon, class, buffs, etc.
        /// </summary>
        private int ResolveAttackVariant(PuzzleTilesMatchedPayload payload)
        {
            return 1;
        }

        /// <summary>Temporary rule: single default support (heal) animation for all support types.</summary>
        private int ResolveSupportVariant(PuzzleTilesMatchedPayload payload)
        {
            return 1;
        }
    }
}
