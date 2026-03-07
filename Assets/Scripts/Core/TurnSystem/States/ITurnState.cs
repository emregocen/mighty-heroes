using MightyHeroes.Core.TurnSystem.Types;

namespace MightyHeroes.Core.TurnSystem.States
{
    /// <summary>
    /// Defines the contract for all turn states.
    /// </summary>
    public interface ITurnState
    {
        TurnStateType StateType { get; }

        void Enter();
        void Tick();
        void Exit();
    }
}