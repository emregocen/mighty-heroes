namespace MightyHeroes.Core.TurnSystem.Types
{
    /// <summary>
    /// Defines all possible turn phases.
    /// </summary>
    public enum TurnStateType
    {
        None,
        TurnStart,
        TurnHero,
        TurnCompanion,
        TurnEnemy,
        TurnEnd
    }
}