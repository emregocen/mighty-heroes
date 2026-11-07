using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Abstraction over hit VFX playback so CombatFeedbackPresenter stays plain
    /// C# and does not instantiate or pool GameObjects itself.
    /// </summary>
    public interface IHitVfxPlayer
    {
        /// <summary>
        /// Plays one VFX prefab at the given position and returns it to the
        /// pool after the given lifetime.
        /// </summary>
        /// <param name="prefab">Particle effect prefab.</param>
        /// <param name="position">World position of the effect (usually the target's center).</param>
        /// <param name="towardCameraOffset">
        /// World distance the effect is pulled from position toward the camera, so it
        /// renders on the visible surface of the target instead of inside its body.
        /// </param>
        /// <param name="scale">Uniform scale applied to the effect.</param>
        /// <param name="lifetime">Seconds before the instance is stopped and pooled.</param>
        void Play(GameObject prefab, Vector3 position, float towardCameraOffset, float scale, float lifetime);
    }
}
