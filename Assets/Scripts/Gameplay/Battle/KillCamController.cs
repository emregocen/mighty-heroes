using System.Collections;
using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Combat.Feedback;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Hero.Payloads;
using Unity.Cinemachine;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Plays the cinematic kill moment on KillCamRequestedEvent: a side camera
    /// between hero and killed enemy (biased toward the hero, with dutch tilt)
    /// plus a short global slow motion.
    ///
    /// Flow:
    /// - Request: place the kill camera, raise its priority above the battle
    ///   camera with a fast blend, start slow motion.
    /// - HeroAttackSequenceEnded (hero starts walking back): one frame later,
    ///   lower the priority with a slower blend so the camera returns together
    ///   with the hero. If the kill ended the battle, the victory camera
    ///   (higher priority) blends from the kill camera instead and this
    ///   controller only releases it once victory has taken over.
    /// - Safety: the kill camera is released after MaxHoldDuration at the latest.
    ///
    /// Slow motion and blends run on unscaled time so their durations are real
    /// seconds. Time.timeScale, brain blend and IgnoreTimeScale are always
    /// restored, including on disable.
    ///
    /// Initialized by BattleBootstrapper. Battle camera priority is 10 and the
    /// victory camera 20, so the kill camera uses 15.
    /// </summary>
    public sealed class KillCamController : MonoBehaviour
    {
        // CONSTANTS

        private const int PriorityActive = 15;
        private const int PriorityInactive = 0;

        // FIELDS

        [Header("Camera")]
        [Tooltip("Cinemachine camera used for the kill shot. It needs no Tracking or Look At target; this controller places it.")]
        [SerializeField] private CinemachineCamera _killCamera;

        [Header("Framing")]
        [Tooltip("Where the camera centers between hero (0) and enemy (1).")]
        [SerializeField, Range(0f, 1f)] private float _heroBias = 0.4f;

        [Tooltip("Sideways distance of the camera from the hero-enemy line.")]
        [SerializeField, Min(0.5f)] private float _sideDistance = 3.5f;

        [Tooltip("Camera height above the hero's root.")]
        [SerializeField] private float _cameraHeight = 1.3f;

        [Tooltip("Height of the point the camera looks at, above the hero's root.")]
        [SerializeField] private float _lookHeight = 1.1f;

        [Tooltip("Camera roll in degrees for a dramatic tilted frame.")]
        [SerializeField, Range(-30f, 30f)] private float _dutchAngle = 8f;

        [Header("Blends (real seconds)")]
        [SerializeField, Min(0f)] private float _blendInDuration = 0.15f;
        [SerializeField, Min(0f)] private float _blendOutDuration = 0.6f;

        [Tooltip("The kill camera is released after this many real seconds even if the hero never starts walking back.")]
        [SerializeField, Min(0.5f)] private float _maxHoldDuration = 3f;

        private EventHub _eventHub;
        private CombatFeedbackData _feedbackData;
        private ICombatEntityLocator _entityLocator;
        private CinemachineBrain _brain;
        private CinemachineBlendDefinition _originalDefaultBlend;
        private bool _originalIgnoreTimeScale;
        private float _originalFixedDeltaTime;
        private bool _isActive;
        private bool _isVictoryPending;
        private Coroutine _slowMotionRoutine;
        private Coroutine _holdRoutine;
        private Coroutine _blendRestoreRoutine;

        // UNITY CALLBACKS

        private void OnDisable()
        {
            RestoreTimeScale();
            RestoreBrain();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Wires the controller into the battle. Called by BattleBootstrapper.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="feedbackData">Slow motion tuning.</param>
        /// <param name="entityLocator">Resolves hero and enemy positions.</param>
        public void Initialize(EventHub eventHub, CombatFeedbackData feedbackData, ICombatEntityLocator entityLocator)
        {
            _eventHub = eventHub;
            _feedbackData = feedbackData;
            _entityLocator = entityLocator;
            _originalFixedDeltaTime = Time.fixedDeltaTime;

            Camera mainCamera = Camera.main;
            _brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;

            if (_killCamera == null || _brain == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem, "KillCamController: missing kill camera or CinemachineBrain, kill-cam disabled.");
                return;
            }

            _killCamera.Priority = PriorityInactive;
            _originalDefaultBlend = _brain.DefaultBlend;
            _originalIgnoreTimeScale = _brain.IgnoreTimeScale;

            RegisterEvents();
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.CombatEvents.KillCamRequestedEvent.Register(OnKillCamRequested);
            _eventHub.HeroEvents.HeroAttackSequenceEndedEvent.Register(OnHeroAttackSequenceEnded);
            _eventHub.TurnEvents.AllEnemiesDefeatedEvent.Register(OnAllEnemiesDefeated);
            _eventHub.HeroEvents.HeroVictoryTurnStartedEvent.Register(OnHeroVictoryTurnStarted);
        }

        private void UnregisterEvents()
        {
            if (_eventHub == null)
                return;

            _eventHub.CombatEvents.KillCamRequestedEvent.Unregister(OnKillCamRequested);
            _eventHub.HeroEvents.HeroAttackSequenceEndedEvent.Unregister(OnHeroAttackSequenceEnded);
            _eventHub.TurnEvents.AllEnemiesDefeatedEvent.Unregister(OnAllEnemiesDefeated);
            _eventHub.HeroEvents.HeroVictoryTurnStartedEvent.Unregister(OnHeroVictoryTurnStarted);
        }

        /// <summary>
        /// Places the kill camera on the side of the hero-enemy line that the
        /// battle camera is already on (never crossing the 180 degree line),
        /// biased toward the hero, looking at the bias point with dutch tilt.
        /// </summary>
        private bool PlaceCamera(int heroId, int enemyId)
        {
            if (!_entityLocator.TryGetPosition(heroId, out Vector3 heroPosition) || !_entityLocator.TryGetPosition(enemyId, out Vector3 enemyPosition))
                return false;

            Vector3 heroToEnemy = enemyPosition - heroPosition;
            heroToEnemy.y = 0f;

            if (heroToEnemy.sqrMagnitude < 0.0001f)
                return false;

            Vector3 focus = Vector3.Lerp(heroPosition, enemyPosition, _heroBias);
            Vector3 side = Vector3.Cross(Vector3.up, heroToEnemy.normalized);
            Vector3 battleCameraPosition = _brain.transform.position;

            if (Vector3.Dot(side, battleCameraPosition - focus) < 0f)
                side = -side;

            Vector3 cameraPosition = focus + side * _sideDistance + Vector3.up * _cameraHeight;
            Vector3 lookPoint = focus + Vector3.up * _lookHeight;

            _killCamera.transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(lookPoint - cameraPosition));
            _killCamera.Lens.Dutch = _dutchAngle;
            return true;
        }

        /// <summary>Raises the kill camera with a fast unscaled blend.</summary>
        private void ActivateCamera()
        {
            _brain.IgnoreTimeScale = true;
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, _blendInDuration);
            _killCamera.Priority = PriorityActive;
            _isActive = true;

            RestartRoutine(ref _blendRestoreRoutine, RestoreBlendAfter(_blendInDuration));
            RestartRoutine(ref _holdRoutine, ReleaseAfterMaxHold());
        }

        /// <summary>Lowers the kill camera; the brain blends back with the given duration.</summary>
        private void ReleaseCamera(float blendDuration)
        {
            if (!_isActive)
                return;

            _isActive = false;
            StopRoutine(ref _holdRoutine);

            _brain.IgnoreTimeScale = true;
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
            _killCamera.Priority = PriorityInactive;

            RestartRoutine(ref _blendRestoreRoutine, RestoreBlendAfter(blendDuration));
        }

        /// <summary>Restores the brain's original default blend and time scale mode.</summary>
        private void RestoreBrain()
        {
            if (_brain == null)
                return;

            _brain.DefaultBlend = _originalDefaultBlend;
            _brain.IgnoreTimeScale = _originalIgnoreTimeScale;
        }

        /// <summary>Restores normal time scale and physics step.</summary>
        private void RestoreTimeScale()
        {
            StopRoutine(ref _slowMotionRoutine);
            Time.timeScale = 1f;

            if (_originalFixedDeltaTime > 0f)
                Time.fixedDeltaTime = _originalFixedDeltaTime;
        }

        /// <summary>Applies time scale and keeps the physics step proportional.</summary>
        private void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = _originalFixedDeltaTime * timeScale;
        }

        private void RestartRoutine(ref Coroutine routine, IEnumerator body)
        {
            StopRoutine(ref routine);
            routine = StartCoroutine(body);
        }

        private void StopRoutine(ref Coroutine routine)
        {
            if (routine == null)
                return;

            StopCoroutine(routine);
            routine = null;
        }

        /// <summary>
        /// Holds slow motion for the configured real time, then eases back to
        /// normal speed over the recover duration.
        /// </summary>
        private IEnumerator RunSlowMotion()
        {
            float slowScale = _feedbackData.KillSlowMotionTimeScale;
            float recoverDuration = _feedbackData.KillSlowMotionRecoverDuration;

            SetTimeScale(slowScale);
            yield return new WaitForSecondsRealtime(_feedbackData.KillSlowMotionHoldDuration);

            float elapsed = 0f;

            while (elapsed < recoverDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetTimeScale(Mathf.Lerp(slowScale, 1f, Mathf.SmoothStep(0f, 1f, elapsed / recoverDuration)));
                yield return null;
            }

            _slowMotionRoutine = null;
            RestoreTimeScale();
        }

        private IEnumerator RestoreBlendAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            _blendRestoreRoutine = null;
            RestoreBrain();
        }

        private IEnumerator ReleaseAfterMaxHold()
        {
            yield return new WaitForSecondsRealtime(_maxHoldDuration);

            _holdRoutine = null;
            ReleaseCamera(_blendOutDuration);
        }

        /// <summary>
        /// Waits one frame so victory detection (raised on the same sequence-ended
        /// event) is known, then returns the camera unless victory takes over.
        /// </summary>
        private IEnumerator ReleaseNextFrame()
        {
            yield return null;

            if (!_isVictoryPending)
                ReleaseCamera(_blendOutDuration);
        }

        // EVENT HANDLERS

        private void OnKillCamRequested(KillCamRequestedPayload payload)
        {
            if (!PlaceCamera(payload.AttackerId, payload.TargetId))
                return;

            ActivateCamera();
            RestartRoutine(ref _slowMotionRoutine, RunSlowMotion());
        }

        private void OnHeroAttackSequenceEnded(HeroAttackSequenceEndedPayload payload)
        {
            if (_isActive)
                StartCoroutine(ReleaseNextFrame());
        }

        private void OnAllEnemiesDefeated(TurnAllEnemiesDefeatedPayload payload)
        {
            _isVictoryPending = true;
        }

        /// <summary>
        /// The victory camera is live (higher priority), so the kill camera can be
        /// lowered with no visible effect; this only cleans up its state.
        /// </summary>
        private void OnHeroVictoryTurnStarted(HeroVictoryTurnStartedPayload payload)
        {
            ReleaseCamera(_blendOutDuration);
        }
    }
}
