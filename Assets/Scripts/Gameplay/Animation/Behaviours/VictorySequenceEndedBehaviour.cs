using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour attached to the final Victory animation state.
    /// Notifies HeroComponent when the victory sequence clip finishes
    /// so the FSM can trigger the turn phase.
    /// </summary>
    public class VictorySequenceEndedBehaviour : StateMachineBehaviour
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
                GameLogger.Log(LogSourceType.Animation, "VictorySequenceEndedBehaviour: HeroComponent not found.");
                return;
            }

            GameLogger.Log(LogSourceType.Animation, "VictorySequenceEndedBehaviour: Victory sequence ended.");
            hero.NotifyVictorySequenceEnded();
        }
    }
}