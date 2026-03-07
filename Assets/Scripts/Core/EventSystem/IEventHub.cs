namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Provides access to global game events.
    /// Acts as the single entry point to the event system.
    /// </summary>
    public interface IEventHub
    {
        /// <summary>
        /// Returns the global game event instance associated
        /// with the given payload type.
        /// </summary>
        /// <typeparam name="TPayload">
        /// The payload type that uniquely identifies the event.
        /// </typeparam>
        GameEvent<TPayload> GetEvent<TPayload>();
    }
}