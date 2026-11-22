using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animations.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animations.Behaviours
{
    public sealed class AttackSequenceEndedBehaviour : StateMachineBehaviour
    {
        private IAnimationSignalReceiver _receiver;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ResolveReceiver(animator);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_receiver == null)
            {
                GameLogger.Log(LogSourceType.Animation,
                    "AttackSequenceEndedBehaviour: No IAnimationSignalReceiver found.");
                return;
            }
            _receiver.NotifyAttackSequenceEnded();
        }

        private void ResolveReceiver(Animator animator)
        {
            if (_receiver != null) return;
            _receiver = animator.GetComponentInParent<IAnimationSignalReceiver>();
            if (_receiver == null)
                GameLogger.Log(LogSourceType.Animation,
                    $"AttackSequenceEndedBehaviour: IAnimationSignalReceiver not found on '{animator.name}' or any parent.");
        }
    }
}
