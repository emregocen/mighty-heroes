using MightyHeroes.Core.StateMachine;
using MightyHeroes.Core.StateMachine.Signals;

namespace MightyHeroes.Gameplay.Hero.StateMachine.States
{
    /// <summary>
    /// Base class for all hero states.
    /// Provides access to the owning hero state machine.
    /// </summary>
    public abstract class HeroBaseState : IState
    {
        protected readonly HeroStateMachine StateMachine;

        protected HeroBaseState(HeroStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Indicates whether this state is terminal. Terminal states cannot transition to any other state.
        /// </summary>
        public virtual bool IsTerminal => false;

        /// <summary>
        /// Called when the state becomes active.      
        /// </summary>
        public virtual void Enter()
        {
        }

        /// <summary>
        /// Called every tick while the state is active.
        /// </summary>
        public virtual void Tick()
        {
        }

        /// <summary>
        /// Called before transitioning away from the state.
        /// Override to perform any necessary cleanup.
        /// Default implementation does nothing.
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary>
        /// Handles incoming signals sent by other states or external events.
        /// Default implementation does nothing.
        /// Override in derived states to handle specific signals as needed.
        /// </summary>
        /// <param name="signal"></param>
        public virtual void Handle(IStateSignal signal)
        {
        }
    }
}