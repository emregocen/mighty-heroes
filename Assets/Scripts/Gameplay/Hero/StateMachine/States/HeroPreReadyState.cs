using DG.Tweening;
using MightyHeroes.Core.Logging;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Pre-action delay state entered immediately after TurnHeroPhaseStartedEvent.
    ///
    /// Responsibilities:
    /// - Resets hero action points so the player sees full pips before acting.
    /// - Activates the AP view to signal that the hero turn is beginning.
    /// - Waits for a short delay before transitioning to ReadyState.
    /// - Cancels the delay cleanly on Exit so no stale callbacks fire if the
    ///   hero is hit or the battle ends during the wait.
    ///
    /// AP view ownership:
    /// SetActionPointViewTurnActive(true)  is called in Enter.
    /// SetActionPointViewTurnActive(false) is called by HeroIdleState.Enter
    /// when the turn ends, not here, because Exit fires on every transition
    /// including to ReadyState.
    /// </summary>
    public sealed class HeroPreReadyState : HeroBaseState
    {
        // CONSTANTS

        /// <summary>
        /// Seconds the hero waits before puzzle input is enabled.
        /// Gives the player time to see the restored AP pips.
        /// Adjust between 0.4f and 0.8f based on feel.
        /// </summary>
        private const float PreReadyDelay = 0.4f;

        // FIELDS

        /// <summary>Active DOTween tween driving the delay. Killed on Exit.</summary>
        private Tween _delayTween;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new hero pre-ready state.
        /// </summary>
        /// <param name="stateMachine">Owning hero state machine.</param>
        public HeroPreReadyState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Resets AP, activates the AP view and starts the pre-action delay.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"Hero PreReady: waiting {PreReadyDelay}s before puzzle input.");

            StateMachine.Hero.ResetActionPoints();
            StateMachine.Hero.SetActionPointViewTurnActive(true);

            _delayTween = DOVirtual.DelayedCall(
                PreReadyDelay,
                StateMachine.SwitchToReadyState,
                ignoreTimeScale: false);
        }

        /// <summary>
        /// Kills the delay tween to prevent stale callbacks.
        /// Called on every exit — transition to ReadyState, TakingHit, or Defeated.
        /// </summary>
        public override void Exit()
        {
            _delayTween?.Kill();
            _delayTween = null;
        }
    }
}