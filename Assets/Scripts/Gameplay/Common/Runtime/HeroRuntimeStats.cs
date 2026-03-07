using MightyHeroes.Gameplay.Hero.Stats;

namespace MightyHeroes.Gameplay.Hero.Data
{
    /// <summary>
    /// Holds all mutable runtime data for the hero.
    /// Used for save/load and gameplay decisions.
    /// </summary>
    public sealed class HeroRuntimeData
    {
        // 1) PROPERTIES

        public int CurrentHealth { get; private set; }
        public int CurrentActionPoints { get; private set; }
        public int ComboCounter { get; private set; }

        // 3) CONSTRUCTOR

        public HeroRuntimeData(HeroStatsData stats)
        {
            CurrentHealth = stats.MaxHealth;
            CurrentActionPoints = stats.BaseActionPoints;
            ComboCounter = 0;
        }

        // 4) PUBLIC METHODS

        public void ResetActionPoints(int baseActionPoints)
        {
            CurrentActionPoints = baseActionPoints;
        }

        public bool TryConsumeActionPoint()
        {
            if (CurrentActionPoints <= 0)
                return false;

            CurrentActionPoints--;
            return true;
        }
    }
}