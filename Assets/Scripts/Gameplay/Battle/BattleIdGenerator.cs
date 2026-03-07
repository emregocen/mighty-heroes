
namespace MightyHeroes.Core.BattleSystem
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
        private int _nextId = 1;

        /// <summary>
        /// Generates the next unique entity identifier.
        /// </summary>
        public int Generate()
        {
            int id = _nextId;
            _nextId++;
            return id;
        }

        /// <summary>
        /// Resets the generator back to initial state.
        /// Intended to be used only when a new battle starts.
        /// </summary>
        public void Reset()
        {
            _nextId = 1;
        }
    }
}