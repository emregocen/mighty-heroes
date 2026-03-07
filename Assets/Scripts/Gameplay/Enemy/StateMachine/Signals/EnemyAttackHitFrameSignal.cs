using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.Signals
{
    /// <summary>
    /// Signal raised by animation event when an enemy attack
    /// reaches its authored hit frame.
    ///
    /// This signal is consumed only by EnemyAttackState.
    /// It does not resolve damage directly.
    /// </summary>
    public sealed class EnemyAttackHitFrameSignal : IStateSignal
    {
        /// <summary>
        /// Direction of the hit (used for reaction animation and combat resolution).
        /// </summary>
        public HitDirection HitDirection { get; }

        public EnemyAttackHitFrameSignal(HitDirection hitDirection)
        {
            HitDirection = hitDirection;
        }
    }
}