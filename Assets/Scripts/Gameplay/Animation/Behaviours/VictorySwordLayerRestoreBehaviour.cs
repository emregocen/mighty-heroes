using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Restores sword arm and sword hand override layer weights when
    /// victory idle loop begins. Attach to the victory idle state.
    /// </summary>
    public sealed class VictorySwordLayerRestoreBehaviour : StateMachineBehaviour
    {
        // FIELDS

        [SerializeField] private string _swordArmLayerName = "SwordArm";
        [SerializeField] private string _swordHandLayerName = "SwordHand";
        [SerializeField] private float _swordArmWeight = 0.65f;
        [SerializeField] private float _swordHandWeight = 1f;

        private int _swordArmLayerIndex = -1;
        private int _swordHandLayerIndex = -1;

        // UNITY CALLBACKS

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ResolveLayerIndices(animator);
            SetLayerWeights(animator, _swordArmWeight, _swordHandWeight);
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