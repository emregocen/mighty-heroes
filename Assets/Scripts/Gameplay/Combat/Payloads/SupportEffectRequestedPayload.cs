using MightyHeroes.Gameplay.Common.Actions;

namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Raised when a support action's visual effect (heal, shield, buff) should
    /// play on an entity.
    ///
    /// This is a presentation request, independent of whether the action
    /// changed any value, so the effect still plays when, for example, the
    /// target is already at full health. The receiving entity is identified by
    /// TargetEntityId, which allows companions to support the hero in the future.
    /// </summary>
    public sealed class SupportEffectRequestedPayload
    {
        // PROPERTIES

        /// <summary>Runtime id of the entity that should show the effect.</summary>
        public int TargetEntityId { get; }

        /// <summary>Runtime id of the entity performing the action. Can be 0 if unknown.</summary>
        public int SourceEntityId { get; }

        /// <summary>Support action whose effect should play (Heal, Shield or Buff).</summary>
        public ActionType ActionType { get; }

        // CONSTRUCTOR

        /// <summary>Creates a support effect request.</summary>
        /// <param name="targetEntityId">Entity that shows the effect.</param>
        /// <param name="sourceEntityId">Entity performing the action.</param>
        /// <param name="actionType">Support action whose effect should play.</param>
        public SupportEffectRequestedPayload(int targetEntityId, int sourceEntityId, ActionType actionType)
        {
            TargetEntityId = targetEntityId;
            SourceEntityId = sourceEntityId;
            ActionType = actionType;
        }
    }
}
