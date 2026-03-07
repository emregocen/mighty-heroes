namespace MightyHeroes.Gameplay.Combat.Types
{
    /// <summary>
    /// Represents the category of damage resolved by the combat system.
    /// 
    /// Physical and Magical damage may be absorbed by shields.
    /// True damage bypasses shields and is applied directly to health.
    /// </summary>
    public enum DamageType
    {
        Physical = 0,
        Magical = 1,
        True = 2
    }
}