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
    /// (designer-set per grid element type in the puzzle grid asset). Attack
    /// and MagicAttack are both attack actions with different DamageType —
    /// the distinction lives entirely in ActionType, not in tile color, so
    /// a designer can reassign which color means what purely in the puzzle
    /// config without any code change here.
    ///
    /// ActionTypes with no implemented system yet (Buff, Defense, Special,
    /// Skip, None) resolve to HeroActionExecution.None() — no AP is spent
    /// and no animation plays for those matches.
    /// </summary>
    public sealed class HeroActionResolver
        : ActionResolverBase<PuzzleTilesMatchedPayload, HeroActionExecution>
    {
        // PUBLIC METHODS

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

        private HeroActionExecution ResolveAttack(PuzzleTilesMatchedPayload payload, DamageType damageType)
        {
            int attackVariant = ResolveAttackVariant(payload);
            int attackIntensity = ResolveIntensity(payload.MatchCount);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroActionResolver: Attack MatchCount={payload.MatchCount} Intensity={attackIntensity} DamageType={damageType}");

            return HeroActionExecution.Attack(attackVariant, attackIntensity, damageType);
        }

        private HeroActionExecution ResolveHeal(PuzzleTilesMatchedPayload payload)
        {
            int healVariant = ResolveHealVariant(payload);
            int healIntensity = ResolveIntensity(payload.MatchCount);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroActionResolver: Heal MatchCount={payload.MatchCount} Intensity={healIntensity}");

            return HeroActionExecution.Heal(healVariant, healIntensity);
        }

        /// <summary>
        /// Temporary rule: single default attack animation regardless of DamageType.
        /// Can later depend on weapon, class, buffs, etc. — DamageType itself
        /// already selects Physical vs Magical VFX/animation branch downstream.
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