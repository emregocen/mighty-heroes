using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Enemy.AI;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.States
{
    /// <summary>
    /// Represents the decision phase of the enemy.
    ///
    /// In this state, the enemy resolves its next action via the AI layer
    /// and immediately transitions to the appropriate execution state.
    /// </summary>
    public sealed class EnemyReadyState : EnemyBaseState
    {
        /// <summary>
        /// Cached decision produced by the AI layer.
        /// </summary>
        private EnemyActionDecision _decision;

        /// <summary>
        /// Creates a new enemy ready state.
        /// </summary>
        public EnemyReadyState(EnemyStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Resolves the next enemy action and transitions immediately.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.Enemy,
                $"Enemy[{StateMachine.Enemy.EntityId}] Ready: resolving AI decision"
            );

            _decision = ResolveDecision();

            switch (_decision.ActionType)
            {
                case ActionType.Attack:
                    StateMachine.SwitchToAttackState(_decision.AttackVariant, _decision.AttackIntensity);
                    break;

                case ActionType.None:
                default:
                    StateMachine.SwitchToIdleState();
                    break;
            }
        }

        /// <summary>
        /// Clears cached decision data.
        /// </summary>
        public override void Exit()
        {
            _decision = null;
        }

        /// <summary>
        /// Resolves the next action decision from the AI layer.
        /// </summary>
        private EnemyActionDecision ResolveDecision()
        {
            EnemyActionResolver resolver = StateMachine.ActionResolver;

            return resolver.Resolve(StateMachine.Enemy);
        }
    }
}