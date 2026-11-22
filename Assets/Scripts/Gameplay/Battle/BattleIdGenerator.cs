namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Generates unique entity identifiers within a single battle scope.
    /// 
    /// This generator guarantees uniqueness only during the lifetime
    /// of a battle session. It is deterministic and independent
    /// from Unity instance identifiers.
    /// </summary>
    public sealed class BattleIdGenerator
    {
        // FIELDS

        private int _nextId = 1;

        // PUBLIC METHODS

        /// <summary>
        /// Generates the next unique entity identifier.
        /// </summary>
        public int Generate()
        {
            int id = _nextId;
            _nextId++;
            return id;
        }

    }
}