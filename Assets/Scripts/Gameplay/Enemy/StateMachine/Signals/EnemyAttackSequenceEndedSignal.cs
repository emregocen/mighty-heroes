using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.Signals
{
    /// <summary>
    /// Signal raised by animation state machine behaviour
    /// when the entire enemy attack animation sequence has completed.
    ///
    /// This signal is consumed by EnemyAttackState
    /// to decide turn flow continuation.
    /// </summary>
    public sealed class EnemyAttackSequenceEndedSignal : IStateSignal
    {
        public EnemyAttackSequenceEndedSignal()
        {
        }
    }
}