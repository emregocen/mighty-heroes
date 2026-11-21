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
    ///
    /// Skip: from SkipInputDelay after Play until the battle camera takes over,
    /// a tap or click skips the rest. Running tweens and the sequence stop, the
    /// hero is placed at the battle position in idle, the camera blends to the
    /// battle camera over SkipCameraBlendDuration, and the same completion
    /// step as the normal end runs (sword, puzzle reveal, callbacks). The
    /// optional skip hint is shown only while skipping is possible.
    /// </summary>
    public sealed class BattleIntroController : MonoBehaviour, IIntroSignalReceiver
    {
        // CONSTANTS

        private const int IntroCameraPriority = 20;
        private const int BattleCameraActivePriority = 10;
        private const int InactivePriority = 0;
        private const float LocomotionBlendOutDuration = 0.2f;
        private const float SkipHintFadeDuration = 0.25f;
        private const float SkipHintPulseMinAlpha = 0.45f;
        private const float SkipHintPulseDuration = 0.8f;

        private static readonly int AnimIsLocomotion = Animator.StringToHash("IsLocomotion");
        private static readonly int AnimLocomotionVariant = Animator.StringToHash("LocomotionVariant");
        private static readonly int AnimLocomotionBlend = Animator.StringToHash("LocomotionBlend");
        private static readonly int AnimTauntVariant = Animator.StringToHash("TauntVariant");
        private static readonly int AnimTaunt = Animator.StringToHash("Taunt");

        // FIELDS

        /// <summary>Raised when the hero stops walking and the taunt starts.</summary>
        public event Action TauntStarted;

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
        [SerializeField] private Animator _heroAnimator;

        [Header("Hero Taunt Lower Body Lock")]
        [Tooltip("Animator layer that holds the idle pose for the legs (shared with LowerBodyLockBehaviour). Weight is driven by code during the taunt.")]
        [SerializeField] private string _tauntLowerBodyLayerName = "LowerBodyLock";
        [SerializeField] private float _lowerBodyLockBlendInDuration = 0.1f;
        [SerializeField] private float _lowerBodyLockBlendOutDuration = 0.15f;

        [Header("Hero Sword")]
        [SerializeField] private GameObject _swordProp;

        [Header("Skip Hint (optional)")]
        [Tooltip("Group holding the 'tap to skip' text. Shown while the intro can be skipped.")]
        [SerializeField] private CanvasGroup _skipHintGroup;

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
        private Vector3 _heroIntroPosition;
        private Vector3 _heroBattlePosition;
        private bool _hasHeroPositions;
        private Vector2 _puzzleHiddenPosition;
        private Vector2 _puzzleShownPosition;
        private bool _tauntEnded;
        private Coroutine _introCoroutine;
        private int _tauntLowerBodyLayerIndex = -1;
        private Tween _lowerBodyLockTween;
        private Tween _walkTween;
        private Tween _orbitTween;
        private Tween _locomotionBlendTween;
        private Tween _skipHintTween;
        private Action _onSwordDrawn;
        private Action _onComplete;
        private float _skipEnabledTime;
        private bool _isSkippable;
        private bool _isTaunting;
        private CinemachineBrain _brain;
        private CinemachineBlendDefinition _originalDefaultBlend;
        private bool _isBlendOverridden;

        // UNITY CALLBACKS

        private void Awake()
        {
            IntroSignalRegistry.Register(this);
            CacheTauntLowerBodyLayerIndex();
        }

        private void Update()
        {
            if (_isSkippable && Time.unscaledTime >= _skipEnabledTime && IsSkipInputPressed())
                SkipIntro();
        }

        private void OnDestroy()
        {
            IntroSignalRegistry.Unregister(this);

            KillSequenceTweens();
            _lowerBodyLockTween?.Kill();
            _skipHintTween?.Kill();
            RestoreBrainBlend();

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

            if (_heroRoot != null && _hasHeroPositions)
                _heroRoot.position = _heroIntroPosition;

            if (_swordProp != null)
                _swordProp.SetActive(false);

            if (_orbitCameraRig != null)
            {
                _orbitCameraRig.SetTarget(_heroRoot);
                _orbitCameraRig.SetImmediate(_startPose.Yaw, _startPose.Pitch, _startPose.Distance);
            }

            SetCameraPriorities(introActive: true, battleActive: false);
            HidePuzzleImmediate();
            HideSkipHintImmediate();

            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController.Initialize complete.");
        }

        /// <summary>
        /// Starts the intro sequence. Without a config the intro is skipped and both
        /// callbacks fire immediately.
        /// </summary>
        /// <param name="onSwordDrawn">Invoked when the sword prop becomes visible.</param>
        /// <param name="onComplete">Invoked when the whole intro has finished.</param>
        public void Play(Action onSwordDrawn, Action onComplete)
        {
            if (_config == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "BattleIntroController: no config, skipping intro.");
                onSwordDrawn?.Invoke();
                onComplete?.Invoke();
                return;
            }

            _onSwordDrawn = onSwordDrawn;
            _onComplete = onComplete;
            _skipEnabledTime = Time.unscaledTime + _config.SkipInputDelay;
            SetSkippable(true);

            _introCoroutine = StartCoroutine(RunIntroSequence());
        }

        /// <summary>
        /// Sets the hero's intro start and battle positions from the battle site
        /// and turns the hero to the battle facing. Called by BattleBootstrapper
        /// before Initialize. Without it the hero stays where the scene placed him.
        /// </summary>
        /// <param name="introPosition">Where the intro walk starts.</param>
        /// <param name="battlePosition">Where the hero walks to and fights.</param>
        /// <param name="battleRotation">Hero facing during the battle.</param>
        public void SetHeroPositions(Vector3 introPosition, Vector3 battlePosition, Quaternion battleRotation)
        {
            _heroIntroPosition = introPosition;
            _heroBattlePosition = battlePosition;
            _hasHeroPositions = true;

            if (_heroRoot != null)
                _heroRoot.rotation = battleRotation;
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

        private IEnumerator RunIntroSequence()
        {
            _tauntEnded = false;

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 0 — camera settle.");
            yield return new WaitForSeconds(_config.CameraSettleDuration);

            float walkDistance = (_heroRoot != null && _hasHeroPositions)
                ? Vector3.Distance(_heroRoot.position, _heroBattlePosition)
                : 0f;

            float walkDuration = walkDistance / Mathf.Max(_config.HeroWalkSpeed, 0.01f);

            GameLogger.Log(LogSourceType.BattleSystem,
                $"Intro Phase 1 — hero walk + slow orbit start. walkDuration={walkDuration:F2} ");

            StartLocomotion();

            // Hero walk and camera slow-orbit run in parallel — neither blocks the other.
            if (_heroRoot != null && _hasHeroPositions && walkDuration > 0f)
            {
                _walkTween = _heroRoot
                    .DOMove(_heroBattlePosition, walkDuration)
                    .SetEase(Ease.Linear);

                _orbitTween = TweenOrbitTo(_tauntPose, walkDuration, _slowOrbitEase);
            }
            else if (_orbitCameraRig != null)
            {
                _orbitCameraRig.SetImmediate(_tauntPose.Yaw, _tauntPose.Pitch, _tauntPose.Distance);
            }

            if (_walkTween != null)
                yield return _walkTween.WaitForCompletion();

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

            _orbitTween = TweenOrbitTo(_enemyRevealPose, _config.FastOrbitDuration, _fastOrbitEase);

            if (_orbitTween != null)
                yield return _orbitTween.WaitForCompletion();
            else
                yield return new WaitForSeconds(_config.FastOrbitDuration);

            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 3 — enemy reveal complete.");

            SetSkippable(false);
            SetCameraPriorities(introActive: false, battleActive: true);
            GameLogger.Log(LogSourceType.BattleSystem, "Intro Phase 4 — swapped to battle camera.");

            yield return CompleteIntro(_config.SweepBackDuration);
        }

        /// <summary>
        /// Final step shared by the normal end and the skip: draws the sword,
        /// reveals the puzzle, waits for the camera handoff and fires the callbacks.
        /// </summary>
        /// <param name="handoffDuration">Seconds to wait for the camera to settle on the battle camera.</param>
        private IEnumerator CompleteIntro(float handoffDuration)
        {
            if (_swordProp != null)
                _swordProp.SetActive(true);

            _onSwordDrawn?.Invoke();

            if (_puzzleRect != null || _puzzleCanvasGroup != null)
                RevealPuzzle();

            yield return new WaitForSeconds(handoffDuration);

            RestoreBrainBlend();
            _introCoroutine = null;

            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController: intro complete.");
            _onComplete?.Invoke();
        }

        // PRIVATE METHODS — Skip

        /// <summary>
        /// Skips the rest of the intro: stops the sequence and its tweens, puts the
        /// hero at the battle position in idle, blends quickly to the battle camera
        /// and runs the shared completion step.
        /// </summary>
        private void SkipIntro()
        {
            SetSkippable(false);

            if (_introCoroutine != null)
                StopCoroutine(_introCoroutine);

            KillSequenceTweens();

            GameLogger.Log(LogSourceType.BattleSystem, "BattleIntroController: intro skipped.");

            if (_heroRoot != null && _hasHeroPositions)
                _heroRoot.position = _heroBattlePosition;

            if (_heroAnimator != null)
            {
                _heroAnimator.SetFloat(AnimLocomotionBlend, 0f);
                _heroAnimator.ResetTrigger(AnimTaunt);
                StopLocomotion();
            }

            if (_isTaunting)
                EndTaunt();

            OverrideBrainBlend(_config.SkipCameraBlendDuration);
            SetCameraPriorities(introActive: false, battleActive: true);

            _introCoroutine = StartCoroutine(CompleteIntro(_config.SkipCameraBlendDuration));
        }

        /// <summary>
        /// True on the frame a mouse click or a new touch begins. Uses the legacy
        /// input API like the rest of the battle input; with touch, Unity also
        /// reports the first touch as mouse button 0.
        /// </summary>
        private static bool IsSkipInputPressed()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
                return true;

            return UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
        }

        /// <summary>Turns skipping on or off and shows or hides the hint with it.</summary>
        private void SetSkippable(bool isSkippable)
        {
            _isSkippable = isSkippable;

            if (isSkippable)
                ShowSkipHint();
            else
                HideSkipHint();
        }

        /// <summary>Kills the walk, orbit and locomotion blend tweens of the sequence.</summary>
        private void KillSequenceTweens()
        {
            _walkTween?.Kill();
            _orbitTween?.Kill();
            _locomotionBlendTween?.Kill();

            _walkTween = null;
            _orbitTween = null;
            _locomotionBlendTween = null;
        }

        /// <summary>Temporarily replaces the brain's default blend with a short one.</summary>
        private void OverrideBrainBlend(float duration)
        {
            if (_brain == null)
            {
                Camera mainCamera = Camera.main;
                _brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;
            }

            if (_brain == null)
                return;

            if (!_isBlendOverridden)
                _originalDefaultBlend = _brain.DefaultBlend;

            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, duration);
            _isBlendOverridden = true;
        }

        /// <summary>Restores the brain's default blend if it was overridden by a skip.</summary>
        private void RestoreBrainBlend()
        {
            if (!_isBlendOverridden || _brain == null)
                return;

            _brain.DefaultBlend = _originalDefaultBlend;
            _isBlendOverridden = false;
        }

        // PRIVATE METHODS — Skip hint

        private void HideSkipHintImmediate()
        {
            if (_skipHintGroup == null)
                return;

            _skipHintGroup.alpha = 0f;
            _skipHintGroup.interactable = false;
            _skipHintGroup.blocksRaycasts = false;
        }

        /// <summary>Fades the hint in, then pulses its alpha until it is hidden.</summary>
        private void ShowSkipHint()
        {
            if (_skipHintGroup == null)
                return;

            _skipHintTween?.Kill();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_skipHintGroup.DOFade(1f, SkipHintFadeDuration));
            sequence.Append(_skipHintGroup.DOFade(SkipHintPulseMinAlpha, SkipHintPulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
            _skipHintTween = sequence;
        }

        private void HideSkipHint()
        {
            if (_skipHintGroup == null)
                return;

            _skipHintTween?.Kill();
            _skipHintTween = _skipHintGroup.DOFade(0f, SkipHintFadeDuration);
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

            _locomotionBlendTween = DOVirtual.Float(1f, 0f, LocomotionBlendOutDuration,
                value => _heroAnimator.SetFloat(AnimLocomotionBlend, value));

            yield return _locomotionBlendTween.WaitForCompletion();
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

            _isTaunting = true;
            TauntStarted?.Invoke();
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

            _isTaunting = false;
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