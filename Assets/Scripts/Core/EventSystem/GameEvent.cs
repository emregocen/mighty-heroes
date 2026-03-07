using System;

namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Strongly-typed game event implementation.
    /// Allows publishing and subscribing to events
    /// with a specific payload type.
    /// </summary>
    /// <typeparam name="TPayload">
    /// The payload type carried by this event.
    /// </typeparam>
    public sealed class GameEvent<TPayload>
    {
        // 1) FIELDS

        private event Action<TPayload> _listeners;

        // 4) PUBLIC METHODS

        /// <summary>
        /// Raises the event and notifies all registered listeners.
        /// </summary>
        /// <param name="payload">
        /// The payload data associated with the event.
        /// </param>
        public void Raise(TPayload payload)
        {
            _listeners?.Invoke(payload);
        }

        /// <summary>
        /// Registers a listener callback for this event.
        /// </summary>
        /// <param name="listener">
        /// The callback to invoke when the event is raised.
        /// </param>
        public void Register(Action<TPayload> listener)
        {
            _listeners += listener;
        }

        /// <summary>
        /// Unregisters a previously registered listener callback.
        /// </summary>
        /// <param name="listener">
        /// The callback to remove from the listener list.
        /// </param>
        public void Unregister(Action<TPayload> listener)
        {
            _listeners -= listener;
        }
    }
}