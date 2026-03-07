using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Raised when the entire attack sequence animation has completed.
    /// </summary>
    public sealed class HeroAttackSequenceEndedSignal : IStateSignal
    {
    }
}