using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Puzzle.Payloads;

namespace MightyHeroes.Gameplay.Hero.Actions
{
    /// <summary>
    /// Resolves puzzle results into executable hero actions.
    /// Contains decision rules for hero-driven actions.
    /// </summary>
    public sealed class HeroActionResolver
        : ActionResolverBase<PuzzleTilesMatchedPayload, HeroActionExecution>
    {
        // public methods

        /// <summary>
        /// Resolves a puzzle match payload into a hero action execution.
        /// </summary>
        /// <param name="payload">Puzzle match payload.</param>
        /// <returns>Resolved hero action execution.</returns>
        public override HeroActionExecution Resolve(PuzzleTilesMatchedPayload payload)
        {
            if (payload == null)
                return HeroActionExecution.None();

            if (payload.ActionType != ActionType.Attack)
                return HeroActionExecution.None();

            int attackVariant = ResolveAttackVariant(payload);
            int attackIntensity = ResolveAttackIntensity(payload);

            GameLogger.Log(
                LogSourceType.Hero,
                $"HeroActionResolver: MatchCount={payload.MatchCount}, Intensity={attackIntensity}"
            );

            return HeroActionExecution.Attack(
                attackVariant,
                attackIntensity
            );
        }

        // private methods

        /// <summary>
        /// Resolves which attack variant the hero should use.
        /// </summary>
        /// <param name="payload">Puzzle match payload.</param>
        /// <returns>Attack variant index.</returns>
        private int ResolveAttackVariant(PuzzleTilesMatchedPayload payload)
        {
            // Temporary rule:
            // Single default attack animation.
            // Can later depend on weapon, class, buffs, etc.
            return 1;
        }

        /// <summary>
        /// Resolves attack intensity based on puzzle match count.
        /// </summary>
        /// <param name="payload">Puzzle match payload.</param>
        /// <returns>Attack intensity.</returns>
        private int ResolveAttackIntensity(PuzzleTilesMatchedPayload payload)
        {
            return ResolveIntensity(payload.MatchCount);
        }
    }
}