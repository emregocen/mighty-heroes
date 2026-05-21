using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Disables sword arm and sword hand override layers when victory
    /// locomotion animation begins. Attach to the first victory clip state.
    /// </summary>
    public sealed class VictorySwordLayerDisableBehaviour : StateMachineBehaviour
    {
        // FIELDS

        [SerializeField] private string _swordArmLayerName = "SwordArm";
        [SerializeField] private string _swordHandLayerName = "SwordHand";

        private int _swordArmLayerIndex = -1;
        private int _swordHandLayerIndex = -1;

        // UNITY CALLBACKS

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ResolveLayerIndices(animator);
            SetLayerWeights(animator, 0f, 0f);
        }

        // PRIVATE METHODS

        private void ResolveLayerIndices(Animator animator)
        {
            if (_swordArmLayerIndex >= 0)
                return;

            _swordArmLayerIndex = animator.GetLayerIndex(_swordArmLayerName);
            _swordHandLayerIndex = animator.GetLayerIndex(_swordHandLayerName);
        }

        private void SetLayerWeights(Animator animator, float armWeight, float handWeight)
        {
            if (_swordArmLayerIndex >= 0)
                animator.SetLayerWeight(_swordArmLayerIndex, armWeight);

            if (_swordHandLayerIndex >= 0)
                animator.SetLayerWeight(_swordHandLayerIndex, handWeight);
        }
    }
}