namespace MightyHeroes.Gameplay.Combat.Payloads
{
    /// <summary>
    /// Raised by CombatFeedbackPresenter when a hero hit should trigger the
    /// cinematic kill moment (side kill-cam plus slow motion): the final hit of
    /// a high-tier attack that kills its target.
    /// </summary>
    public readonly struct KillCamRequestedPayload
    {
        // PROPERTIES

        /// <summary>Hero that landed the killing hit.</summary>
        public int AttackerId { get; }

        /// <summary>Enemy that was killed.</summary>
        public int TargetId { get; }

        // CONSTRUCTOR

        /// <summary>
        /// Creates a kill-cam request payload.
        /// </summary>
        public KillCamRequestedPayload(int attackerId, int targetId)
        {
            AttackerId = attackerId;
            TargetId = targetId;
        }
    }
}
