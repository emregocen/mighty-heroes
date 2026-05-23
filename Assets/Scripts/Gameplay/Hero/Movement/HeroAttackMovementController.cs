using System;
using DG.Tweening;
using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Movement
{
    /// <summary>
    /// Handles hero attack root motion transfer, sword layer blending,
    /// and post-attack retreat.
    /// Attack advance is animation-driven (root motion), retreat is DOTween-driven.
    /// Raises RetreatCompleted when the hero reaches the pre-attack origin so
    /// HeroComponent can forward the signal into the FSM without coupling states
    /// to this MonoBehaviour directly.
    /// </summary>
    public sealed class HeroAttackMovementController : MonoBehaviour
    {
        // CONSTANTS

        private static readonly int IsRetreatingHash =
            Animator.StringToHash("IsRetreating");

        // FIELDS

        [Header("Dependencies")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _heroRoot;
        [SerializeField] private Transform _animatorVisualRoot;

        [Header("Sword Layers")]
        [SerializeField] private string _swordArmLayerName = "SwordArm";
        [SerializeField] private string _swordHandLayerName = "SwordHand";
        [SerializeField] private float _swordArmIdleWeight = 0.65f;
        [SerializeField] private float _swordHandIdleWeight = 1f;
        [SerializeField] private float _swordLayerBlendDuration = 0.12f;

        [Header("Retreat")]
        [SerializeField] private float _retreatSpeed = 1.8f;
        [SerializeField] private float _minimumRetreatDistance = 0.05f;
        [SerializeField] private Ease _retreatEase = Ease.Linear;

        private int _swordArmLayerIndex = -1;
        private int _swordHandLayerIndex = -1;

        private Vector3 _capturedStartPosition;
        private Vector3 _visualInitialLocalPosition;
        private Quaternion _visualInitialLocalRotation;

        private Tween _swordArmTween;
        private Tween _swordHandTween;
        private Tween _retreatTween;

        // PROPERTIES

        /// <summary>Gets whether an attack animation is currently active.</summary>
        public bool IsAttacking { get; private set; }

        /// <summary>Gets whether the hero is currently retreating.</summary>
        public bool IsRetreating { get; private set; }

        /// <summary>
        /// Raised when the hero starts retreating after an attack sequence.
        /// </summary>
        public event Action RetreatStarted;

        /// <summary>
        /// Raised when the hero reaches the pre-attack origin and retreat ends.
        /// Consumed by HeroComponent to forward HeroRetreatEndedSignal into the FSM.
        /// </summary>
        public event Action RetreatCompleted;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            CacheLayerIndices();
            CacheVisualInitialTransform();
            CaptureStartPosition();
            EnableIdleSwordPoseInstant();
        }

        private void OnAnimatorMove()
        {
            if (_animator == null || _heroRoot == null)
                return;

            if (!IsAttacking)
                return;

            Vector3 deltaPosition = _animator.deltaPosition;
            deltaPosition.y = 0f;

            _heroRoot.position += deltaPosition;
            _heroRoot.rotation *= _animator.deltaRotation;

            ResetAnimatorVisualRoot();
        }

        private void OnDisable()
        {
            KillSwordTweens();
            KillRetreatTween();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Captures the current hero root world position as the retreat destination.
        /// Called from Awake and optionally re-called before each attack by HeroComponent.
        /// </summary>
        public void CaptureStartPosition()
        {
            if (_heroRoot == null) return;
            _capturedStartPosition = _heroRoot.position;
            GameLogger.Log(LogSourceType.Hero, $"CaptureStartPosition — captured={_capturedStartPosition}");
        }

        /// <summary>
        /// Called when the attack sequence starts.
        /// Enables root motion transfer and disables idle sword pose layers.
        /// </summary>
        public void NotifyAttackStarted()
        {
            CaptureStartPosition();

            IsAttacking = true;
            IsRetreating = false;

            DisableIdleSwordPose();

            if (_animator != null)
                _animator.applyRootMotion = true;
        }

        /// <summary>
        /// Called when the attack sequence finishes.
        /// Disables root motion transfer and starts the DOTween retreat.
        /// </summary>
        public void NotifyAttackEnded()
        {
            IsAttacking = false;

            if (_animator != null)
                _animator.applyRootMotion = false;

            EnableIdleSwordPose();
            StartRetreat();
        }

        public void NotifyVictoryStarted()
        {
            KillSwordTweens();
            SetLayerWeight(_swordArmLayerIndex, 0f);
            SetLayerWeight(_swordHandLayerIndex, 0f);
        }

        /// <summary>
        /// Immediately cancels an in-progress retreat without raising RetreatCompleted.
        /// Called when battle ends during retreat so the FSM can transition to
        /// VictoryState cleanly without a stale tween or callback firing afterward.
        /// </summary>
        public void CancelRetreat()
        {
            if (!IsRetreating)
                return;

            KillRetreatTween();
            EnableIdleSwordPose();
            ResetAnimatorVisualRoot();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Starts moving the hero back to _capturedStartPosition via DOTween.
        /// Duration is derived from distance so speed stays constant regardless
        /// of how far root motion advanced the hero.
        /// Calls CompleteRetreat directly if the hero is already at origin.
        /// </summary>
        private void StartRetreat()
        {
            if (_heroRoot == null)
            {
                CompleteRetreat();
                return;
            }

            float distance = Vector3.Distance(_heroRoot.position, _capturedStartPosition);

            if (distance <= _minimumRetreatDistance)
            {
                CompleteRetreat();
                return;
            }

            KillRetreatTween();

            float duration = distance / _retreatSpeed;

            IsRetreating = true;
            SetAnimatorRetreat(true);
            RetreatStarted?.Invoke();

            _retreatTween = _heroRoot
                .DOMove(_capturedStartPosition, duration)
                .SetEase(_retreatEase)
                .OnComplete(CompleteRetreat);

            GameLogger.Log(LogSourceType.Hero,
                $"HeroAttackMovementController.StartRetreat distance={distance:F2} duration={duration:F2}");
        }

        /// <summary>
        /// Finalizes retreat: resets state, restores animator and visual root,
        /// then raises RetreatCompleted for HeroComponent to forward into the FSM.
        /// </summary>
        private void CompleteRetreat()
        {
            IsRetreating = false;
            SetAnimatorRetreat(false);
            EnableIdleSwordPose();
            ResetAnimatorVisualRoot();
            _retreatTween = null;

            GameLogger.Log(LogSourceType.Hero, "HeroAttackMovementController.CompleteRetreat");

            RetreatCompleted?.Invoke();
        }

        /// <summary>Sets the IsRetreating animator bool parameter.</summary>
        /// <param name="isRetreating">True to play retreat animation clip.</param>
        private void SetAnimatorRetreat(bool isRetreating)
        {
            if (_animator == null)
                return;

            _animator.SetBool(IsRetreatingHash, isRetreating);
        }

        /// <summary>Caches animator layer indices for sword override layers by name.</summary>
        private void CacheLayerIndices()
        {
            if (_animator == null)
                return;

            _swordArmLayerIndex = _animator.GetLayerIndex(_swordArmLayerName);
            _swordHandLayerIndex = _animator.GetLayerIndex(_swordHandLayerName);
        }

        /// <summary>Blends sword idle override layers up with configured duration.</summary>
        private void EnableIdleSwordPose()
        {
            BlendLayer(_swordArmLayerIndex, _swordArmIdleWeight, ref _swordArmTween);
            BlendLayer(_swordHandLayerIndex, _swordHandIdleWeight, ref _swordHandTween);
        }

        /// <summary>Blends sword idle override layers down to zero with configured duration.</summary>
        private void DisableIdleSwordPose()
        {
            BlendLayer(_swordArmLayerIndex, 0f, ref _swordArmTween);
            BlendLayer(_swordHandLayerIndex, 0f, ref _swordHandTween);
        }

        /// <summary>Sets sword idle override layers to their target weights immediately, without blending.</summary>
        private void EnableIdleSwordPoseInstant()
        {
            SetLayerWeight(_swordArmLayerIndex, _swordArmIdleWeight);
            SetLayerWeight(_swordHandLayerIndex, _swordHandIdleWeight);
        }

        /// <summary>
        /// Tweens a single animator layer weight to the target value.
        /// Kills any existing tween on that layer before starting a new one.
        /// </summary>
        /// <param name="layerIndex">Animator layer index to blend.</param>
        /// <param name="targetWeight">Target layer weight value.</param>
        /// <param name="tween">Tween field to manage lifecycle; passed by ref.</param>
        private void BlendLayer(int layerIndex, float targetWeight, ref Tween tween)
        {
            if (_animator == null || layerIndex < 0)
                return;

            tween?.Kill();

            tween = DOTween.To(
                () => _animator.GetLayerWeight(layerIndex),
                value => _animator.SetLayerWeight(layerIndex, value),
                targetWeight,
                _swordLayerBlendDuration);
        }

        /// <summary>Sets an animator layer weight directly when the index is valid.</summary>
        /// <param name="layerIndex">Animator layer index.</param>
        /// <param name="weight">Weight value to set.</param>
        private void SetLayerWeight(int layerIndex, float weight)
        {
            if (_animator == null || layerIndex < 0)
                return;
        }

        /// <summary>Caches the initial local transform of the animator visual root for later reset.</summary>
        private void CacheVisualInitialTransform()
        {
            if (_animatorVisualRoot == null)
                return;

            _visualInitialLocalPosition = _animatorVisualRoot.localPosition;
            _visualInitialLocalRotation = _animatorVisualRoot.localRotation;
        }

        /// <summary>
        /// Resets the animator visual root to its cached local transform after root motion transfer.
        /// Prevents visual drift when root motion is applied to _heroRoot instead of the animator GameObject.
        /// </summary>
        private void ResetAnimatorVisualRoot()
        {
            if (_animatorVisualRoot == null)
                return;

            _animatorVisualRoot.localPosition = _visualInitialLocalPosition;
            _animatorVisualRoot.localRotation = _visualInitialLocalRotation;
        }

        /// <summary>Kills active sword layer blend tweens and nulls the references.</summary>
        private void KillSwordTweens()
        {
            _swordArmTween?.Kill();
            _swordHandTween?.Kill();

            _swordArmTween = null;
            _swordHandTween = null;
        }

        /// <summary>
        /// Kills the active retreat tween and resets retreat state.
        /// Called from OnDisable and before starting a new retreat.
        /// </summary>
        private void KillRetreatTween()
        {
            if (_retreatTween == null)
                return;

            _retreatTween.Kill();
            _retreatTween = null;

            IsRetreating = false;
            SetAnimatorRetreat(false);
        }
    }
}