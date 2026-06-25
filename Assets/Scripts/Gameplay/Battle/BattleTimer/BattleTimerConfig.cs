using UnityEngine;

namespace MightyHeroes.Battle.BattleTimer
{
    /// <summary>
    /// Per-level configuration for the battle timer.
    ///
    /// When a level references this asset, the timer is active for that battle.
    /// When a level leaves the timer config field empty, no timer is created.
    /// Designers do not need a separate "enable" flag — the presence of the asset
    /// is the enable signal.
    ///
    /// Place one asset per timer profile (e.g. BattleTimer_60s, BattleTimer_30s_Hard)
    /// and reference the appropriate one from each level's configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BattleTimerConfig",
        menuName = "MightyHeroes/Battle/Battle Timer Config")]
    public sealed class BattleTimerConfig : ScriptableObject
    {
        // FIELDS

        [Header("Duration")]
        [Tooltip("Total seconds the hero has to defeat all enemies once the timer starts.")]
        [SerializeField, Min(1f)] private float _durationSeconds = 60f;

        [Header("Start Behaviour")]
        [Tooltip("When enabled, the timer is visible and counting down from the moment the " +
                 "battle begins, without waiting for the hero's first puzzle match.")]
        [SerializeField] private bool _startImmediately;

        [Header("Warning")]
        [Tooltip("When remaining seconds drop to or below this value, the view enters warning " +
                 "mode (color change and scale pulse). Set to 0 to disable warning visuals.")]
        [SerializeField, Min(0f)] private float _warningThresholdSeconds = 10f;

        // PROPERTIES

        /// <summary>Total countdown duration in seconds.</summary>
        public float DurationSeconds => _durationSeconds;

        /// <summary>
        /// When true, the timer starts ticking immediately when the battle begins
        /// rather than waiting for the hero's first puzzle match.
        /// </summary>
        public bool StartImmediately => _startImmediately;

        /// <summary>Threshold at which the view should switch to warning visuals.</summary>
        public float WarningThresholdSeconds => _warningThresholdSeconds;
    }
}