using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// Positions a camera on a sphere around a live target (the hero), recomputed
    /// every LateUpdate from the target's current position. Because the orbit
    /// pivot follows the target directly, the camera keeps the target framed
    /// automatically even while the target is moving.
    ///
    /// Yaw is measured from ReferenceYaw, which the battle intro sets to the
    /// hero's battle facing: Yaw 0 puts the camera in front of the hero looking
    /// at him, Yaw 180 behind him looking toward the enemies. The same intro
    /// poses therefore work at every battle site, whatever direction the hero
    /// faces in the world.
    /// </summary>
    public sealed class OrbitCameraRig : MonoBehaviour
    {
        // FIELDS

        [SerializeField] private Transform _target;

        [Tooltip("World-space offset from target.position used as both the orbit " +
                 "sphere's center and the camera's look-at point (roughly chest height).")]
        [SerializeField] private Vector3 _pivotOffset = new Vector3(0f, 1.3f, 0f);

        // PROPERTIES

        /// <summary>Orbit yaw in degrees relative to ReferenceYaw (0 = in front of the hero).</summary>
        public float Yaw { get; set; }

        /// <summary>World yaw of the hero's front; orbit yaws are added to it.</summary>
        public float ReferenceYaw { get; set; }
        public float Pitch { get; set; }
        public float Distance { get; set; } = 3.5f;

        // UNITY CALLBACKS

        private void LateUpdate()
        {
            if (_target == null)
                return;

            Vector3 pivot = _target.position + _pivotOffset;
            Vector3 direction = Quaternion.Euler(Pitch, ReferenceYaw + Yaw, 0f) * Vector3.back;

            transform.position = pivot + direction * Distance;
            transform.rotation = Quaternion.LookRotation((pivot - transform.position).normalized, Vector3.up);
        }

        // PUBLIC METHODS

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetImmediate(float yaw, float pitch, float distance)
        {
            Yaw = yaw;
            Pitch = pitch;
            Distance = distance;
        }
    }
}