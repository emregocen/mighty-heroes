using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;

namespace MightyHeroes.Gameplay.Hero.Animation.Behaviours
{
    /// <summary>
    /// Fires when the whole attack sequence ends.
    /// This behaviour must be placed on the animator state that represents
    /// the end of the full flow (including recover-to-idle).
    /// </summary>
    public sealed class HeroAttackSequenceEndedBehaviour : StateMachineBehaviour
    {
        private HeroComponent _hero;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_hero != null)
                return;

            _hero = animator.GetComponentInParent<HeroComponent>();

            if (_hero == null)
            {
                GameLogger.Log(LogSourceType.Animation, "HeroAttackSequenceEndedBehaviour could not find HeroComponent.");
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_hero == null)
                return;

            _hero.NotifyAttackSequenceEnded();
        }
    }
}