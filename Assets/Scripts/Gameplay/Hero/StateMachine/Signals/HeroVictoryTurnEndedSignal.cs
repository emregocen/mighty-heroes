using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Fired when the victory turn animation clip finishes.
    /// Received by HeroVictoryState to trigger idle phase and UI fade-in.
    /// </summary>
    public class HeroVictoryTurnEndedSignal : IStateSignal { }
}