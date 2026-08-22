using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using UnityEngine;

namespace MightyHeroes.Gameplay.Animation.Behaviours
{
    /// <summary>
    /// Attach to the entry point of the taunt animation state.
    /// Notifies the registered IIntroSignalReceiver the moment the taunt state
    /// is entered. Uses IntroSignalReceiverRegistry rather than
    /// GetComponentInParent because the receiver typically lives on a separate
    /// scene GameObject, not in the hero's own parent hierarchy.
    /// </summary>
    public sealed class TauntStartedBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            IIntroSignalReceiver receiver = IntroSignalRegistry.Active;

            if (receiver == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    "TauntStartedBehaviour: no receiver registered in IntroSignalReceiverRegistry.");
                return;
            }

            receiver.NotifyTauntStarted();
        }
    }
}