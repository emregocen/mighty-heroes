using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.TurnSystem.Types;

namespace MightyHeroes.Core.TurnSystem.Payloads
{
    /// <summary>
    /// Payload sent when the active turn state changes.
    /// </summary>
    public sealed class TurnStateChangedPayload : IEventPayload
    {
        /// <summary>
        /// The previous turn state.
        /// </summary>
        public TurnStateType PreviousState { get; }

        /// <summary>
        /// The new active turn state.
        /// </summary>
        public TurnStateType CurrentState { get; }

        public TurnStateChangedPayload(TurnStateType previousState, TurnStateType currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
}