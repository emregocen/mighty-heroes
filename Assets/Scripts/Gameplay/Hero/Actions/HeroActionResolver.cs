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
    /// it equals the combo step count, for support actions it selects the
    /// per-tier effect value.
    ///
    /// ActionTypes with no implemented system yet (Buff, Shield, Special,
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
                    return ResolveHeal(payload);

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

        /// <summary>Resolves a heal execution for the tier of the given match.</summary>
        private HeroActionExecution ResolveHeal(PuzzleTilesMatchedPayload payload)
        {
            int healVariant = ResolveHealVariant(payload);
            int tier = ResolveTier(payload.MatchCount, _tierData);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroActionResolver: Heal MatchCount={payload.MatchCount} Tier={tier}");

            return HeroActionExecution.Heal(healVariant, tier);
        }

        /// <summary>
        /// Temporary rule: single default attack animation regardless of DamageType.
        /// Can later depend on weapon, class, buffs, etc.
        /// </summary>
        private int ResolveAttackVariant(PuzzleTilesMatchedPayload payload)
        {
            return 1;
        }

        /// <summary>Temporary rule: single default heal animation.</summary>
        private int ResolveHealVariant(PuzzleTilesMatchedPayload payload)
        {
            return 1;
        }
    }
}
