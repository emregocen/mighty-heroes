using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation;
using MightyHeroes.Gameplay.Animation.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// Orchestrates the battle intro sequence as a coroutine, using an orbital
    /// camera that always tracks the hero's live position.
    ///
    /// Timeline:
    ///  1. Intro camera activated immediately on Initialize, snapped to StartPose,
    ///     orbiting around the hero at his intro start position.
    ///  2. On Play: wait CameraSettleDuration.
    ///  3. Hero walks to battle position. Simultaneously, the camera slow-orbits
    ///     from StartPose to TauntPose over SlowOrbitDuration — both run in
    ///     parallel, and since the orbit rig reads the hero's live position every
    ///     frame, the hero stays centered throughout regardless of which
    ///     animation is still running.
    ///  4. Hero plays taunt at TauntPose. While the taunt plays, the lower body
    ///     layer is blended in so the legs hold the idle pose and do not slide.
    ///     If the taunt runs longer than SlowOrbitDuration, the camera simply
    ///     holds at TauntPose (the orbit tween has already completed) until the
    ///     taunt animation signals it has ended (via IIntroSignalReceiver),
    ///     plus TauntPostHoldDuration.
    ///  5. Camera fast-orbits from its current pose (forced to TauntPose in case
    ///     the slow orbit was still running) to EnemyRevealPose over FastOrbitDuration.
    ///  6. Swap to CM_BattleCamera, sword re-enabled, puzzle revealed.
    ///  7. onComplete callback fires.
    /// </summary>
    public sealed class BattleIntroController : MonoBehaviour, IIntroSignalReceiver
    {
        // CONSTANTS

        private const int IntroCameraPriority = 20;
        private const int BattleCameraActivePriority = 10;
        private const int InactivePriority = 0;
        private const float LocomotionBlendOutDuration = 0.2f;

        private static readonly int AnimIsLocomotion = Animator.StringToHash("IsLocomotion");
        private static readonly int AnimLocomotionVariant = Animator.StringToHash("LocomotionVariant");
        private static readonly int AnimLocomotionBlend = Animator.StringToHash("LocomotionBlend");
        private static readonly int AnimTauntVariant = Animator.StringToHash("TauntVariant");
        private static readonly int AnimTaunt = Animator.StringToHash("Taunt");

        // FIELDS

        [Header("Cameras")]
        [SerializeField] private CinemachineCamera _introCamera;
        [SerializeField] private OrbitCameraRig _orbitCameraRig;
        [SerializeField] private CinemachineCamera _battleCamera;

        [Header("Camera Orbit Keyframes")]
        [Tooltip("Camera pose before the hero starts walking. Example: bottom-left, low angle.")]
        [SerializeField] private OrbitPose _startPose = new OrbitPose(-40f, -8f, 3.5f);

        [Tooltip("Camera pose reached by the slow orbit, held through the rest of the taunt. " +
                 "Example: top-right, higher angle.")]
        [SerializeField] private OrbitPose _tauntPose = new OrbitPose(40f, 20f, 3.5f);

        [Tooltip("Camera pose for the enemy reveal, reached by the faster orbit after taunt ends.")]
        [SerializeField] private OrbitPose _enemyRevealPose = new OrbitPose(160f, 22f, 4.5f);

        [SerializeField] private Ease _slowOrbitEase = Ease.InOutSine;
        [SerializeField] private Ease _fastOrbitEase = Ease.InOutQuad;

        [Header("Hero")]
        [SerializeField] private Transform _heroRoot;
        [SerializeField] private Transform _heroIntroStartPoint;
        [SerializeField] private Transform _heroBattlePosition;
        [SerializeField] private Animator _heroAnimator;

        [Header("Hero Taunt Lower Body Lock")]
        [Tooltip("Animator layer that holds the idle pose for the legs. Weight is driven by code.")]
        [SerializeField] private string _tauntLowerBodyLayerName = "TauntLowerBody";
        [SerializeField] private float _lowerBodyLockBlendInDuration = 0.1f;
        [SerializeField] private float _lowerBodyLockBlendOutDuration = 0.15f;

        [Header("Hero Sword")]
        [SerializeField] private GameObject _swordProp;

        [Header("Puzzle (optional)")]
        [SerializeField] private RectTransform _puzzleRect;
        [SerializeField] private CanvasGroup _puzzleCanvasGroup;
        [Tooltip("How many multiples of the puzzle panel's own height to hide it by. " +
                 "2-3 is usually enough to fully clear the visible area regardless of " +
                 "whether the canvas is screen-space or world-space.")]
        [SerializeField, Min(0.5f)] private float _puzzleHideDistanceMultiplier = 2.5f;

        [Tooltip("If the panel appears to hide upward instead of downward, toggle this.")]
        [SerializeField] private bool _puzzleHideBelow = true;

        private BattleIntroConfig _config;
        private Vector2 _puzzleHiddenPosition;
        private Vector2 _puzzleShownPosition;
        private bool _tauntEnded;
        private Coroutine _introCoroutine;
        private int _tauntLowerBodyLayerIndex = -1;
        private Tween _lowerBodyLockTween;

        // UNITY CALLBACKS

        private void Awake()
        {
            IntroSignalRegistry.Register(this);
            CacheTauntLowerBodyLayerIndex();
        }

        private void OnDestroy()
        {
            IntroSignalRegistry.Unregister(this);

            _lowerBodyLockTween?.Kill();

            if (_introCoroutine != null)
                StopCoroutine(_introCoroutine);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Injects the config, snaps the hero to the intro start position, hides
        /// the sword, binds the orbit rig to the hero and snaps it to StartPose,
        /// and activates the intro camera immediately.
        /// </summary>
        public void Initialize(BattleIntroConfig config)
        {
            _config = config;

            if (_config == null)
                return;

            if (_heroRoot != null && _heroIntroStartPoint != null)
                _heroRoot.position = _heroIntroStartPoint.position;

            if (_swordProp != null)
                _swordProp.SetActive(false);

            if (_orbitCameraRig != null)
            {
                _orbitCameraRig.SetTarget(_heroRoot);
                _orbitCameraRig.SetImmediate(_startPose.Yaw, _startPose.Pitch, _startPose.Distance);
            }

            SetCameraPriorities(introActive: true, battleActive: false);
            HidePuzzleImmediate();

            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController.Initialize complete.");
        }

        public void Play(Action onComplete)
        {
            if (_config == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleIntroController: no config, skipping intro.");
                onComplete?.Invoke();
                return;
            }

            _introCoroutine = StartCoroutine(RunIntroSequence(onComplete));
        }

        // IIntroSignalReceiver

        public void NotifyTauntStarted()
        {
            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController: taunt started.");
        }

        public void NotifyTauntEnded()
        {
            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController: taunt ended.");
            _tauntEnded = true;
        }

        // PRIVATE METHODS — Sequence

        private IEnumerator RunIntroSequence(Action onComplete)
        {
            _tauntEnded = false;

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 0 — camera settle.");
            yield return new WaitForSeconds(_config.CameraSettleDuration);

            float walkDistance = (_heroRoot != null && _heroBattlePosition != null)
                ? Vector3.Distance(_heroRoot.position, _heroBattlePosition.position)
                : 0f;

            float walkDuration = walkDistance / Mathf.Max(_config.HeroWalkSpeed, 0.01f);

            GameLogger.Log(LogSourceType.BattleSystem,
                $"Intro Phase 1 — hero walk + slow orbit start. walkDuration={walkDuration:F2} ");

            StartLocomotion();

            // Hero walk and camera slow-orbit run in parallel — neither blocks the other.
            Tween walkTween = null;
            if (_heroRoot != null && _heroBattlePosition != null && walkDuration > 0f)
            {
                walkTween = _heroRoot
                    .DOMove(_heroBattlePosition.position, walkDuration)
                    .SetEase(Ease.Linear);

                TweenOrbitTo(_tauntPose, walkDuration, _slowOrbitEase);
            }
            else if (_orbitCameraRig != null)
            {
                _orbitCameraRig.SetImmediate(_tauntPose.Yaw, _tauntPose.Pitch, _tauntPose.Distance);
            }

            if (walkTween != null)
                yield return walkTween.WaitForCompletion();

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 1 — hero walk complete.");

            yield return BlendLocomotionToIdle();
            StopLocomotion();

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 2 — taunt start.");
            StartTaunt();

            yield return new WaitUntil(() => _tauntEnded);
            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 2 — taunt signal received, post-hold.");

            yield return new WaitForSeconds(_config.TauntPostHoldDuration);
            EndTaunt();

            // Force the orbit rig to exactly TauntPose in case the slow orbit
            // tween was still running when the taunt finished (taunt shorter
            // than SlowOrbitDuration) — guarantees a clean, jump-free start
            // for the fast orbit regardless of timing.
            if (_orbitCameraRig != null)
            {
                _orbitCameraRig.SetImmediate(_tauntPose.Yaw, _tauntPose.Pitch, _tauntPose.Distance);
            }

            GameLogger.Log(LogSourceType.BattleSystem,
                $"Intro Phase 3 — fast orbit to enemy reveal. duration={_config.FastOrbitDuration:F2}");

            Tween fastOrbitTween = TweenOrbitTo(_enemyRevealPose, _config.FastOrbitDuration, _fastOrbitEase);

            if (fastOrbitTween != null)
                yield return fastOrbitTween.WaitForCompletion();
            else
                yield return new WaitForSeconds(_config.FastOrbitDuration);

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 3 — enemy reveal complete.");

            SetCameraPriorities(introActive: false, battleActive: true);
            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 4 — swapped to battle camera.");

            if (_swordProp != null)
                _swordProp.SetActive(true);

            GameLogger.Log(LogSourceType.BattleSystem,
                $"About to reveal puzzle: rect null={_puzzleRect == null} canvasGroup null={_puzzleCanvasGroup == null}");

            if (_puzzleRect != null || _puzzleCanvasGroup != null)
                RevealPuzzle();

            yield return new WaitForSeconds(_config.SweepBackDuration);

            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController: intro complete.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Tweens the orbit rig's Yaw/Pitch/Distance/Roll from their current live
        /// values to the target pose. Reads current values from the rig itself
        /// (not a fixed "from" parameter) so this composes safely even if a
        /// previous orbit tween did not fully finish.
        /// </summary>
        private Tween TweenOrbitTo(OrbitPose to, float duration, Ease ease)
        {
            if (_orbitCameraRig == null)
                return null;

            Vector3 toVec = new Vector3(to.Yaw, to.Pitch, to.Distance);

            return DOTween.To(
                    () => new Vector3(
                        _orbitCameraRig.Yaw,
                        _orbitCameraRig.Pitch,
                        _orbitCameraRig.Distance),
                    v =>
                    {
                        _orbitCameraRig.Yaw = v.x;
                        _orbitCameraRig.Pitch = v.y;
                        _orbitCameraRig.Distance = v.z;
                    },
                    toVec,
                    duration)
                .SetEase(ease);
        }

        private IEnumerator BlendLocomotionToIdle()
        {
            if (_heroAnimator == null)
                yield break;

            Tween blendTween = DOVirtual.Float(1f, 0f, LocomotionBlendOutDuration,
                value => _heroAnimator.SetFloat(AnimLocomotionBlend, value));

            yield return blendTween.WaitForCompletion();
        }

        // PRIVATE METHODS — Hero animation

        private void StartLocomotion()
        {
            if (_heroAnimator == null)
                return;

            _heroAnimator.applyRootMotion = false;
            _heroAnimator.SetInteger(AnimLocomotionVariant, 1);
            _heroAnimator.SetFloat(AnimLocomotionBlend, 1f);
            _heroAnimator.SetBool(AnimIsLocomotion, true);
        }

        private void StopLocomotion()
        {
            if (_heroAnimator == null)
                return;

            _heroAnimator.SetBool(AnimIsLocomotion, false);
        }

        /// <summary>
        /// Blends the lower body lock layer in, then triggers the taunt on the base layer.
        /// </summary>
        private void StartTaunt()
        {
            if (_heroAnimator == null)
                return;

            BlendLowerBodyLock(1f, _lowerBodyLockBlendInDuration);

            _heroAnimator.SetInteger(AnimTauntVariant, 1);
            _heroAnimator.SetTrigger(AnimTaunt);
        }

        /// <summary>
        /// Ends the taunt and releases the lower body lock layer so battle animations
        /// control the legs again.
        /// </summary>
        private void EndTaunt()
        {
            if (_heroAnimator == null)
                return;

            _heroAnimator.SetInteger(AnimTauntVariant, 0);

            BlendLowerBodyLock(0f, _lowerBodyLockBlendOutDuration);
        }

        /// <summary>
        /// Caches the lower body lock layer index and makes sure it starts with zero weight.
        /// </summary>
        private void CacheTauntLowerBodyLayerIndex()
        {
            if (_heroAnimator == null)
                return;

            _tauntLowerBodyLayerIndex = _heroAnimator.GetLayerIndex(_tauntLowerBodyLayerName);

            if (_tauntLowerBodyLayerIndex < 0)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    $"BattleIntroController: animator layer '{_tauntLowerBodyLayerName}' not found.");
                return;
            }

            _heroAnimator.SetLayerWeight(_tauntLowerBodyLayerIndex, 0f);
        }

        /// <summary>
        /// Tweens the lower body lock layer weight to the target value.
        /// </summary>
        /// <param name="targetWeight">Target layer weight.</param>
        /// <param name="duration">Blend duration in seconds.</param>
        private void BlendLowerBodyLock(float targetWeight, float duration)
        {
            if (_heroAnimator == null || _tauntLowerBodyLayerIndex < 0)
                return;

            _lowerBodyLockTween?.Kill();

            _lowerBodyLockTween = DOTween.To(
                () => _heroAnimator.GetLayerWeight(_tauntLowerBodyLayerIndex),
                value => _heroAnimator.SetLayerWeight(_tauntLowerBodyLayerIndex, value),
                targetWeight,
                duration);
        }

        // PRIVATE METHODS — Cameras, puzzle, sword

        private void SetCameraPriorities(bool introActive, bool battleActive)
        {
            if (_introCamera != null)
                _introCamera.Priority = introActive ? IntroCameraPriority : InactivePriority;

            if (_battleCamera != null)
                _battleCamera.Priority = battleActive ? BattleCameraActivePriority : InactivePriority;
        }

        /// <summary>
        /// Moves the puzzle panel off-screen by a multiple of its own RectTransform
        /// height, rather than an assumed screen-pixel or canvas-size distance.
        /// This works correctly regardless of whether the panel sits on a
        /// screen-space canvas or a world-space one (where "1200" would mean
        /// 1200 world units, sending the panel flying far outside the camera
        /// frustum instead of just off-screen).
        /// </summary>
        private void HidePuzzleImmediate()
        {
            if (_puzzleRect != null)
            {
                _puzzleShownPosition = _puzzleRect.anchoredPosition;

                float ownHeight = _puzzleRect.rect.height > 0f ? _puzzleRect.rect.height : 100f;
                float hideDistance = ownHeight * _puzzleHideDistanceMultiplier;

                Vector2 offset = new Vector2(0f, _puzzleHideBelow ? -hideDistance : hideDistance);
                _puzzleHiddenPosition = _puzzleShownPosition + offset;
                _puzzleRect.anchoredPosition = _puzzleHiddenPosition;

                GameLogger.Log(LogSourceType.BattleSystem,
                    $"HidePuzzleImmediate: shown={_puzzleShownPosition} hidden={_puzzleHiddenPosition} " +
                    $"ownHeight={ownHeight} distance={hideDistance}");
            }

            if (_puzzleCanvasGroup != null)
                _puzzleCanvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Reveals the puzzle panel: slides up to its resting position, then
        /// overshoots slightly upward and settles back — an explicit two-step
        /// bounce rather than relying on Ease.OutBack's built-in overshoot,
        /// which can read as too subtle to notice depending on distance/duration.
        /// </summary>
        private void RevealPuzzle()
        {
            if (_puzzleRect != null)
            {
                float overshootDistance = _puzzleRect.rect.height > 0f
                    ? _puzzleRect.rect.height * 0.01f
                    : 12f;

                Vector2 overshootPosition = _puzzleShownPosition + new Vector2(0f, overshootDistance);

                Sequence sequence = DOTween.Sequence();

                // Main rise: covers most of the distance, ends slightly past the
                // resting position.
                sequence.Append(
                    _puzzleRect.DOAnchorPos(overshootPosition, _config.PuzzleRevealDuration * 0.75f)
                        .SetEase(Ease.OutQuad));

                // Settle: small bounce back down to the exact resting position.
                sequence.Append(
                    _puzzleRect.DOAnchorPos(_puzzleShownPosition, _config.PuzzleRevealDuration * 0.25f)
                        .SetEase(Ease.OutSine));

                sequence.Play();
            }

            if (_puzzleCanvasGroup != null)
            {
                _puzzleCanvasGroup
                    .DOFade(1f, _config.PuzzleRevealDuration * 0.6f)
                    .SetEase(Ease.OutCubic);
            }
        }
    }
}