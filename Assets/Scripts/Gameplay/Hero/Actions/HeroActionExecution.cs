
using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Hero.Actions
{
    /// <summary>
    /// Represents a resolved hero action to be executed by the hero state machine.
    /// This is a pure data model and does not contain Unity dependencies.
    /// </summary>
    public sealed class HeroActionExecution
    {
        /// <summary>
        /// Gets the resolved action type.
        /// </summary>
        public ActionType ActionType { get; }

        /// <summary>
        /// Gets the attack variant selection value.
        /// Meaning is defined by the animator controller.
        /// </summary>
        public int AttackVariant { get; }

        /// <summary>
        /// Gets how much intensity attack should be executed for this action.
        /// </summary>
        public int ActionIntensity { get; }

        private HeroActionExecution(ActionType actionType, int attackVariant, int actionIntensity)
        {
            ActionType = actionType;
            AttackVariant = attackVariant;
            ActionIntensity = actionIntensity;
        }

        /// <summary>
        /// Creates an empty execution result.
        /// </summary>
        /// <returns>A no-op action execution.</returns>
        public static HeroActionExecution None()
        {
            return new HeroActionExecution(ActionType.None, 0, 0);
        }

        /// <summary>
        /// Creates an attack execution result.
        /// </summary>
        /// <param name="attackVariant">Attack variant value.</param>
        /// <param name="actionIntensity">Total action intensity.</param>
        /// <returns>An attack action execution.</returns>
        public static HeroActionExecution Attack(int attackVariant, int actionIntensity)
        {
            return new HeroActionExecution(ActionType.Attack, attackVariant, actionIntensity);
        }
    }
}