namespace MightyHeroes.Gameplay.Ultimate
{
    /// <summary>
    /// Which enemies an ultimate hits. Extended later per hero class and
    /// affinity color (single target, all lines, and so on).
    /// </summary>
    public enum UltimateTargetMode
    {
        /// <summary>Every alive enemy in the current front line.</summary>
        FrontLine = 0
    }
}
