using UnityEngine;

namespace MightyHeroes.Gameplay.Equipment.Data
{
    /// <summary>
    /// Stores visual setup data for a weapon that will be mounted on a character socket.
    /// This asset is responsible only for presentation data and does not contain gameplay stats.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WeaponVisualConfig",
        menuName = "MightyHeroes/Equipment/Weapon Visual Config")]
    public sealed class WeaponVisualConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _weaponId;
        [SerializeField] private string _displayName;

        [Header("Visual")]
        [SerializeField] private GameObject _weaponPrefab;

        [Header("Mount Offsets")]
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;

        /// <summary>
        /// Gets the unique weapon identifier.
        /// </summary>
        public string WeaponId => _weaponId;

        /// <summary>
        /// Gets the display name of the weapon.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Gets the weapon prefab that will be instantiated on the socket.
        /// </summary>
        public GameObject WeaponPrefab => _weaponPrefab;

        /// <summary>
        /// Gets the local position offset relative to the mount socket.
        /// </summary>
        public Vector3 LocalPosition => _localPosition;

        /// <summary>
        /// Gets the local rotation offset in Euler angles relative to the mount socket.
        /// </summary>
        public Vector3 LocalEulerAngles => _localEulerAngles;

        /// <summary>
        /// Gets the local scale applied to the spawned weapon instance.
        /// </summary>
        public Vector3 LocalScale => _localScale;
    }
}