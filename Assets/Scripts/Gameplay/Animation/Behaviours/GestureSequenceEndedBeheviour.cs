using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    public class GestureSequenceEndedBeheviour : StateMachineBehaviour
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