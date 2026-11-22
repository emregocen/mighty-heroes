using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Plays a particle effect on its entity for each support action (heal,
    /// shield, buff) when a SupportEffectRequestedEvent targets the entity.
    ///
    /// Each entry is spawned once under the spawn anchor and replayed on every
    /// request, so nothing is instantiated during battle. The source prefabs
    /// are never modified; settings that would break replaying (play on awake,
    /// loop, stop action) are overridden on the spawned instances. An entry can
    /// tint its effect, so one generic Synty effect can be reused in different
    /// colors. The tint replaces the particles' start color; a color over
    /// lifetime gradient keeps its alpha but loses its own hue (otherwise it
    /// would keep recoloring the particles). Particle shaders that use vertex
    /// colors are tinted that way. Synty's shader graph materials ignore vertex
    /// colors and use _BaseColor instead, so for those the tint is also set
    /// through a material property block; shared materials are not modified.
    /// </summary>
    public sealed class SupportEffectView : MonoBehaviour
    {
        // CONSTANTS

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        // FIELDS

        [Tooltip("Parent of the spawned effects. Empty uses this object.")]
        [SerializeField] private Transform _spawnAnchor;

        [Tooltip("One effect per support action.")]
        [SerializeField] private EffectEntry[] _effects = Array.Empty<EffectEntry>();

        private EventHub _eventHub;
        private int _entityId;
        private SpawnedEffect[] _spawnedEffects = Array.Empty<SpawnedEffect>();

        // UNITY CALLBACKS

        private void Awake()
        {
            SpawnEffects();
        }

        private void OnDestroy()
        {
            if (_eventHub != null)
                _eventHub.CombatEvents.SupportEffectRequestedEvent.Unregister(OnSupportEffectRequested);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Subscribes to support effect requests for the given entity.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="entityId">Runtime id of the entity that owns this view.</param>
        public void Initialize(EventHub eventHub, int entityId)
        {
            if (eventHub == null)
                return;

            _eventHub = eventHub;
            _entityId = entityId;

            _eventHub.CombatEvents.SupportEffectRequestedEvent.Register(OnSupportEffectRequested);
        }

        // PRIVATE METHODS

        /// <summary>
        /// Instantiates every configured effect under the anchor, prepares it
        /// for replaying, applies the tint and keeps it stopped and hidden.
        /// </summary>
        private void SpawnEffects()
        {
            Transform parent = _spawnAnchor != null ? _spawnAnchor : transform;
            _spawnedEffects = new SpawnedEffect[_effects.Length];

            for (int i = 0; i < _effects.Length; i++)
            {
                EffectEntry entry = _effects[i];

                if (entry.Prefab == null)
                    continue;

                GameObject instance = Instantiate(entry.Prefab, parent);
                instance.transform.localPosition = entry.LocalPosition;
                instance.transform.localRotation = Quaternion.identity;

                ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);

                foreach (ParticleSystem system in systems)
                {
                    ParticleSystem.MainModule main = system.main;
                    main.playOnAwake = false;
                    main.loop = false;
                    main.stopAction = ParticleSystemStopAction.None;

                    if (entry.Tint.a > 0f)
                        ApplyParticleTint(system, entry.Tint);

                    system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                if (entry.Tint.a > 0f)
                    ApplyRendererTint(instance, entry.Tint);

                instance.SetActive(false);
                _spawnedEffects[i] = new SpawnedEffect(entry.ActionType, instance, systems);
            }
        }

        /// <summary>
        /// Sets the tint as the particle start color and turns a color over
        /// lifetime gradient white, keeping its alpha, so it only fades.
        /// </summary>
        private static void ApplyParticleTint(ParticleSystem system, Color tint)
        {
            ParticleSystem.MainModule main = system.main;
            main.startColor = tint;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;

            if (!colorOverLifetime.enabled)
                return;

            ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;

            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Gradient:
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(ToAlphaOnly(gradient.gradient));
                    break;

                case ParticleSystemGradientMode.TwoGradients:
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(ToAlphaOnly(gradient.gradientMin), ToAlphaOnly(gradient.gradientMax));
                    break;

                case ParticleSystemGradientMode.Color:
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, gradient.color.a));
                    break;

                case ParticleSystemGradientMode.TwoColors:
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, gradient.colorMin.a), new Color(1f, 1f, 1f, gradient.colorMax.a));
                    break;
            }
        }

        /// <summary>Returns a copy of the gradient with white color keys and the original alpha keys.</summary>
        private static Gradient ToAlphaOnly(Gradient source)
        {
            Gradient result = new Gradient();

            if (source == null)
                return result;

            result.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, source.alphaKeys);
            return result;
        }

        /// <summary>
        /// Tints the renderers whose material has a _BaseColor property (shader
        /// graph materials that ignore vertex colors) through a property block.
        /// </summary>
        private static void ApplyRendererTint(GameObject instance, Color tint)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            foreach (Renderer effectRenderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material material = effectRenderer.sharedMaterial;

                if (material == null || !material.HasProperty(BaseColorId))
                    continue;

                effectRenderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, tint);
                effectRenderer.SetPropertyBlock(block);
            }
        }

        /// <summary>Restarts every particle system of the given spawned effect.</summary>
        private static void PlayEffect(SpawnedEffect effect)
        {
            if (!effect.Instance.activeSelf)
                effect.Instance.SetActive(true);

            foreach (ParticleSystem system in effect.Systems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(false);
            }
        }

        // EVENT HANDLERS

        private void OnSupportEffectRequested(SupportEffectRequestedPayload payload)
        {
            if (payload.TargetEntityId != _entityId)
                return;

            foreach (SpawnedEffect effect in _spawnedEffects)
            {
                if (effect != null && effect.ActionType == payload.ActionType)
                    PlayEffect(effect);
            }
        }

        // NESTED TYPES

        /// <summary>Inspector entry: which effect plays for which support action.</summary>
        [Serializable]
        private struct EffectEntry
        {
            [Tooltip("Support action that plays this effect (Heal, Shield or Buff).")]
            public ActionType ActionType;

            [Tooltip("Particle effect prefab.")]
            public GameObject Prefab;

            [Tooltip("Position relative to the spawn anchor.")]
            public Vector3 LocalPosition;

            [Tooltip("Start color for every particle system of the effect. Alpha 0 keeps the prefab's own colors.")]
            public Color Tint;
        }

        /// <summary>A spawned, replayable effect instance.</summary>
        private sealed class SpawnedEffect
        {
            public SpawnedEffect(ActionType actionType, GameObject instance, ParticleSystem[] systems)
            {
                ActionType = actionType;
                Instance = instance;
                Systems = systems;
            }

            public ActionType ActionType { get; }
            public GameObject Instance { get; }
            public ParticleSystem[] Systems { get; }
        }
    }
}
