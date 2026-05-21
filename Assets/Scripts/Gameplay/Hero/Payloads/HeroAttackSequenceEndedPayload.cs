namespace MightyHeroes.Gameplay.Hero.Payloads
{
    /// <summary>
    /// Raised by HeroComponent when the full attack sequence animation ends.
    /// Used by BattleBootstrapper to defer AllEnemiesDefeatedEvent until after
    /// the attack call stack has fully unwound.
    /// </summary>
    public sealed class HeroAttackSequenceEndedPayload { }
}