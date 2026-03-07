using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine.Events;

namespace MightyHeroes.Gameplay.Puzzle.Core
{
    /// <summary>
    ///     UnityEvent with integer argument
    /// </summary>
    public class UnityIntEvent : UnityEvent<int>
    {
    }

    public class ElementDespawnedEvent : UnityEvent<ActionType, int>
    {
    }

    /// <summary>
    ///     UnityEvent with two integer arguments, used to broadcast changes to the score
    /// </summary>
    public class ScoreChangedEvent : UnityEvent<int, int>
    {
    }

    /// <summary>
    ///     Static class holding all relevant game events
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        ///     Event that is fired when bubbles are despawned
        /// </summary>
        public static ElementDespawnedEvent OnElementsDespawned { get; } = new();

        /// <summary>
        ///     Event that is fired when the selection of bubbles has changed
        /// </summary>
        public static UnityIntEvent OnSelectionChanged { get; } = new();

        /// <summary>
        ///     Event that is fired when the score has changed
        /// </summary>
        public static ScoreChangedEvent OnScoreChanged { get; } = new();
    }
}