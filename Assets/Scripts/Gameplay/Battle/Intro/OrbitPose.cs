using System;

namespace MightyHeroes.Gameplay.Battle.Intro
{
    /// <summary>
    /// A single orbit camera keyframe expressed in spherical coordinates
    /// relative to OrbitCameraRig's pivot.
    ///
    /// Yaw: horizontal angle in degrees. 0 = directly behind the hero's forward.
    /// Positive rotates toward the hero's right, negative toward the left.
    ///
    /// Pitch: vertical angle in degrees. 0 = eye level with the pivot.
    /// Negative = camera below, looking up. Positive = camera above, looking down.
    ///
    /// Distance: world units from the pivot.
    /// </summary>
    [Serializable]
    public struct OrbitPose
    {
        public float Yaw;
        public float Pitch;
        public float Distance;

        public OrbitPose(float yaw, float pitch, float distance)
        {
            Yaw = yaw;
            Pitch = pitch;
            Distance = distance;
        }
    }
}