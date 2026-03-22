namespace MightyHeroes.Gameplay.Combat.Interfaces
{
    /// <summary>
    /// Represents an entity that can be targeted by combat attacks.
    ///
    /// Damage flow:
    /// CombatSystem raises DamageIntentEvent with the target EntityId.
    /// The target entity listens to this event, filters by EntityId,
    /// and applies damage through its own CombatHealth pipeline.
    ///
    /// This interface does not define ReceiveDamage because CombatSystem
    /// never calls entity methods directly — it only raises events.
    /// </summary>
    public interface ICombatTarget : ICombatEntity
    {
        /// <summary>
        /// Current health value of this target.
        /// Read by CombatSystem to determine alive status for target selection.
        /// </summary>
        int CurrentHealth { get; }
    }
}