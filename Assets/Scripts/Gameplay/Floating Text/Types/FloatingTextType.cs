namespace MightyHeroes.Gameplay.FloatingText.Types
{
    /// <summary>
    /// Identifies the visual/behavioural category of a floating text instance.
    /// Used to select the correct FloatingTextTypeSettings entry from
    /// FloatingTextConfig and to route world-space vs screen-space placement.
    /// </summary>
    public enum FloatingTextType
    {
        Damage = 0,
        Heal = 1,
        KnockOut = 2,
        Overkill = 3,
        Combo = 4,
    }
}