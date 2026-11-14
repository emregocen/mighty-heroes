namespace MightyHeroes.Gameplay.Ultimate.Payloads
{
    /// <summary>
    /// Raised by UltimateGauge whenever its value changes (filled or consumed).
    /// </summary>
    public readonly struct UltimateGaugeChangedPayload
    {
        // PROPERTIES

        /// <summary>Entity that owns the gauge.</summary>
        public int EntityId { get; }

        /// <summary>Gauge value before the change.</summary>
        public int PreviousValue { get; }

        /// <summary>Gauge value after the change.</summary>
        public int CurrentValue { get; }

        /// <summary>Gauge capacity.</summary>
        public int Capacity { get; }

        /// <summary>True when the gauge is full after the change.</summary>
        public bool IsFull => CurrentValue >= Capacity;

        /// <summary>True when this change filled a previously not-full gauge.</summary>
        public bool BecameFull => IsFull && PreviousValue < Capacity;

        // CONSTRUCTOR

        /// <summary>
        /// Creates an ultimate gauge changed payload.
        /// </summary>
        public UltimateGaugeChangedPayload(int entityId, int previousValue, int currentValue, int capacity)
        {
            EntityId = entityId;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
            Capacity = capacity;
        }
    }
}
