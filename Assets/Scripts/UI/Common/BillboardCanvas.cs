using UnityEngine;

namespace MightyHeroes.UI.Common
{
    /// <summary>
    /// Keeps the canvas positioned on its parent while preserving
    /// the initial vertical offset and facing the main camera.
    /// </summary>
    public sealed class BillboardCanvas : MonoBehaviour
    {
        // FIELDS

        private Transform _cachedTransform;
        private Transform _followTarget;
        private Transform _cameraTransform;
        private float _initialYOffset;

        // UNITY CALLBACKS

        /// <summary>
        /// Caches required references and validates setup.
        /// </summary>
        private void Awake()
        {
            CacheReferences();
            Validate();
        }

        /// <summary>
        /// Updates canvas position and rotation after transform changes.
        /// </summary>
        private void LateUpdate()
        {
            Vector3 targetPosition = _followTarget.position;
            targetPosition.y += _initialYOffset;

            _cachedTransform.position = targetPosition;

            if (_cameraTransform != null)
            {
                _cachedTransform.rotation = _cameraTransform.rotation;
            }
        }

        // PRIVATE METHODS

        /// <summary>
        /// Caches transform references and initial vertical offset.
        /// </summary>
        private void CacheReferences()
        {
            _cachedTransform = transform;
            _followTarget = _cachedTransform.parent;

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_followTarget != null)
            {
                _initialYOffset =
                    _cachedTransform.position.y - _followTarget.position.y;
            }
        }

        /// <summary>
        /// Validates required references and disables component if invalid.
        /// </summary>
        private void Validate()
        {
            if (_followTarget == null)
            {
                Debug.LogError(
                    $"{nameof(BillboardCanvas)} requires a parent transform.",
                    this);

                enabled = false;
                return;
            }
        }
    }
}