using System.Collections;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Equipment.Data;
using UnityEngine;

namespace MightyHeroes.Gameplay.Equipment.Views
{
    /// <summary>
    /// Spawns the weapon prefab onto the weapon socket and plays visual effects.
    /// BladeTipSocket is created at runtime from WeaponVisualConfig data —
    /// the original Synty prefab asset is never modified.
    /// Trail is driven by AttackStateBehaviour via BeginTrail / EndTrail.
    /// Hit impact is triggered by the combat system via PlayHitImpact.
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        // ─── FIELDS ───────────────────────────────────────────────────

        [Header("Socket")]
        [Tooltip("Assign Prop_R_Socket for Synty characters.")]
        [SerializeField] private Transform _weaponSocket;

        [Header("Default Weapon")]
        [SerializeField] private WeaponVisualConfig _defaultWeapon;

        private GameObject         _weaponInstance;
        private WeaponVisualConfig _currentConfig;
        private TrailRenderer      _trailRenderer;

        // ─── PROPERTIES ───────────────────────────────────────────────

        /// <summary>Gets the currently equipped weapon config.</summary>
        public WeaponVisualConfig CurrentConfig => _currentConfig;

        // ─── UNITY CALLBACKS ──────────────────────────────────────────

        private void Start()
        {
            if (_defaultWeapon != null)
                Equip(_defaultWeapon);
        }

        // ─── PUBLIC METHODS ───────────────────────────────────────────

        /// <summary>
        /// Equips the given weapon config.
        /// Destroys the previous instance, spawns the new prefab,
        /// and creates the BladeTipSocket with TrailRenderer at runtime.
        /// </summary>
        /// <param name="config">Weapon visual config to equip.</param>
        public void Equip(WeaponVisualConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning($"{nameof(WeaponView)}: Config is null.", this);
                return;
            }

            if (_weaponSocket == null)
            {
                Debug.LogError($"{nameof(WeaponView)}: Weapon socket not assigned.", this);
                return;
            }

            if (config.WeaponPrefab == null)
            {
                Debug.LogError($"{nameof(WeaponView)}: WeaponPrefab missing in '{config.name}'.", this);
                return;
            }

            if (_currentConfig == config)
                return;

            ClearWeapon();

            _weaponInstance = Instantiate(config.WeaponPrefab, _weaponSocket);
            _weaponInstance.transform.localPosition = config.LocalPosition;
            _weaponInstance.transform.localRotation = Quaternion.Euler(config.LocalEulerAngles);
            _weaponInstance.transform.localScale    = config.LocalScale;

