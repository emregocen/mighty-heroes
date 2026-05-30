using UnityEngine;

namespace MightyHeroes.Gameplay.Hero.Camera
{
    /// <summary>
    /// Over-the-shoulder battle camera that follows the hero with smooth interpolation.
    ///
    /// Sits on its own GameObject (the battle camera, not parented to the hero).
    /// Position is computed from a smoothed copy of the hero's rotation rather than
    /// the hero's instantaneous rotation, so the shoulder offset arcs gently when
    /// the hero changes target instead of snapping sideways. SmoothDamp keeps the
    /// motion frame-rate independent.
    /// </summary>
    public sealed class HeroCameraRig : MonoBehaviour
    {
        // FIELDS

        [Header("References")]
        [SerializeField] private Transform _heroRoot;

        [Header("Over-the-shoulder offsets (hero local space)")]
        [SerializeField] private float _backDistance = 5f;
        [SerializeField] private float _rightOffset = 2.2f;
        [SerializeField] private float _heightOffset = 2.4f;
        [SerializeField] private Vector3 _lookOffset = new Vector3(0f, 1.3f, 0f);

        [Header("Smoothing")]
        [Tooltip("Position SmoothDamp time in seconds. Higher = lazier follow.")]
        [SerializeField] private float _positionSmoothTime = 0.22f;

        [Tooltip("How fast the camera's reference rotation eases toward the hero's rotation. " +
                 "Lower = more lag when the hero turns to a new target, giving a gentler arc.")]
        [SerializeField] private float _orientationFollowSpeed = 4f;

        [Tooltip("Slerp speed for the camera's actual look-at rotation.")]
        [SerializeField] private float _lookAtSpeed = 8f;

        private Vector3 _positionVelocity;
        private Quaternion _smoothedHeroRotation;
        private bool _hasInitialState;

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (_heroRoot == null)
                return;

            if (!_hasInitialState)
            {
                CaptureInitialState();
                return;
            }

            UpdateSmoothedRotation();
            UpdatePosition();
            UpdateLookAt();
        }

        // PRIVATE METHODS

        /// <summary>
        /// Initializes the smoothed rotation and snaps the camera into place
        /// on the first frame so it does not interpolate from a zero state.
        /// </summary>
        private void CaptureInitialState()
        {
            _smoothedHeroRotation = _heroRoot.rotation;
            transform.position = ComputeDesiredPosition();
            transform.rotation = ComputeLookRotation();
            _hasInitialState = true;
        }

        /// <summary>
        /// Slerps the smoothed reference rotation toward the hero's actual rotation.
        /// Position math reads from this smoothed value, which is what produces the
        /// gentle arc when the hero turns to face a new target.
        /// </summary>
        private void UpdateSmoothedRotation()
        {
            _smoothedHeroRotation = Quaternion.Slerp(
                _smoothedHeroRotation,
                _heroRoot.rotation,
                _orientationFollowSpeed * Time.deltaTime);
        }

        /// <summary>
        /// SmoothDamps the camera toward the over-the-shoulder offset computed
        /// from the smoothed rotation.
        /// </summary>
        private void UpdatePosition()
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                ComputeDesiredPosition(),
                ref _positionVelocity,
                _positionSmoothTime);
        }

        /// <summary>
        /// Slerps the camera's rotation toward the hero's chest/head area.
        /// </summary>
        private void UpdateLookAt()
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                ComputeLookRotation(),
                _lookAtSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Builds the world position for the right-shoulder camera slot using the
        /// smoothed orientation. Reading from _smoothedHeroRotation rather than the
        /// hero's live transform is the key to a gentle arc on target change.
        /// </summary>
        private Vector3 ComputeDesiredPosition()
        {
            Vector3 forward = _smoothedHeroRotation * Vector3.forward;
            Vector3 right = _smoothedHeroRotation * Vector3.right;

            return _heroRoot.position
                   - forward * _backDistance
                   + right * _rightOffset
                   + Vector3.up * _heightOffset;
        }

        /// <summary>
        /// Builds the rotation that points the camera at the hero's chest/head area.
        /// </summary>
        private Quaternion ComputeLookRotation()
        {
            Vector3 lookPosition = _heroRoot.position + _lookOffset;
            Vector3 direction = lookPosition - transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
                return transform.rotation;

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}