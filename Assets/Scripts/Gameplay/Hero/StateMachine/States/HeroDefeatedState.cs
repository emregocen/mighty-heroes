using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Terminal state representing hero defeat.
    ///
    /// Plays the defeat animation toward the direction of the killing blow.
    /// Disables puzzle input so the player cannot act after death.
    ///
    /// This state is terminal — no further transitions occur.
    /// Battle end is handled by BattleBootstrapper listening to EntityDiedEvent.
    /// </summary>
    public sealed class HeroDefeatedState : HeroBaseState
    {
        // FIELDS

        private readonly HitDirection _hitDirection;

        // PROPERTIES

        /// <summary>Marks this state as terminal — hero cannot leave defeated state.</summary>
        public override bool IsTerminal => true;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new defeated state.
        /// </summary>
        /// <param name="stateMachine">Owning hero state machine.</param>
        /// <param name="hitDirection">Direction of the killing blow for animation.</param>
        public HeroDefeatedState(HeroStateMachine stateMachine, HitDirection hitDirection)
            : base(stateMachine)
        {
            _hitDirection = hitDirection;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Disables puzzle input and plays the directional defeat animation.
        /// </summary>
        public override void Enter()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"Hero entered HeroDefeatedState Direction={_hitDirection}");

            StateMachine.Hero.SetPuzzlePlayable(false);
            StateMachine.Hero.PlayDefeat(_hitDirection);
        }

        public override void Exit() { }
    }
}