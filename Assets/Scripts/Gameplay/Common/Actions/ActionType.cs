namespace MightyHeroes.Gameplay.Common.Actions
{
    /// <summary>
    /// Defines the high-level hero action types produced by puzzle resolution.
    /// </summary>
    public enum ActionType
    {
        /// <summary>Indicates there is no actionable hero action.</summary>
        None = 0,

        /// <summary>Indicates a physical attack action.</summary>
        Attack = 1,

        /// <summary>Indicates a magical attack action.</summary>
        MagicAttack = 2,

        /// <summary>Indicates a buff action.</summary>
        Buff = 3,

        /// <summary>Indicates a heal action.</summary>
        Heal = 4,

        /// <summary>Indicates a shield action that grants absorb shield to the hero.</summary>
        Shield = 5,

        /// <summary>Indicates a special action.</summary>
        Special = 6,

        /// <summary>Indicates no action should be taken for this match.</summary>
        Skip = 7

    }
}