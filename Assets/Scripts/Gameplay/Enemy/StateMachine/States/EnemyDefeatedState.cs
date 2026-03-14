using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Terminal state representing enemy defeat.
    ///
    /// Plays the defeat animation toward the direction of the killing blow.
    /// HitDirection is set on the animator before the Defeat trigger fires
    /// so the animator state machine can select the correct directional clip.
    ///
    /// This state is terminal — no further transitions occur.
    /// </summary>
    public sealed class EnemyDefeatedState : EnemyBaseState
    {
        // FIELDS

        private readonly HitDirection _hitDirection;

        // PROPERTIES

        /// <summary>Marks this state as terminal — enemy cannot leave defeated state.</summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new defeated state.
        /// </summary>
        /// <param name="stateMachine">Owning enemy state machine.</param>
        /// <param name="hitDirection">Direction of the killing blow for animation.</param>
        public EnemyDefeatedState(EnemyStateMachine stateMachine, HitDirection hitDirection)
            : base(stateMachine)
        {
            _hitDirection = hitDirection;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Plays the directional defeat animation.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Enemy,
                $"Enemy[{StateMachine.Enemy.EntityId}] entered EnemyDefeatedState Direction={_hitDirection}");

            StateMachine.Enemy.PlayDefeat(_hitDirection);
        }

        public override void Exit() { }
    }
}