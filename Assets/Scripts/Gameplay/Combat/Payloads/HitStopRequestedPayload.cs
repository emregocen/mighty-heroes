namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Raised by CombatFeedbackPresenter when a hit should briefly freeze the
    /// attacker's and the target's animations (hit-stop). Each entity component
    /// pauses its own Animator when its id matches.
    /// </summary>
    public readonly struct HitStopRequestedPayload
    {
        // PROPERTIES

        /// <summary>Entity that landed the hit.</summary>
        public int AttackerId { get; }

        /// <summary>Entity that received the hit.</summary>
        public int TargetId { get; }

        /// <summary>Freeze duration in seconds.</summary>
        public float Duration { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates a hit-stop request payload.
        /// </summary>
        public HitStopRequestedPayload(int attackerId, int targetId, float duration)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Duration = duration;
        }

        // PUBLIC METHODS

        /// <summary>True when the given entity takes part in this hit-stop.</summary>
        public bool Involves(int entityId)
        {
            return entityId == AttackerId || entityId == TargetId;
        }
    }
}
