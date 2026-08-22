using MightyHeroes.Battle.BattleTimer.Payloads;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Puzzle.Payloads;
using System;

namespace MightyHeroes.Battle.BattleTimer
{
    /// <summary>
    /// Battle-scoped countdown timer.
    ///
    /// Owns its remaining seconds and pause state. Does not own a tick loop —
    /// BattleTimerRunner drives it via Tick on Update. Communicates exclusively
    /// through the EventHub.
    ///
    /// Lifecycle: created in idle state by BattleBootstrapper. Starts on the first
    /// PuzzleTilesMatchedEvent. Ticks until either Expired (raises ExpiredEvent)
    /// or externally stopped via Stop (raises StoppedEvent).
    /// </summary>
    public sealed class BattleTimer : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly BattleTimerConfig _config;

        private float _remainingSecondsExact;
        private int _lastBroadcastSeconds;
        private int _pauseCounter;
        private bool _isRunning;
        private bool _isExpired;
        private bool _isDisposed;
        private bool _isIntroComplete;

        // PROPERTIES

        /// <summary>True after Start has been called and the timer has not yet expired or stopped.</summary>
        public bool IsRunning => _isRunning;

        /// <summary>Remaining seconds as a precise float (used internally and by tests).</summary>
        public float RemainingSecondsExact => _remainingSecondsExact;

        /// <summary>Current displayed seconds (ceil of remaining). Available before Start is called.</summary>
        public int RemainingSeconds => _lastBroadcastSeconds;

        /// <summary>True if any pause source is currently holding the timer.</summary>
        public bool IsPaused => _pauseCounter > 0;

        // CONSTRUCTOR

        /// <summary>
        /// Constructs the timer. If StartImmediately is set in the config, the timer begins ticking right away.
        /// Otherwise it waits for the first PuzzleTilesMatchedEvent.
        /// </summary>
        public BattleTimer(EventHub eventHub, BattleTimerConfig config)
        {
            _eventHub = eventHub;
            _config = config;
            _remainingSecondsExact = config.DurationSeconds;
            _lastBroadcastSeconds = CeilToInt(_remainingSecondsExact);
        }
        // PUBLIC METHODS

