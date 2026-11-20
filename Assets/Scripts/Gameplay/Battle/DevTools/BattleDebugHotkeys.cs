#if UNITY_EDITOR || DEVELOPMENT_BUILD
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Feedback;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Puzzle.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.DevTools
{
    /// <summary>
    /// Development-only battle hotkeys. Compiled only in the Editor and in
    /// development builds; added at runtime by BattleBootstrapper, so no scene
    /// object or reference is needed.
    ///
    /// L: plays a Tier 3 hero attack on the current target, as if the player had
    ///    matched Tier3MinMatch attack tiles, and forces the kill moment
    ///    (kill-cam and slow motion) on its final hit even if the target
    ///    survives. Works only while the hero is waiting for puzzle input.
    ///    Like a real match it also charges the ultimate gauge, and fires the
    ///    ultimate instead when the gauge is already full.
    /// </summary>
    public sealed class BattleDebugHotkeys : MonoBehaviour
    {
        // CONSTANTS

        private const KeyCode Tier3AttackKey = KeyCode.L;

        // FIELDS

        private EventHub _eventHub;
        private HeroComponent _heroComponent;
        private CombatFeedbackPresenter _combatFeedbackPresenter;
        private int _tier3MatchCount;
        private bool _isInitialized;

        // UNITY CALLBACKS

        private void Update()
        {
            if (_isInitialized && UnityEngine.Input.GetKeyDown(Tier3AttackKey))
                PlayTier3Attack();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Wires the hotkeys into the battle. Called by BattleBootstrapper.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="heroComponent">Hero, used to check that it is waiting for input.</param>
        /// <param name="combatFeedbackPresenter">Feedback presenter whose kill moment is forced.</param>
        /// <param name="tier3MatchCount">Match count that resolves to Tier 3.</param>
        public void Initialize(EventHub eventHub, HeroComponent heroComponent, CombatFeedbackPresenter combatFeedbackPresenter, int tier3MatchCount)
        {
            _eventHub = eventHub;
            _heroComponent = heroComponent;
            _combatFeedbackPresenter = combatFeedbackPresenter;
            _tier3MatchCount = tier3MatchCount;
            _isInitialized = _eventHub != null && _heroComponent != null;
        }

        // PRIVATE METHODS

        /// <summary>
        /// Raises a Tier 3 attack match for the hero and forces the kill moment
        /// on its final hit.
        /// </summary>
        private void PlayTier3Attack()
        {
            if (!_heroComponent.CanChangeTarget)
            {
                GameLogger.Log(LogSourceType.BattleSystem, "BattleDebugHotkeys: hero is not waiting for input, Tier 3 attack ignored.");
                return;
            }

            _combatFeedbackPresenter?.ForceNextKillMoment();

            GameLogger.Log(LogSourceType.BattleSystem, $"BattleDebugHotkeys: Tier 3 attack (match count {_tier3MatchCount}).");

            _eventHub.PuzzleEvents.PuzzleTilesMatchedEvent.Raise(new PuzzleTilesMatchedPayload(ActionType.Attack, _tier3MatchCount));
        }
    }
}
#endif
