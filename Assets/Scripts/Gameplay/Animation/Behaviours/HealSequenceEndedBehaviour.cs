using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour attached to the Heal animation state.
    /// Notifies HeroComponent when the heal animation clip finishes so the
    /// FSM can transition out of HeroSupportState.
    ///
    /// Heal is hero-only in the current demo scope, so this talks to
    /// HeroComponent directly instead of going through IAnimationSignalReceiver
    /// (which EnemyComponent also implements and should not need to change
    /// for a hero-only action).
    /// </summary>
    public sealed class HealSequenceEndedBehaviour : StateMachineBehaviour
    {
        // FIELDS

        private bool _fired;

        // UNITY CALLBACKS

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _fired = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_fired)
                return;

            _fired = true;

            HeroComponent hero = animator.GetComponentInParent<HeroComponent>();

            if (hero == null)
            {
                GameLogger.Log(LogSourceType.Animation, "HealSequenceEndedBehaviour: HeroComponent not found.");
                return;
            }

            GameLogger.Log(LogSourceType.Animation, "HealSequenceEndedBehaviour: Heal sequence ended.");
            hero.NotifyHealSequenceEnded();
        }
    }
}