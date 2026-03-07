namespace MightyHeroes.Gameplay.Common.Actions
{
    /// <summary>
    /// Base class for resolving actions for any combat-capable entity.
    /// Contains shared decision logic such as intensity calculation.
    /// </summary>
    /// <typeparam name="TInput">Input data required to resolve the action.</typeparam>
    /// <typeparam name="TResult">Resolved action output.</typeparam>
    public abstract class ActionResolverBase<TInput, TResult>
    {
        /// <summary>
        /// Resolves the given input into an action result.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <returns>Resolved action result.</returns>
        public abstract TResult Resolve(TInput input);

        /// <summary>
        /// Converts a magnitude value into a normalized action intensity.
        /// </summary>
        /// <param name="value">Raw magnitude value.</param>
        /// <returns>Normalized intensity.</returns>
        protected int ResolveIntensity(int value)
        {
            int result = 3;

            if (value < 5)
                result = 2;

            if (value < 3)
                result = 1;

            return result;
        }
    }
}