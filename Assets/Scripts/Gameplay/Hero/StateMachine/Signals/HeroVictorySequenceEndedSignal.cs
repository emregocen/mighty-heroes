using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Fired when the victory animation sequence clip finishes.
    /// Received by HeroVictoryState to trigger the turn phase.
    /// </summary>
    public class HeroVictorySequenceEndedSignal : IStateSignal { }
}