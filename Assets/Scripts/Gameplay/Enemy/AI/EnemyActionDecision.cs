using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Enemy.AI
{
    /// <summary>
    /// Represents the resolved decision of an enemy action.
    /// Immutable data produced by the AI layer.
    /// </summary>
    public sealed class EnemyActionDecision
    {
        /// <summary>
        /// Type of action the enemy has decided to perform.
        /// </summary>
        public ActionType ActionType { get; }

        /// <summary>
        /// Attack animation variant to be used.
        /// </summary>
        public int AttackVariant { get; }

        /// <summary>
        /// Attack intensity defining combo length.
        /// </summary>
        public int AttackIntensity { get; }

        public EnemyActionDecision(
            ActionType actionType,
            int attackVariant,
            int attackIntensity)
        {
            ActionType = actionType;
            AttackVariant = attackVariant;
            AttackIntensity = attackIntensity;
        }

        /// <summary>
        /// Creates an attack action decision.
        /// </summary>
        public static EnemyActionDecision Attack(
            int attackVariant,
            int attackIntensity)
        {
            return new EnemyActionDecision(
                ActionType.Attack,
                attackVariant,
                attackIntensity
            );
        }
    }
}