using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Attach to the exit point of the taunt animation state.
    /// Notifies the registered IIntroSignalReceiver the moment the taunt
    /// state is exited.
    /// </summary>
    public sealed class TauntEndedBehaviour : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            IIntroSignalReceiver receiver = IntroSignalRegistry.Active;

            if (receiver == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "TauntEndedBehaviour: no receiver registered in IntroSignalReceiverRegistry.");
                return;
            }

            receiver.NotifyTauntEnded();
        }
    }
}