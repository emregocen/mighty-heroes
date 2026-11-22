using MightyHeroes.Gameplay.Battle.TurnFlow.Types;

namespace MightyHeroes.Gameplay.Battle.TurnFlow.States
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