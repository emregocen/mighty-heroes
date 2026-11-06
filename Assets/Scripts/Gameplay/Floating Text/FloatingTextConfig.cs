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
            public float FontSize = 60f;

            [Tooltip("Bold/Italic/Normal for this type.")]
            public FontStyles FontStyle = FontStyles.Normal;

            [Header("Motion")]
            [Tooltip("Total seconds the text is visible before it's returned to the pool.")]
            public float Lifetime = 0.9f;

            [Tooltip("How far upward (canvas reference pixels) the text drifts over its lifetime.")]
            public float RiseDistance = 70f;

            [Tooltip("Degrees of random tilt applied on spawn, in [-value, value].")]
            public float RandomTiltDegrees = 8f;

            [Tooltip("Random offset radius (canvas reference pixels) applied on spawn so simultaneous texts don't overlap exactly.")]
            public float RandomOffsetRadius = 25f;

            [Header("Scale")]
            public float StartScale = 0.6f;
            public float PunchInDuration = 0.12f;

            [Header("Fade")]
            [Range(0f, 1f)] public float FadeStartFraction = 0.6f;
        }

        // FIELDS

        [Header("Pool")]
        [SerializeField, Min(1)] private int _poolSizePerPrefab = 8;

        [Header("Stacking (anchored texts)")]
        [Tooltip("Seconds after the last spawn on an entity during which a new text still stacks above it.")]
        [SerializeField, Min(0f)] private float _stackWindowSeconds = 0.6f;

        [Tooltip("Vertical distance in canvas reference pixels between stacked texts on the same entity.")]
        [SerializeField, Min(0f)] private float _stackSpacing = 56f;

        [Header("Per-Type Settings")]
        [SerializeField] private List<TypeSettings> _typeSettings = new List<TypeSettings>();

        // PROPERTIES

        /// <summary>Number of pre-warmed instances per prefab pool.</summary>
        public int PoolSizePerPrefab => _poolSizePerPrefab;

        /// <summary>Stack window in seconds for anchored texts.</summary>
        public float StackWindowSeconds => _stackWindowSeconds;

        /// <summary>Stack spacing in canvas reference pixels for anchored texts.</summary>
        public float StackSpacing => _stackSpacing;

        // PUBLIC METHODS

        /// <summary>Returns the settings for the given type, or defaults if none are configured.</summary>
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