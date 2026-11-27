using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.TurnFlow.Payloads;

namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Records the outcome of a won battle. On TurnBattleVictoryEvent it rates
    /// the win by turns taken, saves the best stars for the level and unlocks
    /// the next level. The result screen reads LastResult afterwards.
    /// Created and disposed by BattleBootstrapper.
    /// </summary>
    public sealed class LevelResultTracker : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly LevelCatalogData _levelCatalog;
        private readonly LevelData _level;
        private readonly int _levelIndex;

        private bool _isDisposed;

        // PROPERTIES

        /// <summary>True once a victory has been recorded in this battle.</summary>
        public bool HasResult { get; private set; }

        /// <summary>Result of the won battle. Valid when HasResult is true.</summary>
        public LevelResult LastResult { get; private set; }

        // CONSTRUCTOR

        /// <summary>Creates the tracker and starts listening for victory.</summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="levelCatalog">Catalog of the current level; null disables unlocking.</param>
        /// <param name="level">Level being played; null uses default star limits.</param>
        /// <param name="levelIndex">Catalog index of the level, or -1 when no level is used.</param>
        public LevelResultTracker(EventHub eventHub, LevelCatalogData levelCatalog, LevelData level, int levelIndex)
        {
            _eventHub = eventHub;
            _levelCatalog = levelCatalog;
            _level = level;
            _levelIndex = levelIndex;

            _eventHub.TurnEvents.TurnBattleVictoryEvent.Register(OnBattleVictory);
        }

        // PUBLIC METHODS

        /// <summary>Stops listening for victory.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.TurnEvents.TurnBattleVictoryEvent.Unregister(OnBattleVictory);
            _isDisposed = true;
        }

        // PRIVATE METHODS

        /// <summary>Saves the stars and unlocks the next level when one exists.</summary>
        private void SaveProgress(int stars)
        {
            if (_levelIndex < 0)
                return;

            LevelProgressStore.RecordStars(_levelIndex, stars);

            if (_levelCatalog != null && _levelIndex + 1 < _levelCatalog.Count)
                LevelProgressStore.UnlockLevel(_levelIndex + 1);
        }

        // EVENT HANDLERS

        private void OnBattleVictory(TurnBattleVictoryPayload payload)
        {
            if (HasResult)
                return;

            int turnsTaken = payload.FinalTurnIndex + 1;
            int stars = BattleStarRating.Calculate(_level, turnsTaken);

            LastResult = new LevelResult(turnsTaken, stars);
            HasResult = true;

            SaveProgress(stars);

            GameLogger.Log(LogSourceType.BattleSystem, $"LevelResultTracker: level {_levelIndex} won in {turnsTaken} turns, {stars} stars. Best={LevelProgressStore.GetBestStars(_levelIndex)}");
        }
    }
}
