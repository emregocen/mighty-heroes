using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

public sealed class AttackSequenceEndedBehaviour : StateMachineBehaviour
{
    private IAnimationSignalReceiver _receiver;
    private bool _hasFired;

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