            _currentConfig = config;
            _trailRenderer = CreateTrail(config);
        }

        /// <summary>Removes the current weapon instance and clears state.</summary>
        public void Unequip()
        {
            ClearWeapon();
            _currentConfig = null;
        }

        /// <summary>
        /// Starts trail emission.
        /// Called by AttackStateBehaviour on WindUp state entry.
        /// </summary>
        public void BeginTrail()
        {
            if (_trailRenderer == null)
                return;

            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }

        /// <summary>
        /// Stops trail emission.
        /// Called by AttackStateBehaviour on FollowThrough state exit.
        /// </summary>
        public void EndTrail()
        {
            if (_trailRenderer == null)
                return;

            _trailRenderer.emitting = false;
        }

        /// <summary>
        /// Plays the slash arc effect for a normal swing.
        /// Called by AttackStateBehaviour on WindUp state entry.
        /// </summary>
        /// <param name="hitDirection">Swing direction for correct X-scale mirroring.</param>
        public void PlaySlashEffect(HitDirection hitDirection)
        {
            if (_currentConfig?.SlashEffectPrefab == null)
                return;

            SpawnEffect(
                _currentConfig.SlashEffectPrefab,
                _weaponSocket,
                hitDirection,
                _currentConfig.SlashEffectLifetime);
        }

        /// <summary>
        /// Plays the special attack effect.
        /// Called by AttackStateBehaviour on special WindUp state entry.
        /// </summary>
        /// <param name="hitDirection">Swing direction for correct X-scale mirroring.</param>
        public void PlaySpecialEffect(HitDirection hitDirection)
        {
            if (_currentConfig?.SpecialEffectPrefab == null)
                return;

            SpawnEffect(
                _currentConfig.SpecialEffectPrefab,
                _weaponSocket,
                hitDirection,
                _currentConfig.SpecialEffectLifetime);
        }

        /// <summary>
        /// Plays the hit impact effect at the target position.
        /// Called by the combat system when a hit is confirmed.
        /// </summary>
        /// <param name="hitSocket">World-space socket on the target character.</param>
        public void PlayHitImpact(Transform hitSocket)
        {
            if (_currentConfig?.HitImpactPrefab == null || hitSocket == null)
                return;

            GameObject instance = Instantiate(_currentConfig.HitImpactPrefab, hitSocket);
            StartCoroutine(DestroyAfter(instance, _currentConfig.HitImpactLifetime));
        }

        // ─── PRIVATE METHODS ──────────────────────────────────────────

        /// <summary>
        /// Creates a BladeTipSocket GameObject as a runtime child of the weapon instance
        /// at the position defined in the config, then adds and configures a TrailRenderer.
        /// The original Synty prefab asset is never modified.
        /// </summary>
        /// <param name="config">Weapon visual config containing trail and blade tip data.</param>
        private TrailRenderer CreateTrail(WeaponVisualConfig config)
        {
            if (_weaponInstance == null)
                return null;

            // Reuse if already exists (e.g. re-equip same weapon).
            Transform tip = _weaponInstance.transform.Find("BladeTipSocket");

            if (tip == null)
            {
                GameObject socket = new GameObject("BladeTipSocket");
                socket.transform.SetParent(_weaponInstance.transform, false);
                socket.transform.localPosition = config.BladeTipLocalPosition;
                tip = socket.transform;
            }

            TrailRenderer trail = tip.GetComponent<TrailRenderer>();

            if (trail == null)
                trail = tip.gameObject.AddComponent<TrailRenderer>();

            ApplyTrailConfig(trail, config);
            trail.emitting = false;
            return trail;
        }

        /// <summary>
        /// Applies all trail visual settings from the config to the given TrailRenderer.
        /// </summary>
        /// <param name="trail">TrailRenderer to configure.</param>
        /// <param name="config">Source config.</param>
        private static void ApplyTrailConfig(TrailRenderer trail, WeaponVisualConfig config)
        {
            trail.time              = config.TrailTime;
            trail.startWidth        = config.TrailStartWidth;
            trail.endWidth          = config.TrailEndWidth;
            trail.minVertexDistance = config.TrailMinVertexDistance;

            if (config.TrailColor != null)
                trail.colorGradient = config.TrailColor;

            if (config.TrailMaterial != null)
                trail.material = config.TrailMaterial;
        }

        /// <summary>
        /// Spawns an effect prefab under the given parent.
        /// Mirrors X-scale for Left-direction swings.
        /// </summary>
        private void SpawnEffect(GameObject prefab, Transform parent, HitDirection hitDirection, float lifetime)
        {
            if (parent == null)
                return;

            GameObject instance = Instantiate(prefab, parent);
            float      xScale   = hitDirection == HitDirection.Left ? -1f : 1f;

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale    = new Vector3(xScale, 1f, 1f);

            StartCoroutine(DestroyAfter(instance, lifetime));
        }

        private void ClearWeapon()
        {
            if (_weaponInstance != null)
            {
                Destroy(_weaponInstance);
                _weaponInstance = null;
            }

            _trailRenderer = null;
        }

        private IEnumerator DestroyAfter(GameObject instance, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);

            if (instance != null)
                Destroy(instance);
        }
    }
}