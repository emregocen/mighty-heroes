namespace MightyHeroes.Gameplay.Common.Actions
{
    /// <summary>
    /// Defines the high-level hero action types produced by puzzle resolution.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Indicates there is no actionable hero action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates an attack action.
        /// </summary>
        Attack = 1,

        /// <summary>
        /// Indicates a buff action.
        /// </summary>
        Buff = 2,

        /// <summary>
        /// Indicates a heal action.
        /// </summary>
        Heal = 3,

        /// <summary>
        /// Indicates a defense action.
        /// </summary>
        Defense = 4,

        /// <summary>
        /// Indicates a special action.
        /// </summary>
        Special = 5,
        
        /// <summary>
        /// 
        /// </summary>
        Skip=6
    }
}