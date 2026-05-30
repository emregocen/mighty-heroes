namespace MightyHeroes.Battle.BattleTimer.Payloads
{
    /// <summary>
    /// Raised exactly once when the timer reaches zero with the battle still active.
    /// BattleFlowController subscribes to this and triggers the defeat flow.
    /// </summary>
    public readonly struct BattleTimerExpiredPayload
    {
    }
}