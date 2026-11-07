namespace MightyHeroes.Gameplay.Ultimate
{
    /// <summary>
    /// How many action points an ultimate consumes when it fires.
    /// </summary>
    public enum UltimateApCostMode
    {
        /// <summary>Free action; filling the gauge is the cost.</summary>
        Free = 0,

        /// <summary>Consumes one action point like a regular action.</summary>
        One = 1,

        /// <summary>Consumes all remaining action points and ends the hero turn.</summary>
        AllRemaining = 2
    }
}
