using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Enemy.AI
{
    /// <summary>
    /// Resolves enemy actions based on enemy state and AI rules.
    /// </summary>
    public sealed class EnemyActionResolver
        : ActionResolverBase<EnemyComponent, EnemyActionDecision>
    {
        /// <summary>
        /// Resolves the next enemy action decision.
        /// </summary>
        /// <param name="enemy">Enemy component.</param>
        /// <returns>Resolved enemy action decision.</returns>
        public override EnemyActionDecision Resolve(EnemyComponent enemy)
        {
            if (enemy == null)
                return null;

            int attackVariant = ResolveAttackVariant(enemy);
            int intensity = ResolveAttackIntensity(enemy);

            return EnemyActionDecision.Attack(attackVariant, intensity);
        }

        /// <summary>
        /// Resolves which attack variant the enemy should use.
        /// </summary>
        /// <param name="enemy">Enemy component.</param>
        /// <returns>Attack variant index.</returns>
        private int ResolveAttackVariant(EnemyComponent enemy)
        {
            return 1;
        }

        /// <summary>
        /// Resolves attack intensity for the enemy.
        /// </summary>
        /// <param name="enemy">Enemy component.</param>
        /// <returns>Attack intensity.</returns>
        private int ResolveAttackIntensity(EnemyComponent enemy)
        {
            return 1;
        }
    }
}