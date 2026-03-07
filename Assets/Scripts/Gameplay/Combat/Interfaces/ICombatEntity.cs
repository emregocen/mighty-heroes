namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Represents an entity that can participate in combat.
    /// Acts as the common base interface for attackers and targets.
    /// </summary>
    public interface ICombatEntity
    {
        /// <summary>
        /// Unique runtime identifier of the combat entity.
        /// </summary>
        int EntityId { get; }

        /// <summary>
        /// Indicates whether the entity is alive.
        /// </summary>
        bool IsAlive { get; }
    }
}