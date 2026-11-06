using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Combat.Modifiers
{
    /// <summary>
    /// Plain C# runtime model for temporary combat modifiers of a single entity.
    ///
    /// Currently holds one modifier: a pending attack buff expressed as a
    /// percent bonus that applies to every hit of the entity's next attack
    /// sequence and is then consumed by the attacker's state machine.
    ///
    /// Stacking rule: buffs do not add up. Granting a new buff keeps the
    /// higher of the current and the new percent.
    ///
    /// Owned by EntityComponent next to CombatHealth, so Hero, Enemy and
    /// Companion share the same model. Raises AttackBuffChangedEvent so
    /// presentation (status icons, floating text) can react without coupling.
    /// </summary>
    public sealed class CombatModifiers
    {
        // CONSTANTS

        private const int PercentBase = 100;

        // FIELDS

        private readonly EventHub _eventHub;
        private readonly int _entityId;
        private int _attackBuffPercent;

        // PROPERTIES

        /// <summary>Owning entity runtime id.</summary>
        public int EntityId => _entityId;

        /// <summary>Pending attack bonus percent (25 = +25%). Zero when no buff is active.</summary>
        public int AttackBuffPercent => _attackBuffPercent;

        /// <summary>True while an attack buff is waiting to be consumed.</summary>
        public bool HasAttackBuff => _attackBuffPercent > 0;

        // CONSTRUCTOR

        /// <summary>
        /// Creates an empty modifier model for the given entity.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub for raising modifier events.</param>
        /// <param name="entityId">Owner entity runtime id.</param>
        public CombatModifiers(EventHub eventHub, int entityId)
        {
            _eventHub = eventHub;
            _entityId = entityId;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Grants an attack buff. Keeps the higher of the current and the new percent.
        /// Raises AttackBuffChangedEvent only when the value changes.
        /// </summary>
        /// <param name="percent">Attack bonus percent to grant.</param>
        /// <param name="sourceEntityId">Entity granting the buff. 0 if unknown.</param>
        public void GrantAttackBuff(int percent, int sourceEntityId)
        {
            if (percent <= _attackBuffPercent)
                return;

            SetAttackBuff(percent, sourceEntityId);
        }

        /// <summary>
        /// Clears the pending attack buff after an attack sequence has used it.
        /// Raises AttackBuffChangedEvent only when a buff was active.
        /// </summary>
        public void ConsumeAttackBuff()
        {
            if (_attackBuffPercent == 0)
                return;

            SetAttackBuff(0, _entityId);
        }

        /// <summary>
        /// Applies the pending attack buff to a resolved damage value.
        /// Does not consume the buff; consumption happens once per attack sequence.
        /// </summary>
        /// <param name="damage">Damage resolved by the combat resolver.</param>
        /// <returns>Damage including the attack buff, rounded to nearest integer.</returns>
        public int ApplyAttackBuff(int damage)
        {
            if (_attackBuffPercent <= 0 || damage <= 0)
                return damage;

            return (damage * (PercentBase + _attackBuffPercent) + PercentBase / 2) / PercentBase;
        }

        // PRIVATE METHODS

        private void SetAttackBuff(int percent, int sourceEntityId)
        {
            int previous = _attackBuffPercent;
            _attackBuffPercent = percent;

            _eventHub.CombatEvents.AttackBuffChangedEvent.Raise(
                new AttackBuffChangedPayload(_entityId, previous, _attackBuffPercent, sourceEntityId));
        }
    }
}
