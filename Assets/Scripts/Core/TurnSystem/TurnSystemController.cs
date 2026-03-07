using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.Core.TurnSystem
{
    /// <summary>
    /// Unity-facing controller that bridges the Unity lifecycle with the turn state machine.
    /// Responsible only for driving the turn state machine tick.
    /// </summary>
    public sealed class TurnSystemController : MonoBehaviour
    {
        // Fields

        /// <summary>
        /// Active battle turn state machine instance.
        /// Owned by the battle scope and injected by the battle bootstrapper.
        /// </summary>
        private TurnStateMachine _turnStateMachine;

        // Unity callbacks

        private void Update()
        {
            _turnStateMachine?.Tick();
        }

        private void OnDestroy()
        {
            GameLogger.Log(
                LogSourceType.TurnSystem,
                $"TurnSystemController.OnDestroy InstanceId={GetInstanceID()}"
            );
        }

        // Public methods

        /// <summary>
        /// Initializes the controller with the given turn state machine.
        /// Must be called by the battle bootstrapper.
        /// </summary>
        /// <param name="stateMachine">Battle-scoped turn state machine.</param>
        public void Initialize(TurnStateMachine stateMachine)
        {
            _turnStateMachine = stateMachine;

            GameLogger.Log(
                LogSourceType.TurnSystem,
                $"TurnSystemController.Initialize InstanceId={GetInstanceID()}"
            );
        }
    }
}