namespace MightyHeroes.Battle.BattleTimer.Payloads
{
    /// <summary>
    /// Raised whenever the displayed remaining seconds change (floor value, not every frame).
    /// IsInWarningZone is true when remaining is at or below the configured warning threshold.
    /// </summary>
    public readonly struct BattleTimerRemainingChangedPayload
    {
        // PROPERTIES

        public int RemainingSeconds { get; }
        public float RemainingSecondsExact { get; }
        public bool IsInWarningZone { get; }

        // CONSTRUCTOR

        public BattleTimerRemainingChangedPayload(int remainingSeconds, float remainingSecondsExact, bool isInWarningZone)
        {
            RemainingSeconds = remainingSeconds;
            RemainingSecondsExact = remainingSecondsExact;
            IsInWarningZone = isInWarningZone;
        }
    }
}