using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Fired when HeroAttackMovementController completes the DOTween retreat
    /// back to the cached origin. Routed via HeroComponent → HeroStateMachine
    /// into HeroRetreatState.
    /// </summary>
    public sealed class HeroRetreatEndedSignal : IStateSignal { }
}