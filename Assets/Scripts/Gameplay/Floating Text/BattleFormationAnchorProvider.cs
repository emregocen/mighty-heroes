using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Resolves floating-text world anchors using the hero reference and
    /// BattleFormation's enemy lookup. The height offset (Inspector value on
    /// BattleBootstrapper) is applied uniformly above the entity root so texts
    /// start around head height; the anchored view then projects it to screen.
    /// </summary>
    public sealed class BattleFormationAnchorProvider : IFloatingTextAnchorProvider
    {
        // FIELDS

        private readonly HeroComponent _hero;
        private readonly BattleFormation _formation;
        private readonly float _heightOffset;

        // CONSTRUCTOR

        /// <summary>
        /// Creates an anchor provider for the given hero and formation.
        /// </summary>
        /// <param name="hero">Battle hero.</param>
        /// <param name="formation">Enemy formation used to find enemies by id.</param>
        /// <param name="heightOffset">World-space height above the entity root.</param>
        public BattleFormationAnchorProvider(HeroComponent hero, BattleFormation formation, float heightOffset)
        {
            _hero = hero;
            _formation = formation;
            _heightOffset = heightOffset;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Returns the world anchor above the given entity, or Vector3.zero if
        /// the id matches neither the hero nor a formation enemy.
        /// </summary>
        public Vector3 GetAnchorPosition(int entityId)
        {
            if (_hero != null && entityId == _hero.EntityId)
                return _hero.Position + Vector3.up * _heightOffset;

            ICombatEntity enemy = _formation.FindEnemyById(entityId);

            if (enemy != null)
                return enemy.Position + Vector3.up * _heightOffset;

            return Vector3.zero;
        }
    }
}
