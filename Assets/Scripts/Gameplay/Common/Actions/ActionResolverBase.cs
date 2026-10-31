namespace MightyHeroes.Gameplay.Common.Actions
{
    /// <summary>
    /// Base class for resolving actions for any combat-capable entity.
    /// Contains shared decision logic such as match count to tier resolution.
    /// </summary>
    /// <typeparam name="TInput">Input data required to resolve the action.</typeparam>
    /// <typeparam name="TResult">Resolved action output.</typeparam>
    public abstract class ActionResolverBase<TInput, TResult>
    {
        // PUBLIC METHODS

        /// <summary>
        /// Resolves the given input into an action result.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <returns>Resolved action result.</returns>
        public abstract TResult Resolve(TInput input);

        // PROTECTED METHODS

        /// <summary>
        /// Converts a matched tile count into an action tier (1..ActionTierData.TierCount)
        /// using the thresholds defined in the given tier data.
        /// Any count below Tier2MinMatch resolves to Tier 1.
        /// </summary>
        /// <param name="matchCount">Number of matched tiles.</param>
        /// <param name="tierData">Tier threshold configuration.</param>
        /// <returns>Resolved action tier.</returns>
        protected int ResolveTier(int matchCount, ActionTierData tierData)
        {
            if (matchCount >= tierData.Tier3MinMatch)
                return 3;

            if (matchCount >= tierData.Tier2MinMatch)
                return 2;

            return 1;
        }
    }
}
