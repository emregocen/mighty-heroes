

using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Puzzle.Payloads
{
    /// <summary>
    /// Payload describing the result of a resolved puzzle tile match action.
    /// Represents an aggregated outcome of multiple matched tiles.
    /// </summary>
    public sealed class PuzzleTilesMatchedPayload
    {
        // 2) PROPERTIES

        /// <summary>
        /// Type of the matched tiles.
        /// </summary>
        public ActionType ActionType { get; }

        /// <summary>
        /// Total number of tiles matched in a single action.
        /// </summary>
        public int MatchCount { get; }

        // 3) CONSTRUCTOR

        public PuzzleTilesMatchedPayload(ActionType actionType, int matchCount)
        {
            ActionType = actionType;
            MatchCount = matchCount;
        }
    }
}