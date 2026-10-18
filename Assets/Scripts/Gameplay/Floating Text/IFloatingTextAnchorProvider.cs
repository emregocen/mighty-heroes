using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Resolves the world-space anchor point (typically chest/head height)
    /// for a given entity, so FloatingTextPresenter can spawn text above the
    /// correct target without depending on HeroComponent/EnemyComponent directly.
    /// </summary>
    public interface IFloatingTextAnchorProvider
    {
        /// <summary>Returns the world position to spawn floating text above, for the given entity id.</summary>
        Vector3 GetAnchorPosition(int entityId);
    }
}