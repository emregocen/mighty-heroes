using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Signal forwarded into the active hero state when the heal animation
    /// sequence completes. Raised by HeroStateMachine.NotifyHealSequenceEnded,
    /// itself called from HeroHealSequenceEndedBehaviour (an Animator state
    /// machine behaviour on the heal clip's exit).
    /// </summary>
    public sealed class HeroHealSequenceEndedSignal : IStateSignal
    {
    }
}