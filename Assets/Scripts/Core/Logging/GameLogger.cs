using System.Diagnostics;

namespace MightyHeroes.Core.Logging
{
    /// <summary>
    /// Centralized logging utility used to trace gameplay flow and system behavior.
    ///
    /// Responsibilities:
    /// - Enforce consistent log formatting
    /// - Control per-system log filtering
    /// - Support ordered gameplay flow verification
    /// - Strip non-critical logs from release builds
    /// </summary>
    public static class GameLogger
    {
        // 0) CONSTANTS

        /// <summary>
        /// Standard log output format applied to all log entries.
        /// </summary>
        private const string LOG_FORMAT = "{0}|{1}| {2}";

        // 1) FIELDS

        /// <summary>
        /// Enables or disables flow-related logs globally.
        /// Used to verify gameplay order and state transitions.
        /// </summary>
        public static bool EnableFlowLogs = true;

        /// <summary>
        /// Enables or disables debug-level logs.
        /// Intended for verbose diagnostic output.
        /// </summary>
        public static bool EnableDebugLogs = true;

        /// <summary>
        /// Enables or disables warning and error logs.
        /// </summary>
        public static bool EnableErrorLogs = true;

        /// <summary>
        /// Enables logs originating from hero-related systems.
        /// </summary>
        public static bool EnableHeroLogs = true;

        /// <summary>
        /// Enables logs originating from enemy-related systems.
        /// </summary>
        public static bool EnableEnemyLogs = true;

        /// <summary>
        /// Enables logs originating from turn management systems.
        /// </summary>
        public static bool EnableTurnLogs = true;

        /// <summary>
        /// Enables logs originating from battle flow systems.
        /// </summary>
        public static bool EnableBattleLogs = true;

        /// <summary>
        /// Enables logs originating from puzzle systems.
        /// </summary>
        public static bool EnablePuzzleLogs = true;

        /// <summary>
        /// Enables logs originating from combat resolution systems.
        /// </summary>
        public static bool EnableCombatLogs = true;

        /// <summary>
        /// Enables logs originating from state machine systems.
        /// </summary>
        public static bool EnableStateMachineLogs = true;

        /// <summary>
        /// Enables logs originating from animation systems.
        /// </summary>
        public static bool EnableAnimationLogs = true;

        // 3) CONSTRUCTOR
        // None

        // 4) PUBLIC METHODS

        /// <summary>
        /// Writes a flow-level log entry for the specified source.
        /// Used to validate gameplay order and state transitions.
        /// </summary>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <param name="message">The message to be logged.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(LogSourceType sourceType, string message)
        {
            if (!EnableFlowLogs || !IsSourceEnabled(sourceType))
                return;

            UnityEngine.Debug.Log(Format("LOG", sourceType, message));
        }

        /// <summary>
        /// Writes a debug-level log entry for the specified source.
        /// Intended for detailed diagnostic information.
        /// </summary>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <param name="message">The message to be logged.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(LogSourceType sourceType, string message)
        {
            if (!EnableDebugLogs || !IsSourceEnabled(sourceType))
                return;

            UnityEngine.Debug.Log(Format("DBG", sourceType, message));
        }

        /// <summary>
        /// Writes a warning-level log entry for the specified source.
        /// Indicates non-critical issues that require attention.
        /// </summary>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <param name="message">The warning message.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(LogSourceType sourceType, string message)
        {
            if (!EnableErrorLogs || !IsSourceEnabled(sourceType))
                return;

            UnityEngine.Debug.LogWarning(Format("WRN", sourceType, message));
        }

        /// <summary>
        /// Writes an error-level log entry for the specified source.
        /// Used for critical failures that must be visible in all builds.
        /// </summary>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <param name="message">The error message.</param>
        public static void Error(LogSourceType sourceType, string message)
        {
            if (!EnableErrorLogs || !IsSourceEnabled(sourceType))
                return;

            UnityEngine.Debug.LogError(Format("ERR", sourceType, message));
        }

        // 6) PRIVATE METHODS

        /// <summary>
        /// Determines whether logs from the given source type are currently enabled.
        /// </summary>
        /// <param name="sourceType">The system category to check.</param>
        /// <returns>True if logs from this source are enabled; otherwise false.</returns>
        private static bool IsSourceEnabled(LogSourceType sourceType)
        {
            return sourceType switch
            {
                LogSourceType.Hero => EnableHeroLogs,
                LogSourceType.Enemy => EnableEnemyLogs,
                LogSourceType.TurnSystem => EnableTurnLogs,
                LogSourceType.BattleSystem => EnableBattleLogs,
                LogSourceType.PuzzleSystem => EnablePuzzleLogs,
                LogSourceType.CombatSystem => EnableCombatLogs,
                LogSourceType.StateMachine => EnableStateMachineLogs,
                LogSourceType.Animation => EnableAnimationLogs,
                LogSourceType.System => true,
                _ => true
            };
        }

        /// <summary>
        /// Formats a log entry using the standard log format.
        /// </summary>
        /// <param name="level">The log level identifier.</param>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <param name="message">The message content.</param>
        /// <returns>A formatted log string.</returns>
        private static string Format(string level, LogSourceType sourceType, string message)
        {
            return string.Format(LOG_FORMAT, level, GetPrefix(sourceType), message);
        }

        /// <summary>
        /// Returns the prefix string associated with the given source type.
        /// Used to visually group logs in the console.
        /// </summary>
        /// <param name="sourceType">The system category emitting the log.</param>
        /// <returns>The prefix string.</returns>
        private static string GetPrefix(LogSourceType sourceType)
        {
            return sourceType switch
            {
                LogSourceType.Hero => "PLAYER",
                LogSourceType.Enemy => "ENEMY ",
                LogSourceType.TurnSystem => "TURN  ",
                LogSourceType.BattleSystem => "BATTLE",
                LogSourceType.PuzzleSystem => "PUZZLE",
                LogSourceType.CombatSystem => "COMBAT",
                LogSourceType.StateMachine => "FSM   ",
                LogSourceType.Animation => "ANIM  ",
                LogSourceType.System => "SYSTEM",
                _ => "LOG"
            };
        }
    }
}