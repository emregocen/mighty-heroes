namespace MightyHeroes.Core.Logging
{
    /// <summary>
    /// Identifies the system responsible for emitting a log entry.
    /// Used for filtering and prefixing.
    /// </summary>
    public enum LogSourceType
    {
        Hero,
        Enemy,
        TurnSystem,
        BattleSystem,
        PuzzleSystem,
        CombatSystem,
        StateMachine,
        Animation,
        System,
        UI
    }
}