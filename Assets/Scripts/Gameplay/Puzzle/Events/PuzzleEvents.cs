using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Puzzle.Payloads;

namespace MightyHeroes.Gameplay.Puzzle.Events
{
    /// <summary>
    /// Provides access to all puzzle-related game events.
    /// Acts as a semantic wrapper over the global event registry.
    /// </summary>
    public sealed class PuzzleEvents : EventGroupBase
    {
        // 3) CONSTRUCTOR

        /// <summary>
        /// Creates the puzzle events group using the shared event registry.
        /// </summary>
        public PuzzleEvents(IEventRegistry registry)
            : base(registry)
        {
        }

        // 2) PROPERTIES

        /// <summary>
        /// Raised when a puzzle tile match is resolved.
        /// </summary>
        public GameEvent<PuzzleTilesMatchedPayload> PuzzleTilesMatchedEvent =>
            Registry.GetEvent<PuzzleTilesMatchedPayload>();
    }
}