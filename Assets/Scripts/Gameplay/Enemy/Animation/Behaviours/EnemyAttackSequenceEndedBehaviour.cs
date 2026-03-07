using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Enemy;

namespace MightyHeroes.Gameplay.Hero.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour responsible for detecting the end of
    /// an enemy attack animation sequence.
    /// 
    /// This behaviour is attached to animator states that represent
    /// the final state of an attack flow.
    /// 
    /// It notifies the EnemyComponent when the animator state exits,
    /// allowing the enemy state machine to safely advance its logic.
    /// 
    /// This class has no dependency on gameplay state machines
    /// and does not contain combat or decision logic.
    /// </summary>
    public sealed class EnemyAttackSequenceEndedBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Cached enemy component reference resolved from the animator hierarchy.
        /// </summary>
        private EnemyComponent _enemy;

        /// <summary>
        /// Called when the animator first enters the state this behaviour is attached to.
        /// Used to cache the EnemyComponent reference.
        /// </summary>
        /// <param name="animator">Animator controlling the state.</param>
        /// <param name="stateInfo">Information about the entered state.</param>
        /// <param name="layerIndex">Animator layer index.</param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enemy != null)
                return;

            _enemy = animator.GetComponentInParent<EnemyComponent>();

            if (_enemy == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    "Enemy attack sequence could not find EnemyComponent in animator hierarchy."
                );
            }
        }

        /// <summary>
        /// Called when the animator exits the state this behaviour is attached to.
        /// This indicates that the full attack animation sequence has completed.
        /// </summary>
        /// <param name="animator">Animator controlling the state.</param>
        /// <param name="stateInfo">Information about the exited state.</param>
        /// <param name="layerIndex">Animator layer index.</param>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enemy == null)
            {
                GameLogger.Log(
                    LogSourceType.Animation,
                    "Enemy attack sequence has no reference to EnemyComponent. Cannot notify attack sequence end."
                );
                return;
            }
                
            GameLogger.Log(
                LogSourceType.Animation,
                "Enemy attack sequence ended. Notifying EnemyComponent."
            );
            _enemy.NotifyAttackSequenceEnded();
        }
    }
}