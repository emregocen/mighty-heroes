using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.Signals
{
    /// <summary>
    /// Produced by EnemyStateMachine when CombatHitResolvedEvent is received.
    /// Used by EnemyAttackState to detect mid-combo target death and force finish.
    /// </summary>
    public sealed class EnemyCombatHitResolvedSignal : IStateSignal
    {
        public CombatHitResolvedPayload Payload { get; }

        public EnemyCombatHitResolvedSignal(CombatHitResolvedPayload payload)
        {
            Payload = payload;
        }
    }
}