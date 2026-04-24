using MightyHeroes.Gameplay.Equipment.Data;
using UnityEngine;

namespace MightyHeroes.Gameplay.Equipment.Views
{
    /// <summary>
    /// Responsible for handling weapon visual representation on a character.
    /// This class spawns and attaches weapon prefabs to the correct socket.
    /// It does not contain any gameplay or combat logic.
    /// </summary>
    public sealed class CharacterWeaponView : MonoBehaviour
    {
        // 1) INSPECTOR REFERENCES

        [Header("Mount Point")]
        [SerializeField] private Transform _rightHandSocket;

        [Header("Default Setup")]
        [SerializeField] private WeaponVisualConfig _defaultWeapon;

        // 2) RUNTIME STATE

        private GameObject _currentWeaponInstance;
        private WeaponVisualConfig _currentWeaponConfig;

        // 3) PROPERTIES

        /// <summary>
        /// Gets the currently equipped weapon visual config.
        /// </summary>
        public WeaponVisualConfig CurrentWeaponConfig => _currentWeaponConfig;

        // 4) UNITY LIFECYCLE

        private void Start()
        {
            if (_defaultWeapon != null)
            {
                Equip(_defaultWeapon);
            }
        }

        // 5) PUBLIC API

        /// <summary>
        /// Equips the provided weapon visual config.
        /// Destroys previous weapon instance and spawns a new one.
        /// </summary>
        /// <param name="config">Weapon visual configuration.</param>
        public void Equip(WeaponVisualConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning(
                    $"{nameof(CharacterWeaponView)}: Equip failed. Config is null.",
                    this);
                return;
            }

            if (_rightHandSocket == null)
            {
                Debug.LogError(
                    $"{nameof(CharacterWeaponView)}: Right hand socket is not assigned.",
                    this);
                return;
            }

            if (config.WeaponPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(CharacterWeaponView)}: Weapon prefab is missing in config '{config.name}'.",
                    this);
                return;
            }

            // Prevent redundant re-equip
            if (_currentWeaponConfig == config)
                return;

            ClearCurrentWeapon();

            _currentWeaponInstance =
                Instantiate(config.WeaponPrefab, _rightHandSocket);

            ApplyLocalTransform(_currentWeaponInstance.transform, config);

            _currentWeaponConfig = config;
        }

        /// <summary>
        /// Removes currently equipped weapon visual.
        /// </summary>
        public void Unequip()
        {
            ClearCurrentWeapon();
            _currentWeaponConfig = null;
        }

        // 6) INTERNAL METHODS

        /// <summary>
        /// Applies local transform offsets from config to weapon instance.
        /// </summary>
        private static void ApplyLocalTransform(
            Transform weaponTransform,
            WeaponVisualConfig config)
        {
            weaponTransform.localPosition = config.LocalPosition;
            weaponTransform.localRotation =
                Quaternion.Euler(config.LocalEulerAngles);
            weaponTransform.localScale = config.LocalScale;
        }

        /// <summary>
        /// Destroys the currently active weapon instance.
        /// </summary>
        private void ClearCurrentWeapon()
        {
            if (_currentWeaponInstance == null)
                return;

            Destroy(_currentWeaponInstance);
            _currentWeaponInstance = null;
        }
    }
}