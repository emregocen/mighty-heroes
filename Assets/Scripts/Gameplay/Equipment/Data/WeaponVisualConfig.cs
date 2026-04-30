using UnityEngine;

namespace MightyHeroes.Gameplay.Equipment.Data
{
    /// <summary>
    /// Visual configuration for a single weapon.
    /// Holds the prefab, mount transform offsets, blade tip position,
    /// trail settings, and effect prefabs.
    /// No combat or gameplay logic.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WVC_",
        menuName = "Mighty Heroes/Equipment/Weapon Visual Config")]
    public sealed class WeaponVisualConfig : ScriptableObject
    {
        // ─── FIELDS ───────────────────────────────────────────────────

        [Header("Prefab")]
        [SerializeField] private GameObject _weaponPrefab;

        [Header("Mount Transform")]
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;

        [Header("Trail — Blade Tip")]
        [Tooltip("Local position of the blade tip on the weapon prefab. " +
                 "BladeTipSocket is created here at runtime. No prefab modification needed.")]
        [SerializeField] private Vector3 _bladeTipLocalPosition = new Vector3(0f, 1f, 0f);

        [Header("Trail — Appearance")]
        [SerializeField] private Material      _trailMaterial;
        [SerializeField] private float         _trailTime             = 0.12f;
        [SerializeField] private float         _trailStartWidth       = 0.05f;
        [SerializeField] private float         _trailEndWidth         = 0.00f;
        [SerializeField] private float         _trailMinVertexDistance = 0.04f;
        [SerializeField] private Gradient      _trailColor;

        [Header("Effects")]
        [Tooltip("Particle played along the blade during a normal swing.")]
        [SerializeField] private GameObject _slashEffectPrefab;

        [Tooltip("Particle played during a special or ultimate attack.")]
        [SerializeField] private GameObject _specialEffectPrefab;

        [Tooltip("Particle spawned at the hit point on the target.")]
        [SerializeField] private GameObject _hitImpactPrefab;

        [Header("Effect Lifetime")]
        [SerializeField] private float _slashEffectLifetime    = 0.8f;
        [SerializeField] private float _specialEffectLifetime  = 1.2f;
        [SerializeField] private float _hitImpactLifetime      = 0.6f;

        // ─── PROPERTIES ───────────────────────────────────────────────

        /// <summary>Gets the weapon prefab spawned on the weapon socket.</summary>
        public GameObject WeaponPrefab => _weaponPrefab;

        /// <summary>Gets the local position offset applied after mounting.</summary>
        public Vector3 LocalPosition => _localPosition;

        /// <summary>Gets the local rotation offset as Euler angles.</summary>
        public Vector3 LocalEulerAngles => _localEulerAngles;

        /// <summary>Gets the local scale applied after mounting.</summary>
        public Vector3 LocalScale => _localScale;

        /// <summary>
        /// Gets the local position of the blade tip on the weapon prefab.
        /// BladeTipSocket is created at this position at runtime.
        /// </summary>
        public Vector3 BladeTipLocalPosition => _bladeTipLocalPosition;

        /// <summary>Gets the material applied to the TrailRenderer.</summary>
        public Material TrailMaterial => _trailMaterial;

        /// <summary>Gets how long the trail remains visible in seconds.</summary>
        public float TrailTime => _trailTime;

        /// <summary>Gets the trail width at the start (newest) point.</summary>
        public float TrailStartWidth => _trailStartWidth;

        /// <summary>Gets the trail width at the end (oldest) point.</summary>
        public float TrailEndWidth => _trailEndWidth;

        /// <summary>Gets the minimum distance between trail vertices.</summary>
        public float TrailMinVertexDistance => _trailMinVertexDistance;

        /// <summary>Gets the colour-over-lifetime gradient of the trail.</summary>
        public Gradient TrailColor => _trailColor;

        /// <summary>Gets the slash arc particle prefab for normal swings.</summary>
        public GameObject SlashEffectPrefab => _slashEffectPrefab;

        /// <summary>Gets the special effect prefab for special or ultimate attacks.</summary>
        public GameObject SpecialEffectPrefab => _specialEffectPrefab;

        /// <summary>Gets the hit impact prefab spawned at the point of contact.</summary>
        public GameObject HitImpactPrefab => _hitImpactPrefab;

        /// <summary>Gets the slash effect lifetime in seconds.</summary>
        public float SlashEffectLifetime => _slashEffectLifetime;

        /// <summary>Gets the special effect lifetime in seconds.</summary>
        public float SpecialEffectLifetime => _specialEffectLifetime;

        /// <summary>Gets the hit impact lifetime in seconds.</summary>
        public float HitImpactLifetime => _hitImpactLifetime;
    }
}