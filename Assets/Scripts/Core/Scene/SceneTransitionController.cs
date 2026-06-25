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
            if (_isTransitioning)
                return;

            _isTransitioning = true;

            GameLogger.Log(LogSourceType.UI,
                $"SceneTransitionController: transitioning to {sceneName}");

            FadeOut(() =>
            {
                SceneManager.LoadScene(sceneName);
                FadeIn(() => _isTransitioning = false);
            });
        }

        // PRIVATE METHODS

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