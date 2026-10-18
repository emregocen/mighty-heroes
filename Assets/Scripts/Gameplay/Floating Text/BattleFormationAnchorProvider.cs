using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Hero;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Resolves floating-text anchor points using the hero reference and
    /// BattleFormation's enemy lookup. Chest-height offset is applied uniformly;
    /// adjust _heightOffset if hero/enemy proportions differ significantly.
    /// </summary>
    public sealed class BattleFormationAnchorProvider : IFloatingTextAnchorProvider
    {
        // FIELDS

        private readonly HeroComponent _hero;
        private readonly BattleFormation _formation;
        private readonly float _heightOffset;

        // CONSTRUCTOR

        public BattleFormationAnchorProvider(HeroComponent hero, BattleFormation formation, float heightOffset = 1.6f)
        {
            _hero = hero;
            _formation = formation;
            _heightOffset = heightOffset;
        }

        // PUBLIC METHODS
public Vector3 GetAnchorPosition(int entityId)
{
    if (_hero != null && entityId == _hero.EntityId)
    {
        Debug.Log($"[AnchorProvider] entityId={entityId} matched HERO '{_hero.name}' at {_hero.Position}");
        return _hero.Position + Vector3.up * _heightOffset;
    }

    var enemy = _formation.FindEnemyById(entityId);

    if (enemy != null)
    {
        Debug.Log($"[AnchorProvider] entityId={entityId} matched ENEMY '{(enemy as UnityEngine.Component)?.gameObject.name}' at {enemy.Position}");
        return enemy.Position + Vector3.up * _heightOffset;
    }

    Debug.Log($"[AnchorProvider] entityId={entityId} matched NOTHING, returning zero");
    return Vector3.zero;
}
    }
}