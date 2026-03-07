using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Produced by HeroStateMachine when CombatHitResolvedEvent is received.
    /// Used by HeroAttackState to detect mid-combo target death and force finish.
    /// </summary>
    public sealed class HeroCombatHitResolvedSignal : IStateSignal
    {
        public CombatHitResolvedPayload Payload { get; }

        public HeroCombatHitResolvedSignal(CombatHitResolvedPayload payload)
        {
            Payload = payload;
        }
    }
}