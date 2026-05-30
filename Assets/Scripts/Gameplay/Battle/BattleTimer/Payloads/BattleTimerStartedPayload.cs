namespace MightyHeroes.Battle.BattleTimer.Payloads
{
    /// <summary>
    /// Raised once when the timer transitions from idle to running.
    /// Carries the total duration so listeners can size their UI.
    /// </summary>
    public readonly struct BattleTimerStartedPayload
    {
        // PROPERTIES

        public float TotalSeconds { get; }
        public int RemainingSeconds { get; }

        // CONSTRUCTOR

        public BattleTimerStartedPayload(float totalSeconds, int remainingSeconds)
        {
            TotalSeconds = totalSeconds;
            RemainingSeconds = remainingSeconds;
        }
    }
}