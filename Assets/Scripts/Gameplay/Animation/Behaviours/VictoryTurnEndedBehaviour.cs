using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Animator state behaviour attached to the VictoryTurn animation state.
    /// Notifies HeroComponent when the turn clip finishes
    /// so the FSM can trigger the victory idle phase and UI fade-in.
    /// </summary>
    public class VictoryTurnEndedBehaviour : StateMachineBehaviour
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

            GameLogger.Log(LogSourceType.Animation,
                $"VictoryTurnEndedBehaviour: animator.gameObject={animator.gameObject.name}");

            GameLogger.Log(LogSourceType.Animation,
                $"VictoryTurnEndedBehaviour: transform.root={animator.transform.root.name}");

            HeroComponent hero = animator.GetComponentInParent<HeroComponent>();

            GameLogger.Log(LogSourceType.Animation,
                $"VictoryTurnEndedBehaviour: hero={hero}");

            if (hero == null)
                return;

            hero.NotifyVictoryTurnEnded();
        }
    }
}