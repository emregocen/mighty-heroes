using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Types;
using UnityEngine;
using MightyHeroes.Gameplay.Hero.StateMachine;

namespace MightyHeroes.Gameplay.Hero.Presentation
{
    /// <summary>
    /// Receives animation events directly from the Hero Animator GameObject
    /// and forwards them to the HeroStateMachine.
    ///
    /// Unity animation events can only be fired on the Animator's GameObject,
    /// so this proxy acts as a thin bridge between animation timing
    /// and gameplay state logic.
    ///
    /// This class does NOT contain combat logic.
    /// It only forwards animation timing signals.
    /// </summary>
    public sealed class HeroAnimationEventProxy : MonoBehaviour
    {
        /// <summary>
        /// Owning hero state machine.
        /// </summary>
        private HeroStateMachine _stateMachine;

        /// <summary>
        /// Initializes the proxy with the active hero state machine.
        /// Must be called by HeroComponent during setup.
        /// </summary>
        /// <param name="stateMachine">Hero state machine instance.</param>
        public void Initialize(HeroStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// Called by animation events at the exact hit frame of an attack animation.
        /// The hit direction parameter represents the direction of the strike
        /// as defined by the attack animation itself.
        /// </summary>
        /// <param name="hitDirection">
        /// Direction of the hit, encoded as an integer value of <see cref="HitDirection"/>.
        /// </param>
        public void AnimationEvent_AttackHitFrame(int hitDirection)
        {
            GameLogger.Log(LogSourceType.Hero, "Hero hit frame event called: " + hitDirection);
            _stateMachine.NotifyAttackHitFrame((HitDirection)hitDirection);
        }

        /// <summary>
        /// Called by an animation event at the frame of the heal animation where
        /// the heal particle effect should start.
        /// </summary>
        public void AnimationEvent_HealEffectFrame()
        {
            GameLogger.Log(LogSourceType.Hero, "Hero heal effect frame event called.");
            _stateMachine.NotifyHealEffectFrame();
        }
    }
}