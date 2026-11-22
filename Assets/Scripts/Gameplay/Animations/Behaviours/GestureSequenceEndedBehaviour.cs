using UnityEngine;

namespace MightyHeroes.Gameplay.Animations.Behaviours
{
    public class GestureSequenceEndedBehaviour : StateMachineBehaviour
    {
        [SerializeField]
        private string parameterName = "GestureVariant";

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            animator.SetInteger(parameterName, 0);
        }
    }
}