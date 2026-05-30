using TMPro;
using UnityEngine;

/// <summary>
/// Holds reusable TextMeshPro style configuration data.
/// Used for consistent UI text styling across the project.
/// </summary>
[CreateAssetMenu(menuName = "Mighty Heroes/UI/Text Style Config")]
public sealed class TextStyleConfig : ScriptableObject
{
    // 1) TEXT COLORS

    [Header("Face")]
    public Color FaceColor = Color.white;

    [Header("Outline")]
    public Color OutlineColor = Color.black;
    [Range(0f, 1f)] public float OutlineWidth = 0.2f;

    [Header("Underlay (Shadow)")]
    public bool UseUnderlay;
    public Color UnderlayColor = Color.black;
    public Vector2 UnderlayOffset = new Vector2(1f, -1f);
    [Range(0f, 1f)] public float UnderlaySoftness = 0.3f;

    [Header("Glow")]
    public bool UseGlow;
    public Color GlowColor = Color.yellow;
    [Range(0f, 1f)] public float GlowPower = 0.3f;
}