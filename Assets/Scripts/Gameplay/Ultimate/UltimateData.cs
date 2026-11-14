using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Ultimate
{
    /// <summary>
    /// Gameplay rules of a hero's ultimate: affinity, gauge capacity, AP cost,
    /// targeting and damage. Referenced by HeroStatsData so each hero class can
    /// own a different ultimate. Presentation tuning (camera, slow motion) lives
    /// in CombatFeedbackData instead.
    ///
    /// Flow: matching affinity tiles fills the gauge by the matched tile count.
    /// While the gauge is full, the next affinity match fires the ultimate
    /// instead of its regular action and empties the gauge.
    ///
    /// Affinity is expressed as the ActionType assigned to the affinity color in
    /// the puzzle configuration, because the puzzle reports matches by action
    /// type. All numbers are placeholders until the balancing pass.
    /// </summary>
    [CreateAssetMenu(fileName = "UltimateData", menuName = "Mighty Heroes/Gameplay/Ultimate Data")]
    public sealed class UltimateData : ScriptableObject
    {
        // FIELDS

        [Header("Affinity")]
        [Tooltip("Action type of the affinity color. Matching these tiles fills the gauge and, when full, fires the ultimate.")]
        [SerializeField] private ActionType _affinityActionType = ActionType.Attack;

        [Header("Gauge")]
        [Tooltip("Number of matched affinity tiles needed to fill the gauge.")]
        [SerializeField, Min(1)] private int _gaugeCapacity = 20;

        [Header("Rules")]
        [Tooltip("Action points consumed when the ultimate fires.")]
        [SerializeField] private UltimateApCostMode _apCostMode = UltimateApCostMode.Free;

        [Tooltip("Enemies hit by the ultimate.")]
        [SerializeField] private UltimateTargetMode _targetMode = UltimateTargetMode.FrontLine;

        [Header("Damage")]
        [Tooltip("Multiplier applied to the regular attack damage of the triggering match tier, per hit target.")]
        [SerializeField, Min(0f)] private float _damageMultiplier = 2.5f;

        // PROPERTIES

        /// <summary>Action type of the affinity color.</summary>
        public ActionType AffinityActionType => _affinityActionType;

        /// <summary>Matched affinity tiles needed to fill the gauge.</summary>
        public int GaugeCapacity => _gaugeCapacity;

        /// <summary>Action points consumed when the ultimate fires.</summary>
        public UltimateApCostMode ApCostMode => _apCostMode;

        /// <summary>Enemies hit by the ultimate.</summary>
        public UltimateTargetMode TargetMode => _targetMode;

        /// <summary>Damage multiplier over the regular attack damage of the triggering tier.</summary>
        public float DamageMultiplier => _damageMultiplier;
    }
}
