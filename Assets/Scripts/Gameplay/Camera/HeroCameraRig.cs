using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Over-the-shoulder camera that stays locked relative to the hero's
    /// recorded facing direction at battle start. The shoulder offset is
    /// applied in local space so it always sits to the right and slightly
    /// behind the character regardless of world rotation.
    /// </summary>
    public class HeroCameraRig : MonoBehaviour
    {
        // FIELDS
        [SerializeField] private Transform _heroTransform;

        [Header("Over-the-shoulder offset (hero local space)")]
        [Tooltip("X: right shoulder offset. Y: height. Z: distance behind.")]
        [SerializeField] private Vector3 _localOffset = new Vector3(0.6f, 1.8f, -3.5f);

        [Tooltip("The point the camera looks at, relative to hero position.")]
        [SerializeField] private Vector3 _lookAtOffset = new Vector3(0f, 1.4f, 0f);

        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _rotationSpeed = 8f;

        private Quaternion _battleFacingRotation;
        private bool _isInitialized;

        // PROPERTIES

        /// <summary>Lock point established at battle start. Camera never rotates after this.</summary>
        public bool IsLocked => _isInitialized;

        // PUBLIC METHODS

        /// <summary>
        /// Call this once when the battle scene loads and the hero is
        /// already facing the enemies. Captures the facing direction.
        /// </summary>
        public void LockToBattleOrientation()
        {
            _battleFacingRotation = _heroTransform.rotation;
            _isInitialized = true;
        }

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (!_isInitialized) return;

            // Calculate desired position using the LOCKED rotation, not current hero rotation.
            // This means the camera never spins when hero turns to face different enemies.
            Vector3 worldOffset = _battleFacingRotation * _localOffset;
            Vector3 targetPosition = _heroTransform.position + worldOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                _followSpeed * Time.deltaTime);

            // Always look at chest/head area of hero
            Vector3 lookTarget = _heroTransform.position + _lookAtOffset;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }
    }
}