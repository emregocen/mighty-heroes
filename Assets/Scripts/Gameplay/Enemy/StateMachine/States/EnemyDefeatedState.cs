using MightyHeroes.Core.Logging;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Terminal state representing enemy defeat.
    /// In this state, the enemy can no longer perform actions.
    /// </summary>
    public sealed class EnemyDefeatedState : EnemyBaseState
    {
        /// <summary>
        /// Indicates that this state is terminal, meaning the enemy cannot transition to any other state from here.
        /// </summary>
        public override bool IsTerminal => true;

        /// <summary>
        /// Creates a new enemy defeated state.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        public EnemyDefeatedState(EnemyStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Called when the enemy enters defeated state.
        /// Plays death animation and keeps the enemy inactive in terms of behavior.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.Enemy,
                $"Enemy[{StateMachine.Enemy.EntityId}] entered EnemyDefeatedState."
            );

            StateMachine.Enemy.PlayDefeat();
        }
    }
}