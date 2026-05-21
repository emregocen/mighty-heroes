using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Animation.Interfaces
{
    /// <summary>
    /// Implemented by entity components that receive animation lifecycle signals
    /// from StateMachineBehaviours.
    /// Allows Behaviours to forward signals without knowing the concrete entity type.
    /// HeroComponent and EnemyComponent both implement this interface.
    /// </summary>
    public interface IAnimationSignalReceiver
    {
        /// <summary>
        /// Called when the hit frame of an attack animation is reached.
        /// Forwards to the FSM as an attack hit frame signal.
        /// </summary>
        /// <param name="hitDirection">Direction of the hit.</param>
        void NotifyAttackHit(HitDirection hitDirection);

        /// <summary>
        /// Called when a single attack clip within a combo ends.
        /// Forwards to the FSM as an attack clip ended signal.
        /// </summary>
        void NotifyAttackClipEnded();

        /// <summary>
        /// Called when the full attack sequence animation starts.
        /// </summary>
        void NotifyAttackSequenceStarted();

        /// <summary>
        /// Called when the full attack sequence animation has completed.
        /// Forwards to the FSM as an attack sequence ended signal.
        /// </summary>
        void NotifyAttackSequenceEnded();

        /// <summary>
        /// Called when the hit reaction animation has effectively completed.
        /// Forwards to the FSM as a hit reaction ended signal.
        /// </summary>
        void NotifyHitReactionEnded();
    }
}