using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Controls hero presentation in the main menu scene.
    ///
    /// Cycles through a designer-specified list of IdleVariant values so only
    /// menu-appropriate animations are played. Picks randomly without immediate repeats.
    /// </summary>
    public sealed class MenuHeroController : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [SerializeField] private Animator _animator;

        [Header("Idle Variants")]
        [Tooltip("Animator IdleVariant values to use in the menu. " +
                 "Add the exact integer values that match menu-appropriate animations.")]
        [SerializeField] private int[] _allowedVariants = { 0, 1, 2 };

        [Tooltip("Minimum seconds before switching to a new idle variant.")]
        [SerializeField] private float _minSwitchInterval = 4f;

        [Tooltip("Maximum seconds before switching to a new idle variant.")]
        [SerializeField] private float _maxSwitchInterval = 8f;

        private static readonly int AnimIdle = Animator.StringToHash("Gesture");
        private static readonly int AnimIdleVariant = Animator.StringToHash("GestureVariant");
        private int _currentVariantIndex = -1;
        private float _switchTimer;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_animator == null)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MenuHeroController: Animator reference is missing.");
                return;
            }

            if (_allowedVariants == null || _allowedVariants.Length == 0)
            {
                GameLogger.Log(LogSourceType.UI,
                    "MenuHeroController: No allowed variants configured.");
                return;
            }

            SetRandomVariant();
            ScheduleNextSwitch();
        }

        private void Update()
        {
            if (_animator == null)
                return;

            _switchTimer -= Time.deltaTime;

            if (_switchTimer <= 0f)
            {
                SetRandomVariant();
                ScheduleNextSwitch();
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Picks a random index from _allowedVariants that differs from the current one,
        /// then sends the corresponding GestureVariant value to the Animator.
        /// Briefly sets Gesture to false so the sub-state machine can re-enter
        /// and pick up the new variant value on the next transition.
        /// </summary>
        private void SetRandomVariant()
        {
            int nextIndex = _currentVariantIndex;

            if (_allowedVariants.Length > 1)
            {
                while (nextIndex == _currentVariantIndex)
                    nextIndex = Random.Range(0, _allowedVariants.Length);
            }
            else
            {
                nextIndex = 0;
            }

            _currentVariantIndex = nextIndex;
            int variantValue = _allowedVariants[_currentVariantIndex];

            GameLogger.Log(LogSourceType.UI,
                $"MenuHeroController: switching to GestureVariant={variantValue}");

            StartCoroutine(ApplyVariantNextFrame(variantValue));
        }

        /// <summary>
        /// Sets Gesture false for one frame so the sub-state machine exits cleanly,
        /// then re-enters with the new GestureVariant value.
        /// </summary>
        private System.Collections.IEnumerator ApplyVariantNextFrame(int variantValue)
        {
            _animator.SetBool(AnimIdle, false);

            yield return null;

            _animator.SetInteger(AnimIdleVariant, variantValue);
            _animator.SetBool(AnimIdle, true);
        }

        /// <summary>
        /// Schedules the next variant switch at a random time within the configured range.
        /// </summary>
        private void ScheduleNextSwitch()
        {
            _switchTimer = Random.Range(_minSwitchInterval, _maxSwitchInterval);
        }
    }
}