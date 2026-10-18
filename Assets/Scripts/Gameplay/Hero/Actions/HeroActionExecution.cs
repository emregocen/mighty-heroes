using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Hero.Actions
{
    /// <summary>
    /// Represents a resolved hero action to be executed by the hero state machine.
    /// This is a pure data model and does not contain Unity dependencies.
    /// </summary>
    public sealed class HeroActionExecution
    {
        public ActionType ActionType { get; }
        public int Variant { get; }
        public int ActionIntensity { get; }
        public DamageType DamageType { get; }

        private HeroActionExecution(ActionType actionType, int variant, int actionIntensity, DamageType damageType)
        {
            ActionType = actionType;
            Variant = variant;
            ActionIntensity = actionIntensity;
            DamageType = damageType;
        }

        public static HeroActionExecution None()
        {
            return new HeroActionExecution(ActionType.None, 0, 0, DamageType.Physical);
        }

        public static HeroActionExecution Attack(int attackVariant, int actionIntensity, DamageType damageType)
        {
            return new HeroActionExecution(ActionType.Attack, attackVariant, actionIntensity, damageType);
        }

        public static HeroActionExecution Heal(int healVariant, int actionIntensity)
        {
            return new HeroActionExecution(ActionType.Heal, healVariant, actionIntensity, DamageType.Physical);
        }
    }
}