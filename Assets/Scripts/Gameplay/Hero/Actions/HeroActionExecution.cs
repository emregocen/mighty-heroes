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
        // PROPERTIES

        /// <summary>High-level action type resolved from the puzzle match.</summary>
        public ActionType ActionType { get; }

        /// <summary>Animation variant index.</summary>
        public int Variant { get; }

        /// <summary>
        /// Action tier (1..ActionTierData.TierCount) resolved from match count.
        /// For attacks this is the combo step count.
        /// </summary>
        public int ActionIntensity { get; }

        /// <summary>Damage type for attack actions. Physical for non-attack actions.</summary>
        public DamageType DamageType { get; }

        /// <summary>
        /// Resolved effect amount for support actions (heal amount, shield amount,
        /// buff percent). Zero for attack and none actions.
        /// </summary>
        public int EffectValue { get; }

        // CONSTRUCTOR

        private HeroActionExecution(ActionType actionType, int variant, int actionIntensity, DamageType damageType, int effectValue)
        {
            ActionType = actionType;
            Variant = variant;
            ActionIntensity = actionIntensity;
            DamageType = damageType;
            EffectValue = effectValue;
        }

        // PUBLIC METHODS

        /// <summary>Creates an execution that performs no action.</summary>
        public static HeroActionExecution None()
        {
            return new HeroActionExecution(ActionType.None, 0, 0, DamageType.Physical, 0);
        }

        /// <summary>Creates an attack execution.</summary>
        /// <param name="attackVariant">Attack animation variant.</param>
        /// <param name="actionIntensity">Action tier, used as combo step count.</param>
        /// <param name="damageType">Damage type of the attack.</param>
        public static HeroActionExecution Attack(int attackVariant, int actionIntensity, DamageType damageType)
        {
            return new HeroActionExecution(ActionType.Attack, attackVariant, actionIntensity, damageType, 0);
        }

        /// <summary>Creates a self-targeted support execution (Heal, Shield, Buff).</summary>
        /// <param name="supportType">Support action type.</param>
        /// <param name="variant">Support animation variant.</param>
        /// <param name="actionIntensity">Action tier.</param>
        /// <param name="effectValue">Resolved effect amount for the tier.</param>
        public static HeroActionExecution Support(ActionType supportType, int variant, int actionIntensity, int effectValue)
        {
            return new HeroActionExecution(supportType, variant, actionIntensity, DamageType.Physical, effectValue);
        }
    }
}
