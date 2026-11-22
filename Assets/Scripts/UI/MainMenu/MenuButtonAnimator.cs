using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Press feedback for a main menu button: shrinks on press and bounces back
    /// on release. Scale only; images are never changed. Runs on unscaled time.
    /// Put it on the button object; clicks are still handled by the Button.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        // FIELDS

        [Header("Target")]
        [Tooltip("Transform to scale. Defaults to this object's RectTransform.")]
        [SerializeField] private RectTransform _target;

        [Header("Press")]
        [Tooltip("Scale multiplier while the button is held down.")]
        [SerializeField] private float _pressedScale = 0.92f;

        [Tooltip("Duration of the press shrink in seconds.")]
        [SerializeField] private float _pressDuration = 0.08f;

        [Tooltip("Duration of the release bounce in seconds.")]
        [SerializeField] private float _releaseDuration = 0.35f;

        private Vector3 _baseScale;
        private Tween _pressTween;
        private bool _isPressed;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            _baseScale = _target.localScale;
        }

        private void OnDisable()
        {
            _pressTween?.Kill();
            _isPressed = false;

            if (_target != null)
                _target.localScale = _baseScale;
        }

        // PRIVATE METHODS

        /// <summary>Scales back to rest with a bounce.</summary>
        private void Release()
        {
            if (!_isPressed)
                return;

            _isPressed = false;
            _pressTween?.Kill();
            _pressTween = _target.DOScale(_baseScale, _releaseDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        // EVENT HANDLERS

        /// <summary>Shrinks the button while it is held down.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _pressTween?.Kill();
            _pressTween = _target.DOScale(_baseScale * _pressedScale, _pressDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        /// <summary>Releases the press when the pointer is lifted.</summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        /// <summary>Releases the press when the pointer is dragged off the button.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }
    }
}
