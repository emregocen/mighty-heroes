using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// Positions a camera by interpolating between two or more scene Transforms.
    ///
    /// Unlike offset-based rigs, every keyframe here is a real Transform placed
    /// directly in the scene — designers position the camera visually in the
    /// Scene view and see the exact result, no vector math involved.
    /// </summary>
    public sealed class CameraTransformBlender : MonoBehaviour
    {
        // PUBLIC METHODS

        /// <summary>
        /// Sets the camera's position and rotation to exactly match the given
        /// keyframe transform. No interpolation — used to snap at sequence start.
        /// </summary>
        public void SnapTo(Transform keyframe)
        {
            if (keyframe == null)
                return;

            transform.SetPositionAndRotation(keyframe.position, keyframe.rotation);
        }

        /// <summary>
        /// Sets the camera's position and rotation to the linear/spherical
        /// interpolation between two keyframes at t (0 to 1).
        /// </summary>
        public void SetBlend(Transform from, Transform to, float t)
        {
            if (from == null || to == null)
                return;

            Vector3 position = Vector3.Lerp(from.position, to.position, t);
            Quaternion rotation = Quaternion.Slerp(from.rotation, to.rotation, t);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}