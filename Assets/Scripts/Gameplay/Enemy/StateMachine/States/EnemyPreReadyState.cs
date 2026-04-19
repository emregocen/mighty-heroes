// Assets/Gameplay/Enemy/StateMachine/States/EnemyPreReadyState.cs

using DG.Tweening;
using MightyHeroes.Core.Logging;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Pre-action delay state entered immediately after TurnEnemyStartedEvent.
    ///
    /// Responsibilities:
    /// - Activates the AP view so the player sees the restored pips before the attack.
    /// - Waits for a short delay before transitioning to ReadyState.
    /// - Cancels the delay cleanly on Exit so no stale callbacks fire if the
    ///   enemy is hit, defeated, or the battle ends during the wait.
    ///
    /// AP view ownership:
    /// SetTurnActive(true)  is called in Enter  — this state owns the bright AP view.
    /// SetTurnActive(false) is called by EnemyIdleState.Enter when the turn ends,
    /// not here, because Exit fires on every transition including to ReadyState.
    /// </summary>
    public sealed class EnemyPreReadyState : EnemyBaseState
    {
        // CONSTANTS

        /// <summary>
        /// Seconds the enemy waits before transitioning to ReadyState.
        /// Gives the player time to see the restored AP pips.
        /// Adjust between 0.4f and 0.8f based on feel.
        /// </summary>
        private const float PreReadyDelay = 0.5f;

        // FIELDS

        /// <summary>Active DOTween tween driving the delay. Killed on Exit.</summary>
        private Tween _delayTween;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new enemy pre-ready state.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        public EnemyPreReadyState(EnemyStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Activates the AP view and starts the pre-action delay.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Enemy,
                $"Enemy[{StateMachine.Enemy.EntityId}] PreReady: waiting {PreReadyDelay}s before action.");

            StateMachine.Enemy.SetActionPointViewTurnActive(true);

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