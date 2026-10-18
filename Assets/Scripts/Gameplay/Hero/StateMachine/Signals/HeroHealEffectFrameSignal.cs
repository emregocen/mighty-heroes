using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Raised at the authored frame of the heal animation where the heal
    /// particle effect should start (for example when the hands are raised).
    /// </summary>
    public sealed class HeroHealEffectFrameSignal : IStateSignal
    {
    }
}