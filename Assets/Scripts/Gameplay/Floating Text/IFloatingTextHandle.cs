using MightyHeroes.Gameplay.FloatingText;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Base contract for anything the pool can spawn and later recycle.
    /// Implemented by both world-space and screen-space floating text views.
    /// </summary>
    public interface IFloatingTextHandle
    {
        /// <summary>True while this instance is actively showing (not available in the pool).</summary>
        bool IsActive { get; }

        /// <summary>
        /// Plays the reveal/rise/fade animation for the given text and settings.
        /// Calls onReturnToPool when finished so the pool can reclaim this instance.
        /// </summary>
        void Play(string text, FloatingTextConfig.TypeSettings settings, System.Action<IFloatingTextHandle> onReturnToPool);

        /// <summary>Immediately stops any animation and hides the instance without waiting for natural completion.</summary>
        void ForceReturn();
    }
}