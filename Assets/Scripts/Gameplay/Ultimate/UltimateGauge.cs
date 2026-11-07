using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Ultimate.Payloads;

namespace MightyHeroes.Gameplay.Ultimate
{
    /// <summary>
    /// Plain C# runtime model of one entity's ultimate gauge.
    ///
    /// Fills by matched affinity tile count, clamped to capacity, and empties
    /// when the ultimate fires. Raises UltimateGaugeChangedEvent on every change
    /// so the HUD can react without coupling. Owned by HeroComponent.
    /// </summary>
    public sealed class UltimateGauge
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly int _entityId;
        private readonly UltimateData _data;
        private int _currentValue;

        // PROPERTIES

        /// <summary>Rules of the ultimate this gauge charges.</summary>
        public UltimateData Data => _data;

        /// <summary>Current gauge value.</summary>
        public int CurrentValue => _currentValue;

        /// <summary>Gauge capacity.</summary>
        public int Capacity => _data.GaugeCapacity;

        /// <summary>True when the next affinity match fires the ultimate.</summary>
        public bool IsFull => _currentValue >= Capacity;

        // CONSTRUCTOR

        /// <summary>
        /// Creates an empty gauge for the given entity and ultimate rules.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="entityId">Owner entity runtime id.</param>
        /// <param name="data">Ultimate rules. Must not be null.</param>
        public UltimateGauge(EventHub eventHub, int entityId, UltimateData data)
        {
            _eventHub = eventHub;
            _entityId = entityId;
            _data = data;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Adds the given amount, clamped to capacity. No event when nothing changes.
        /// </summary>
        /// <param name="amount">Matched affinity tile count.</param>
        public void Fill(int amount)
        {
            if (amount <= 0)
                return;

            SetValue(Math.Min(_currentValue + amount, Capacity));
        }

        /// <summary>Empties the gauge after the ultimate fires.</summary>
        public void Consume()
        {
            SetValue(0);
        }

        // PRIVATE METHODS

        private void SetValue(int value)
        {
            if (value == _currentValue)
                return;

            int previous = _currentValue;
            _currentValue = value;

            _eventHub.HeroEvents.UltimateGaugeChangedEvent.Raise(new UltimateGaugeChangedPayload(_entityId, previous, _currentValue, Capacity));
        }
    }
}
