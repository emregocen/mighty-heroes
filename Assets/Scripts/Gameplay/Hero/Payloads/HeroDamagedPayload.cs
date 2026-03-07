using MightyHeroes.Gameplay.Combat.Types;

namespace MightyHeroes.Gameplay.Hero.Payloads
{
    /// <summary>
    /// Payload raised when the hero receives confirmed and resolved damage.
    ///
    /// This event represents a finalized combat result and is published
    /// only after all combat resolution steps have completed.
    /// </summary>
    public sealed class HeroDamagedPayload
    {
        // 1) PROPERTIES

        /// <summary>
        /// Final resolved damage value applied to the hero.
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// Direction from which the hit was received.
        /// Used to select the appropriate hit reaction animation.
        /// </summary>
        public HitDirection HitDirection { get; }

        // 2) CONSTRUCTOR

        /// <summary>
        /// Creates a new hero damaged payload.
        /// </summary>
        /// <param name="damage">Resolved damage amount.</param>
        /// <param name="hitDirection">Direction of the incoming hit.</param>
        public HeroDamagedPayload(int damage, HitDirection hitDirection)
        {
            Damage = damage;
            HitDirection = hitDirection;
        }
    }
}