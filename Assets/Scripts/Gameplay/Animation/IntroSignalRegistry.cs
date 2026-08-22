namespace MightyHeroes.Gameplay.Animation.Interfaces
{
    /// <summary>
    /// Static registration point for the active IIntroSignalReceiver.
    ///
    /// StateMachineBehaviours (TauntStartedBehaviour, TauntEndedBehaviour) cannot
    /// reach the intro controller via GetComponentInParent because the controller
    /// typically lives on a separate scene GameObject from the hero's Animator.
    /// This registry avoids coupling the animation behaviours directly to
    /// BattleIntroController, keeping the dependency one-directional:
    /// Battle -> Gameplay.Animation, never the reverse.
    /// </summary>
    public static class IntroSignalRegistry
    {
        /// <summary>The currently active receiver for this battle, or null if none.</summary>
        public static IIntroSignalReceiver Active { get; private set; }

        /// <summary>Registers the active receiver. Call from the receiver's Awake.</summary>
        public static void Register(IIntroSignalReceiver receiver)
        {
            Active = receiver;
        }

        /// <summary>Clears the active receiver if it matches. Call from the receiver's OnDestroy.</summary>
        public static void Unregister(IIntroSignalReceiver receiver)
        {
            if (Active == receiver)
                Active = null;
        }
    }
}