using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using UnityEngine;

namespace MightyHeroes.Gameplay.Enemy.Presentation
{
    /// <summary>
    /// Receives animation events directly from the Enemy Animator GameObject
    /// and forwards them to the EnemyStateMachine.
    ///
    /// Unity animation events can only be fired on the Animator's GameObject,
    /// so this proxy acts as a thin bridge between animation timing
    /// and enemy gameplay state logic.
    ///
    /// This class does NOT contain AI or combat logic.
    /// It only forwards animation timing signals.
    /// </summary>
    public sealed class EnemyAnimationEventProxy : MonoBehaviour
    {
        /// <summary>
        /// Owning enemy state machine.
        /// </summary>
        private EnemyStateMachine _stateMachine;

        /// <summary>
        /// Initializes the proxy with the active enemy state machine.
        /// Must be called by EnemyComponent during setup.
        /// </summary>
        /// <param name="stateMachine">Enemy state machine instance.</param>
        public void Initialize(EnemyStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// Called by animation events at the exact hit frame
        /// of an enemy attack animation.
        ///
        /// The hit direction parameter is authored directly
        /// inside the animation clip.
        /// </summary>
        /// <param name="hitDirection">
        /// Direction of the hit, encoded as an integer value
        /// of <see cref="HitDirection"/>.
        /// </param>
        public void AnimationEvent_AttackHitFrame(int hitDirection)
        {
            GameLogger.Log(LogSourceType.Enemy, "Enemy hit frame event called: " + hitDirection );
            _stateMachine.NotifyAttackHitFrame((HitDirection)hitDirection);
        }
    }
}