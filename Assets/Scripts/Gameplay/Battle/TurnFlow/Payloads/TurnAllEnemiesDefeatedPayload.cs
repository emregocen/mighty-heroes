// Gameplay/Battle/TurnFlow/Payloads/AllEnemiesDefeatedPayload.cs

namespace MightyHeroes.Battle.TurnFlow.Payloads
{
    /// <summary>
    /// Raised by BattleBootstrapper when AliveEnemyCount reaches zero.
    /// Carries TurnIndex so HeroAttackState can raise TurnBattleVictoryPayload
    /// with the correct turn data once the attack sequence ends cleanly.
    /// </summary>
    public sealed class AllEnemiesDefeatedPayload
    {
        /// <summary>Turn index at the moment the last enemy was defeated.</summary>
        public int TurnIndex { get; }

        /// <summary>Creates a new AllEnemiesDefeatedPayload.</summary>
        /// <param name="turnIndex">Turn index when the last enemy was defeated.</param>
        public AllEnemiesDefeatedPayload(int turnIndex)
        {
            TurnIndex = turnIndex;
        }
    }
}