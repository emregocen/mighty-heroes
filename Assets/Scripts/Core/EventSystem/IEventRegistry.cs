namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Defines a registry that returns cached game event instances by payload type.
    /// </summary>
    public interface IEventRegistry
    {
        /// <summary>
        /// Returns the game event instance for the given payload type.
        /// Creates and caches it if it does not exist yet.
        /// </summary>
        GameEvent<TPayload> GetEvent<TPayload>();
    }
}