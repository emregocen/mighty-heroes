using System;
using System.Collections.Generic;

namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Caches and manages game event instances by payload type.
    /// Ensures a single GameEvent instance exists per payload.
    /// </summary>
    internal sealed class EventRegistry : IEventRegistry
    {
        private readonly Dictionary<Type, object> _events;

        public EventRegistry()
        {
            _events = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Returns the game event instance for the given payload type.
        /// Creates and caches it if it does not exist yet.
        /// </summary>
        public GameEvent<TPayload> GetEvent<TPayload>()
        {
            var payloadType = typeof(TPayload);

            if (_events.TryGetValue(payloadType, out var existingEvent))
            {
                return (GameEvent<TPayload>)existingEvent;
            }

            var newEvent = new GameEvent<TPayload>();
            _events.Add(payloadType, newEvent);

            return newEvent;
        }
    }
}