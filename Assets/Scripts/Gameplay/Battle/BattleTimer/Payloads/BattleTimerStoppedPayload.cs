namespace MightyHeroes.Battle.BattleTimer.Payloads
{
    /// <summary>
    /// Raised when the timer is stopped externally before reaching zero
    /// (typically because the battle ended in victory or hero defeat).
    /// </summary>
    public readonly struct BattleTimerStoppedPayload
    {
        // PROPERTIES

        public int RemainingSeconds { get; }

        // CONSTRUCTOR

        public BattleTimerStoppedPayload(int remainingSeconds)
        {
            RemainingSeconds = remainingSeconds;
        }
    }
}