using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIGlowPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform glowRect;
    [SerializeField] private Image glowImage;

    [Header("Scale Pulse")]
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.15f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Alpha Pulse")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.75f;

    [Header("Optional Rotation")]
    [SerializeField] private bool rotate;
    [SerializeField] private float rotationSpeed = 25f;

    private Vector3 _initialScale;
    private Color _initialColor;

    private void Awake()
    {
        if (glowRect == null)
            glowRect = GetComponent<RectTransform>();

        if (glowImage == null)
            glowImage = GetComponent<Image>();

        if (glowRect != null)
            _initialScale = glowRect.localScale;

        if (glowImage != null)
        {
            _initialColor = glowImage.color;
            glowImage.raycastTarget = false;
        }
    }

    private void Update()
    {
        if (glowRect == null || glowImage == null)
            return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        float scale = Mathf.Lerp(minScale, maxScale, t);
        glowRect.localScale = _initialScale * scale;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color color = _initialColor;
        color.a = alpha;
        glowImage.color = color;

        if (rotate)
        {
            glowRect.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
        }
    }

    public void SetGlowVisible(bool isVisible)
    {
        if (glowImage != null)
            glowImage.enabled = isVisible;
    }

    public void SetGlowColor(Color color)
    {
        if (glowImage == null)
            return;

        _initialColor = color;
        glowImage.color = color;
    }
}