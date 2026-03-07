using MightyHeroes.Core.Logging;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Terminal state representing hero defeat.
    /// In this state, the hero can no longer perform actions.
    /// </summary>
    public sealed class HeroDefeatedState : HeroBaseState
    {
        /// <summary>
        /// Indicates that this state is terminal, meaning the hero cannot transition to any other state from here.
        /// </summary>
        public override bool IsTerminal => true;

        /// <summary>
        /// Creates a new hero defeated state.
        /// </summary>
        public HeroDefeatedState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Called when the hero enters the defeated state.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(
                LogSourceType.Hero,
                "Hero entered Defeated state."
            );

            // Future:
            // - Disable puzzle input
            // - Trigger defeat UI
            // - Notify battle system
        }
    }
}