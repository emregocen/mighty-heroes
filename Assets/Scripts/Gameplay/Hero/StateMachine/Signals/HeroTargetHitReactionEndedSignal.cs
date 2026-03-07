using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Enemy.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Produced by HeroStateMachine when EnemyHitReactionEndedEvent is received.
    /// Used by HeroAttackState to advance combo ONLY after enemy reaction finishes.
    /// </summary>
    public sealed class HeroTargetHitReactionEndedSignal : IStateSignal
    {
        public readonly EnemyHitReactionEndedPayload Payload;

        public HeroTargetHitReactionEndedSignal(EnemyHitReactionEndedPayload payload)
        {
            Payload = payload;
        }
    }
}