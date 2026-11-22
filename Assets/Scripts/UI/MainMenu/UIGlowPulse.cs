using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MightyHeroes.UI.MainMenu
{
    /// <summary>
    /// Pulses a UI glow image: scale and alpha follow a shared sine wave on
    /// unscaled time, with optional constant rotation. Used for the soft halo
    /// behind main menu buttons. Disables raycasts on the glow image so it
    /// never blocks the button underneath.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIGlowPulse : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [Tooltip("Glow rect to scale and rotate. Defaults to this object's RectTransform.")]
        [FormerlySerializedAs("glowRect")]
        [SerializeField] private RectTransform _glowRect;

        [Tooltip("Glow image to fade. Defaults to this object's Image.")]
        [FormerlySerializedAs("glowImage")]
        [SerializeField] private Image _glowImage;

        [Header("Scale Pulse")]
        [FormerlySerializedAs("minScale")]
        [SerializeField] private float _minScale = 0.95f;

        [FormerlySerializedAs("maxScale")]
        [SerializeField] private float _maxScale = 1.15f;

        [Tooltip("Pulse speed in radians per second.")]
        [FormerlySerializedAs("pulseSpeed")]
        [SerializeField] private float _pulseSpeed = 2f;

        [Header("Alpha Pulse")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("minAlpha")]
        [SerializeField] private float _minAlpha = 0.25f;

        [Range(0f, 1f)]
        [FormerlySerializedAs("maxAlpha")]
        [SerializeField] private float _maxAlpha = 0.75f;

        [Header("Optional Rotation")]
        [FormerlySerializedAs("rotate")]
        [SerializeField] private bool _rotate;

        [Tooltip("Rotation speed in degrees per second.")]
        [FormerlySerializedAs("rotationSpeed")]
        [SerializeField] private float _rotationSpeed = 25f;

        private Vector3 _initialScale;
        private Color _initialColor;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_glowRect == null)
                _glowRect = GetComponent<RectTransform>();

            if (_glowImage == null)
                _glowImage = GetComponent<Image>();

            if (_glowRect != null)
                _initialScale = _glowRect.localScale;

            if (_glowImage != null)
            {
                _initialColor = _glowImage.color;
                _glowImage.raycastTarget = false;
            }
        }

        private void Update()
        {
            if (_glowRect == null || _glowImage == null)
                return;

            UpdatePulse();
        }

        // PUBLIC METHODS

        /// <summary>Shows or hides the glow image without stopping the pulse.</summary>
        /// <param name="isVisible">True to show the glow.</param>
        public void SetGlowVisible(bool isVisible)
        {
            if (_glowImage != null)
                _glowImage.enabled = isVisible;
        }

        /// <summary>Changes the glow base color. Alpha is still driven by the pulse.</summary>
        /// <param name="color">New base color.</param>
        public void SetGlowColor(Color color)
        {
            if (_glowImage == null)
                return;

            _initialColor = color;
            _glowImage.color = color;
        }

        // PRIVATE METHODS

        /// <summary>Applies the current pulse value to scale, alpha and rotation.</summary>
        private void UpdatePulse()
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * _pulseSpeed) + 1f) * 0.5f;

            _glowRect.localScale = _initialScale * Mathf.Lerp(_minScale, _maxScale, pulse);

            Color color = _initialColor;
            color.a = Mathf.Lerp(_minAlpha, _maxAlpha, pulse);
            _glowImage.color = color;

            if (_rotate)
                _glowRect.Rotate(0f, 0f, _rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
