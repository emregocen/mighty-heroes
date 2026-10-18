using UnityEngine;
using UnityEngine.Serialization;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// Per-level configuration for the battle intro sequence.
    ///
    /// When assigned in BattleBootstrapper, the intro plays once before the first turn.
    /// When left empty, the battle starts immediately with no intro.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BattleIntroConfig",
        menuName = "Mighty Heroes/Battle/Battle Intro Config")]
    public sealed class BattleIntroConfig : ScriptableObject
    {
        // FIELDS

        [Header("Camera Settle")]
        [Tooltip("Wait time after the intro camera becomes active, before the hero starts walking.")]
        [SerializeField, Min(0f)] private float _cameraSettleDuration = 1f;

        [Header("Hero Walk")]
        [Tooltip("Hero movement speed in world units per second.")]
        [SerializeField, Min(0.01f)] private float _heroWalkSpeed = 1.5f;

        [Header("Taunt")]
        [Tooltip("Extra hold time after the taunt animation ends, before the fast orbit to enemies begins.")]
        [SerializeField, Min(0f)] private float _tauntPostHoldDuration = 0.5f;

        [Header("Camera Orbit — Fast (enemy reveal)")]
        [FormerlySerializedAs("_enemySweepHoldDuration")]
        [Tooltip("Duration of the faster orbit from the taunt-hold pose to the enemy reveal pose.")]
        [SerializeField, Min(0.1f)] private float _fastOrbitDuration = 0.7f;

        [Header("Camera Handoff")]
        [Tooltip("Wait time after swapping to the battle camera, before the intro is considered complete.")]
        [SerializeField, Min(0f)] private float _sweepBackDuration = 0.8f;

        [Header("Puzzle Reveal")]
        [SerializeField, Min(0f)] private float _puzzleRevealLeadTime = 0.4f;
        [SerializeField, Min(0f)] private float _puzzleRevealDuration = 0.35f;

        // PROPERTIES

        public float CameraSettleDuration => _cameraSettleDuration;
        public float HeroWalkSpeed => _heroWalkSpeed;
        public float TauntPostHoldDuration => _tauntPostHoldDuration;
        public float FastOrbitDuration => _fastOrbitDuration;
        public float SweepBackDuration => _sweepBackDuration;
        public float PuzzleRevealLeadTime => _puzzleRevealLeadTime;
        public float PuzzleRevealDuration => _puzzleRevealDuration;
    }
}