        /// <summary>
        /// Disposes the timer. Unregisters any remaining event subscriptions.
        /// Safe to call multiple times.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.PuzzleEvents.PuzzleTilesMatchedEvent.Unregister(OnFirstPuzzleMatched);
            _isDisposed = true;
        }

        /// <summary>
        /// Advances the timer by deltaTime. No-op if not running, paused, or already expired.
        /// Raises RemainingSecondsChanged whenever the displayed second changes.
        /// Raises Expired when remaining hits zero.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!_isRunning || _isExpired || IsPaused)
                return;

            _remainingSecondsExact -= deltaTime;

            if (_remainingSecondsExact <= 0f)
            {
                _remainingSecondsExact = 0f;
                BroadcastRemainingIfChanged();
                ExpireInternal();
                return;
            }

            BroadcastRemainingIfChanged();
        }

        /// <summary>
        /// Stops the timer externally without expiring it. Called by BattleFlowController
        /// when the battle ends in victory or hero defeat. Raises StoppedEvent.
        /// No-op if not running.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning || _isExpired)
                return;

            _isRunning = false;

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer.Stop remaining={_lastBroadcastSeconds}");

            _eventHub.BattleTimerEvents.StoppedEvent.Raise(
                new BattleTimerStoppedPayload(_lastBroadcastSeconds));
        }

        /// <summary>
        /// Holds the timer until a matching Resume is called.
        /// Multiple pause sources stack via an internal counter, so a buff and a
        /// menu can both pause simultaneously and the timer only resumes once all
        /// have been released.
        /// </summary>
        public void PauseUntilResumed()
        {
            _pauseCounter++;

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer.PauseUntilResumed pauseCount={_pauseCounter}");
        }

        /// <summary>
        /// Releases one pause hold. The timer resumes ticking when the counter reaches zero.
        /// Safe to call when not paused — the counter floors at zero.
        /// </summary>
        public void Resume()
        {
            if (_pauseCounter <= 0)
                return;

            _pauseCounter--;

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer.Resume pauseCount={_pauseCounter}");
        }

        /// <summary>
        /// Adds seconds to the remaining time. Used by time-extension buffs.
        /// Does not start the timer if it is idle and does not revive it if expired.
        /// </summary>
        public void AddTime(float extraSeconds)
        {
            if (_isExpired || extraSeconds <= 0f)
                return;

            _remainingSecondsExact += extraSeconds;
            BroadcastRemainingIfChanged();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer.AddTime added={extraSeconds:F1} newRemaining={_remainingSecondsExact:F1}");
        }

        /// <summary>
        /// Called by BattleBootstrapper once the intro sequence finishes.
        /// Arms the timer's start trigger — either immediately (StartImmediately)
        /// or on the first puzzle match, whichever the config specifies.
        /// </summary>
        public void NotifyIntroCompleted()
        {
            if (_config.StartImmediately)
            {
                StartInternal();
            }
            else
            {
                _eventHub.PuzzleEvents.PuzzleTilesMatchedEvent.Register(OnFirstPuzzleMatched);
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Starts the countdown. Raises StartedEvent and immediately broadcasts the
        /// initial remaining value so the UI shows full duration on the first frame.
        /// </summary>
        private void StartInternal()
        {
            if (_isRunning || _isExpired)
                return;

            _isRunning = true;

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleTimer.Start duration={_config.DurationSeconds}");

            _eventHub.BattleTimerEvents.StartedEvent.Raise(
                new BattleTimerStartedPayload(_config.DurationSeconds, _lastBroadcastSeconds));

            BroadcastRemainingForced();
        }

        /// <summary>
        /// Marks the timer expired and raises ExpiredEvent.
        /// BattleFlowController listens for this and triggers the defeat flow.
        /// </summary>
        private void ExpireInternal()
        {
            _isExpired = true;
            _isRunning = false;

            GameLogger.Log(LogSourceType.BattleSystem, "BattleTimer.Expired");

            _eventHub.BattleTimerEvents.ExpiredEvent.Raise(new BattleTimerExpiredPayload());
        }

        /// <summary>
        /// Raises RemainingSecondsChanged only if the displayed (ceil) second
        /// has changed since the last broadcast. Avoids per-frame UI updates.
        /// </summary>
        private void BroadcastRemainingIfChanged()
        {
            int current = CeilToInt(_remainingSecondsExact);

            if (current == _lastBroadcastSeconds)
                return;

            _lastBroadcastSeconds = current;
            BroadcastRemainingForced();
        }

        /// <summary>
        /// Unconditionally raises RemainingSecondsChanged. Used on Start and AddTime
        /// to push the new value even when the integer second has not changed.
        /// </summary>
        private void BroadcastRemainingForced()
        {
            bool isInWarning = _remainingSecondsExact <= _config.WarningThresholdSeconds;

            _eventHub.BattleTimerEvents.RemainingSecondsChangedEvent.Raise(
                new BattleTimerRemainingChangedPayload(
                    _lastBroadcastSeconds,
                    _remainingSecondsExact,
                    isInWarning));
        }

        private static int CeilToInt(float value)
        {
            return value <= 0f ? 0 : (int)Math.Ceiling(value);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Subscribed once at construction. Fires Start on the first puzzle match
        /// and then unregisters itself so subsequent matches do not retrigger Start.
        /// </summary>
        private void OnFirstPuzzleMatched(PuzzleTilesMatchedPayload payload)
        {
            _eventHub.PuzzleEvents.PuzzleTilesMatchedEvent.Unregister(OnFirstPuzzleMatched);
            StartInternal();
        }
    }
}