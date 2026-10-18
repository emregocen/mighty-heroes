using System;
using System.Collections.Generic;
using MightyHeroes.Gameplay.FloatingText.Types;
using TMPro;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Per-type animation, sizing, and style-reference settings for floating combat text.
    ///
    /// Appearance (face color, outline, underlay, glow) is delegated to a
    /// TextStyleConfig asset via TextStyleApplier — the same styling system
    /// used across the project's UI — so floating text stays visually
    /// consistent with the rest of the HUD and can be restyled from a single
    /// shared asset without touching code.
    /// </summary>
    [CreateAssetMenu(
        fileName = "FloatingTextConfig",
        menuName = "Mighty Heroes/Gameplay/Floating Text Config")]
    public sealed class FloatingTextConfig : ScriptableObject
    {
        // NESTED TYPES

        /// <summary>Settings for a single FloatingTextType.</summary>
        [Serializable]
        public sealed class TypeSettings
        {
            public FloatingTextType Type;

            [Header("Style")]
            [Tooltip("Shared TextStyleConfig asset controlling face color, outline, underlay, and glow. " +
                     "Reuse the same asset across types for a consistent look, or use a distinct one " +
                     "per type (e.g. a bolder K.O./Overkill style).")]
            public TextStyleConfig Style;

            [Tooltip("Font size for this type. Not part of TextStyleConfig since it's a per-context " +
                     "sizing decision (K.O. reads bigger than a damage number), not shared material style.")]
            public float FontSize = 8f;

            [Tooltip("Bold/Italic/Normal for this type.")]
            public FontStyles FontStyle = FontStyles.Normal;

            [Header("Motion")]
            [Tooltip("Total seconds the text is visible before it's returned to the pool.")]
            public float Lifetime = 0.9f;

            [Tooltip("How far upward (world units) the text drifts over its lifetime.")]
            public float RiseDistance = 0.8f;

            [Tooltip("Degrees of random tilt applied on spawn, in [-value, value].")]
            public float RandomTiltDegrees = 8f;

            [Tooltip("Random offset radius applied on spawn so simultaneous texts don't overlap exactly.")]
            public float RandomOffsetRadius = 0.25f;

            [Header("Scale")]
            public float StartScale = 0.6f;
            public float PunchInDuration = 0.12f;

            [Header("Fade")]
            [Range(0f, 1f)] public float FadeStartFraction = 0.6f;
        }

        // FIELDS

        [Header("Pool")]
        [SerializeField, Min(1)] private int _poolSizePerPrefab = 8;

        [Header("Per-Type Settings")]
        [SerializeField] private List<TypeSettings> _typeSettings = new List<TypeSettings>();

        // PROPERTIES

        public int PoolSizePerPrefab => _poolSizePerPrefab;

        // PUBLIC METHODS

        public TypeSettings GetSettings(FloatingTextType type)
        {
            foreach (TypeSettings settings in _typeSettings)
            {
                if (settings.Type == type)
                    return settings;
            }

            return new TypeSettings { Type = type };
        }
    }
}