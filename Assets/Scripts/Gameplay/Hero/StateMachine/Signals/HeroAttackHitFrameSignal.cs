using MightyHeroes.Core.StateMachine.Signals;
using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Hero.StateMachine.Signals
{
    /// <summary>
    /// Raised at the exact hit frame of an attack animation, as defined in the animation event of each combo clip.
    /// Carries the hit direction of the attack, which is used for directional hit reactions on enemies and the hero.
    /// This signal is raised during the HeroAttackState, and is the ONLY place where combat resolution should occur for hero attacks.
    /// </summary>
    public sealed class HeroAttackHitFrameSignal : IStateSignal
    {
        /// <summary>
        /// The direction of the attack hit, used for directional hit reactions on enemies and the hero.
        /// </summary>
        public HitDirection HitDirection { get; }

        /// <summary>
        /// Constructor for the HeroAttackHitFrameSignal.
        /// </summary>
        /// <param name="hitDirection"></param>
        public HeroAttackHitFrameSignal(HitDirection hitDirection)
        {
            HitDirection = hitDirection;
        }
    }
}