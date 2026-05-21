using DG.Tweening;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Hero.Movement;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Presentation
{
    /// <summary>
    /// Handles hero target focus presentation by combining Animator LookAt IK,
    /// persistent selected target focus and temporary combat focus.
    /// Root rotation is delayed while the hero is retreating.
    /// </summary>
    public sealed class HeroTargetFocusController : MonoBehaviour
    {
        // CONSTANTS

        private const float RotationEpsilon = 0.001f;

        // FIELDS

        [Header("Dependencies")]
        [SerializeField] private Transform _heroRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private HeroAttackMovementController _attackMovementController;

        [Header("Look At IK")]
        [SerializeField] private float _headWeight = 0.65f;
        [SerializeField] private float _torsoWeight = 0.30f;
        [SerializeField] private float _eyesWeight = 0f;
        [SerializeField] private float _clampWeight = 0.65f;
        [SerializeField] private float _headBlendDuration = 0.20f;
        [SerializeField] private float _torsoBlendDuration = 0.24f;
        [SerializeField] private float _torsoDelay = 0.04f;
        [SerializeField] private float _lookPositionSmoothTime = 0.16f;

        [Header("Root Rotation")]
        [SerializeField] private float _rootRotationDelay = 0.30f;
        [SerializeField] private float _rootRotationDuration = 0.40f;

        [Header("Return Focus")]
        [SerializeField] private float _returnToPersistentFocusDelay = 0.18f;

        private BattleFormation _battleFormation;
        private EventHub _eventHub;

        private Transform _persistentTargetFocus;
        private Transform _temporaryTargetFocus;
        private Transform _currentTargetFocus;

        private Vector3 _currentLookPosition;
        private Vector3 _lookPositionVelocity;

        private float _currentHeadWeight;
        private float _currentTorsoWeight;

        private bool _hasLookPosition;
        private bool _wasRetreating;
        private bool _hasQueuedFocusAfterRetreat;

        private Sequence _lookAtSequence;
        private Tween _rootRotationTween;
        private Tween _returnFocusTween;

        // PROPERTIES

        /// <summary>
        /// Gets the currently active focus target.
        /// Temporary combat focus overrides persistent selected target focus.
        /// </summary>
        private Transform ActiveTargetFocus =>
            _temporaryTargetFocus != null
                ? _temporaryTargetFocus
                : _persistentTargetFocus;

        /// <summary>
        /// Gets whether hero retreat movement is currently active.
        /// </summary>
        private bool IsRetreating =>
            _attackMovementController != null &&
            _attackMovementController.IsRetreating;

        /// <summary>
        /// Gets whether hero attack movement currently owns root rotation.
        /// </summary>
        private bool IsMovementLocked =>
            _attackMovementController != null &&
            (_attackMovementController.IsAttacking ||
             _attackMovementController.IsRetreating);

        // UNITY CALLBACKS

        private void Awake()
        {
            GameLogger.Log(LogSourceType.Hero,
    $"HeroTargetFocusController.Awake — gameObject={gameObject.name} parent={transform.parent?.name}");


            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (_attackMovementController != null)
            {
                _attackMovementController.RetreatStarted += HandleRetreatStarted;
                _attackMovementController.RetreatCompleted += HandleRetreatCompleted;
            }
        }

        private void Update()
        {
            UpdateRetreatFocusLock();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null || _currentTargetFocus == null)
                return;

            UpdateLookAtIk();
        }

        private void OnDestroy()
        {
            if (_battleFormation != null)
            {
                _battleFormation.CurrentTargetFocusChanged -=
                    HandleCurrentTargetFocusChanged;
            }

            if (_eventHub != null)
            {
                _eventHub.EnemyEvents.EnemyAttackStartedEvent.Unregister(
                    HandleEnemyAttackStarted);

                _eventHub.EnemyEvents.EnemyAttackFinishedEvent.Unregister(
                    HandleEnemyAttackFinished);
            }

            if (_attackMovementController != null)
            {
                _attackMovementController.RetreatStarted -= HandleRetreatStarted;
                _attackMovementController.RetreatCompleted -= HandleRetreatCompleted;
            }

            _lookAtSequence?.Kill();
            _rootRotationTween?.Kill();
            _returnFocusTween?.Kill();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Subscribes to battle target focus and enemy attack events.
        /// </summary>
        /// <param name="battleFormation">Battle formation that owns target state.</param>
        /// <param name="eventHub">Event hub for gameplay event subscriptions.</param>
        public void Initialize(BattleFormation battleFormation, EventHub eventHub)
        {
            if (battleFormation == null || eventHub == null)
                return;

            _battleFormation = battleFormation;
            _eventHub = eventHub;

            _battleFormation.CurrentTargetFocusChanged +=
                HandleCurrentTargetFocusChanged;

            _eventHub.EnemyEvents.EnemyAttackStartedEvent.Register(
                HandleEnemyAttackStarted);

            _eventHub.EnemyEvents.EnemyAttackFinishedEvent.Register(
                HandleEnemyAttackFinished);

            if (_battleFormation.CurrentTargetFocus == null)
                return;

            _persistentTargetFocus = _battleFormation.CurrentTargetFocus;
            FocusTarget(_persistentTargetFocus);
        }

        /// <summary>
        /// Temporarily overrides hero focus toward a combat-related target.
        /// </summary>
        /// <param name="targetFocus">Temporary focus target transform.</param>
        public void SetTemporaryFocus(Transform targetFocus)
        {
            if (targetFocus == null)
                return;

            _temporaryTargetFocus = targetFocus;
            FocusTarget(ActiveTargetFocus);
        }

        /// <summary>
        /// Clears temporary combat focus and returns to persistent selected target.
        /// </summary>
        public void ClearTemporaryFocus()
        {
            _temporaryTargetFocus = null;

            _returnFocusTween?.Kill();

            if (_persistentTargetFocus == null)
                return;

            _returnFocusTween = DOVirtual.DelayedCall(
                _returnToPersistentFocusDelay,
                () =>
                {
                    if (_temporaryTargetFocus != null)
                        return;

                    FocusTarget(_persistentTargetFocus);
                });
        }

        // PRIVATE METHODS

        /// <summary>
        /// Handles persistent player-selected target focus changes.
        /// </summary>
        /// <param name="targetFocus">Newly selected persistent target focus transform.</param>
        private void HandleCurrentTargetFocusChanged(Transform targetFocus)
        {
            if (targetFocus == null)
                return;

            _persistentTargetFocus = targetFocus;

            if (_temporaryTargetFocus != null)
                return;

            FocusTarget(_persistentTargetFocus);
        }

        /// <summary>
        /// Handles enemy attack start events by temporarily focusing the attacker.
        /// </summary>
        /// <param name="payload">Enemy attack started payload.</param>
        private void HandleEnemyAttackStarted(EnemyAttackStartedPayload payload)
        {
            SetTemporaryFocus(payload.AttackerFocusPoint);
        }

        /// <summary>
        /// Handles enemy attack finish events by restoring persistent focus.
        /// </summary>
        /// <param name="payload">Enemy attack finished payload.</param>
        private void HandleEnemyAttackFinished(EnemyAttackFinishedPayload payload)
        {
            ClearTemporaryFocus();
        }

        /// <summary>
        /// Starts focus behaviour toward the given target focus transform.
        /// If the hero is retreating, root and IK focus changes are queued
        /// until retreat completes.
        /// </summary>
        /// <param name="targetFocus">Target focus transform to look at and face.</param>
        private void FocusTarget(Transform targetFocus)
        {
            if (targetFocus == null)
                return;

            if (IsMovementLocked)
            {
                _rootRotationTween?.Kill();
                _returnFocusTween?.Kill();

                _hasQueuedFocusAfterRetreat = true;
                return;
            }

            _currentTargetFocus = targetFocus;

            if (!_hasLookPosition)
            {
                _currentLookPosition = targetFocus.position;
                _hasLookPosition = true;
            }

            StartLookAtBlend();
            StartRootRotation(targetFocus);
        }

        /// <summary>
        /// Applies queued target focus after retreat movement completes.
        /// </summary>
        private void UpdateRetreatFocusLock()
        {
            bool isRetreating = IsRetreating;

            if (_wasRetreating && !isRetreating && _hasQueuedFocusAfterRetreat)
            {
                _hasQueuedFocusAfterRetreat = false;

                if (ActiveTargetFocus != null)
                    FocusTarget(ActiveTargetFocus);
            }

            _wasRetreating = isRetreating;
        }

        /// <summary>
        /// Starts head and torso LookAt weight blending without snapping current pose.
        /// </summary>
        private void StartLookAtBlend()
        {
            _lookAtSequence?.Kill();

            _lookAtSequence = DOTween.Sequence();

            _lookAtSequence.Insert(0f, DOTween.To(
                () => _currentHeadWeight,
                value => _currentHeadWeight = value,
                _headWeight,
                _headBlendDuration));

            _lookAtSequence.Insert(_torsoDelay, DOTween.To(
                () => _currentTorsoWeight,
                value => _currentTorsoWeight = value,
                _torsoWeight,
                _torsoBlendDuration));
        }

        /// <summary>
        /// Applies Animator humanoid LookAt IK toward the smoothed target focus position.
        /// </summary>
        private void UpdateLookAtIk()
        {
            _currentLookPosition = Vector3.SmoothDamp(
                _currentLookPosition,
                _currentTargetFocus.position,
                ref _lookPositionVelocity,
                _lookPositionSmoothTime);

            float totalWeight = Mathf.Max(
                _currentHeadWeight,
                _currentTorsoWeight);

            _animator.SetLookAtPosition(_currentLookPosition);

            _animator.SetLookAtWeight(
                totalWeight,
                _currentTorsoWeight,
                _currentHeadWeight,
                _eyesWeight,
                _clampWeight);
        }

        /// <summary>
        /// Starts delayed root rotation toward the current target focus.
        /// </summary>
        /// <param name="targetFocus">Target focus transform used for facing direction.</param>
        private void StartRootRotation(Transform targetFocus)
        {
            if (_heroRoot == null || targetFocus == null)
                return;

            if (IsMovementLocked)
            {
                _rootRotationTween?.Kill();
                _hasQueuedFocusAfterRetreat = true;
                return;
            }

            Vector3 direction = targetFocus.position - _heroRoot.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= RotationEpsilon)
                return;

            Quaternion startRotation = _heroRoot.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            _rootRotationTween?.Kill();

            _rootRotationTween = DOTween.To(
                    () => 0f,
                    value => _heroRoot.rotation = Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        value),
                    1f,
                    _rootRotationDuration)
                .SetDelay(_rootRotationDelay)
                .SetEase(Ease.OutSine);
        }

        /// <summary>
        /// Locks focus rotation while the hero is retreating.
        /// </summary>
        private void HandleRetreatStarted()
        {
            _rootRotationTween?.Kill();
            _returnFocusTween?.Kill();

            _hasQueuedFocusAfterRetreat = true;
        }

        /// <summary>
        /// Applies the latest queued focus after retreat is completed.
        /// </summary>
        private void HandleRetreatCompleted()
        {
            if (!_hasQueuedFocusAfterRetreat)
                return;

            _hasQueuedFocusAfterRetreat = false;

            if (ActiveTargetFocus != null)
                FocusTarget(ActiveTargetFocus);
        }

        public void NotifyVictoryStarted()
        {
            _lookAtSequence?.Kill();
            _rootRotationTween?.Kill();
            _returnFocusTween?.Kill();

            _currentHeadWeight = 0f;
            _currentTorsoWeight = 0f;
            _currentTargetFocus = null;
        }
    }
}