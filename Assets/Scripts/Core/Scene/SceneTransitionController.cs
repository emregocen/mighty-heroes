using System.Collections;
using DG.Tweening;
using MightyHeroes.Core.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MightyHeroes.Core.Scene
{
    /// <summary>
    /// Handles scene transitions with a full-screen fade overlay.
    ///
    /// Persist across scenes via DontDestroyOnLoad. Only one instance
    /// is ever active — any duplicate created by scene load is destroyed immediately.
    ///
    /// Usage:
    ///   SceneTransitionController.Instance.LoadScene(SceneNames.Battle);
    /// </summary>
    public sealed class SceneTransitionController : MonoBehaviour
    {
        // CONSTANTS

        private const float FadeOutDuration = 0.4f;
        private const float FadeInDuration = 0.5f;
        private const float FadeInDelay = 0.1f;

        // FIELDS

        [Header("References")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        private static SceneTransitionController _instance;
        private bool _isTransitioning;
        private Camera _transitionCamera;

        // PROPERTIES

        /// <summary>Singleton instance. Available after the first MainMenuScene load.</summary>
        public static SceneTransitionController Instance => _instance;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_fadeCanvasGroup != null)
                _fadeCanvasGroup.alpha = 0f;

            CreateTransitionCamera();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Fades out, loads the target scene, then fades in.
        /// No-op if a transition is already in progress.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            LoadScenes(sceneName);
        }

        /// <summary>
        /// Fades out, loads the first scene as the single (active) scene and the
        /// others additively in order, then fades in once all are loaded.
        /// Used for battles: the environment scene first (its lighting stays
        /// active), then BattleScene on top. No-op while a transition runs.
        /// </summary>
        /// <param name="sceneNames">Scenes to load; the first one becomes the active scene.</param>
        public void LoadScenes(params string[] sceneNames)
        {
            if (_isTransitioning || sceneNames == null || sceneNames.Length == 0)
                return;

            _isTransitioning = true;

            GameLogger.Log(LogSourceType.UI, $"SceneTransitionController: transitioning to {string.Join(" + ", sceneNames)}");

            FadeOut(() => StartCoroutine(LoadScenesRoutine(sceneNames)));
        }

        // PRIVATE METHODS

        /// <summary>
        /// Creates a disabled camera that renders nothing but a black clear.
        /// It is on only while scenes load, so there is never a frame without
        /// any camera (for example when the environment scene, which has no
        /// camera, is loaded before BattleScene).
        /// </summary>
        private void CreateTransitionCamera()
        {
            GameObject cameraObject = new GameObject("Transition Camera");
            cameraObject.transform.SetParent(transform, false);

            _transitionCamera = cameraObject.AddComponent<Camera>();
            _transitionCamera.clearFlags = CameraClearFlags.SolidColor;
            _transitionCamera.backgroundColor = Color.black;
            _transitionCamera.cullingMask = 0;
            _transitionCamera.depth = -100f;
            _transitionCamera.enabled = false;
        }

        /// <summary>Loads the scenes one after another with the transition camera on, then fades in.</summary>
        private IEnumerator LoadScenesRoutine(string[] sceneNames)
        {
            _transitionCamera.enabled = true;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                LoadSceneMode mode = i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive;
                yield return SceneManager.LoadSceneAsync(sceneNames[i], mode);
            }

            _transitionCamera.enabled = false;

            FadeIn(() => _isTransitioning = false);
        }

        /// <summary>Fades the overlay to opaque then calls onComplete.</summary>
        private void FadeOut(TweenCallback onComplete)
        {
            if (_fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.gameObject.SetActive(true);

            _fadeCanvasGroup
                .DOFade(1f, FadeOutDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(onComplete);
        }

        /// <summary>Fades the overlay to transparent after a short delay then calls onComplete.</summary>
        private void FadeIn(TweenCallback onComplete)
        {
            if (_fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            DOVirtual.DelayedCall(FadeInDelay, () =>
            {
                _fadeCanvasGroup
                    .DOFade(0f, FadeInDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(onComplete);
            }, ignoreTimeScale: true);
        }
    }
}