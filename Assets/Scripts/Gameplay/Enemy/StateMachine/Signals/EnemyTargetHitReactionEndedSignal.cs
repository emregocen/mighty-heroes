using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Gameplay.Enemy.StateMachine.Signals
{
    /// <summary>
    /// Produced by EnemyStateMachine when HeroHitReactionEndedEvent is received.
    /// Used by EnemyAttackState to advance combo ONLY after hero reaction finishes.
    /// </summary>
    public sealed class EnemyTargetHitReactionEndedSignal : IStateSignal
    {
        public readonly HeroHitReactionEndedPayload Payload;

        public EnemyTargetHitReactionEndedSignal(HeroHitReactionEndedPayload payload)
        {
            Payload = payload;
        }
    }
}