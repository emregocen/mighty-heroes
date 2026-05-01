using MightyHeroes.Core.Logging;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Terminal state entered when all enemies are defeated and the hero wins the battle.
    ///
    /// Responsibilities:
    /// - Plays the victory animation on the hero animator.
    /// - Raises HeroVictoryAnimationStartedEvent so UI and scene systems
    ///   know the animation is running and can schedule their transitions.
    /// - Does not transition to any further state.
    ///
    /// Design note:
    /// This state does not own the scene transition or reward flow.
    /// Those are handled by UI systems that subscribe to HeroVictoryAnimationStartedEvent.
    /// Single responsibility: hero visual victory reaction only.
    /// </summary>
    public sealed class HeroVictoryState : HeroBaseState
    {
        // PROPERTIES

        /// <summary>
        /// Marks this state as terminal so BaseStateMachine blocks
        /// any stray SwitchState calls after victory is reached.
        /// </summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        /// <summary>Creates a new hero victory state.</summary>
        public HeroVictoryState(HeroStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        /// <summary>
        /// Plays the victory animation and notifies listening systems.
        /// Called exactly once when the state is entered.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero, "Enter HeroVictoryState. Playing victory animation.");

            StateMachine.Hero.PlayVictoryAnimation();
        }
    }
}