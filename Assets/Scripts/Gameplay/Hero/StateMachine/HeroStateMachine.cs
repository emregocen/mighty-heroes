using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Hero.Actions;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Hero.StateMachine.Signals;
using MightyHeroes.Gameplay.Hero.StateMachine.States;

namespace MightyHeroes.Gameplay.Hero.StateMachine
{
    /// <summary>
    /// Hero finite state machine.
    ///
    /// Responsibilities:
    /// - Owns hero state lifecycle and all state transitions.
    /// - Routes behaviour signals (animation events) into the active state.
    /// - Holds battle-lifetime EventHub subscriptions for events that must
    ///   be forwarded into the active state as signals regardless of which
    ///   state is currently active (EnemyHitReactionEnded, CombatHitResolved).
    ///
    /// Design note:
    /// State-local subscriptions (puzzle match, turn start) live inside each
    /// state's Enter/Exit. Battle-lifetime subscriptions are set up once in the
    /// constructor and torn down in Dispose.
    /// </summary>
    public sealed class HeroStateMachine : BaseStateMachine
    {
        // ------------------------------------------------------------------
        // FIELDS
        // ------------------------------------------------------------------

        private readonly EventHub _eventHub;
        private readonly HeroComponent _hero;
        private readonly HeroActionResolver _actionResolver;

        // ------------------------------------------------------------------
        // PROPERTIES
        // ------------------------------------------------------------------

        /// <summary>Battle-scoped event hub shared by all systems.</summary>
        public EventHub EventHub => _eventHub;

        /// <summary>Hero entity component owned by this state machine.</summary>
        public HeroComponent Hero => _hero;

        /// <summary>Resolves puzzle results into hero action executions.</summary>
        public HeroActionResolver ActionResolver => _actionResolver;

        // ------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a new hero state machine and subscribes to battle-lifetime
        /// EventHub events required for cross-system signal routing.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub shared by all systems.</param>
        /// <param name="hero">Hero entity component this FSM controls.</param>
        /// <param name="actionResolver">Resolver that converts puzzle results into hero actions.</param>
        public HeroStateMachine(
            EventHub eventHub,
            HeroComponent hero,
            HeroActionResolver actionResolver)
        {
            _eventHub = eventHub;
            _hero = hero;
            _actionResolver = actionResolver;

            _eventHub.EnemyEvents.EnemyHitReactionEndedEvent.Register(OnEnemyHitReactionEnded);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Register(OnCombatHitResolved);

            GameLogger.Log(LogSourceType.Hero, $"Hero FSM EventHub Hash = {_eventHub.GetHashCode()}");
        }

        // ------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------

        /// <summary>
        /// Starts the FSM in idle state.
        /// Must be called exactly once per battle instance after construction.
        /// </summary>
        public void Start()
        {
            Start(new HeroIdleState(this));
        }

        /// <summary>
        /// Unsubscribes all battle-lifetime EventHub listeners and stops the FSM.
        /// Must be called when the battle ends to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _eventHub.EnemyEvents.EnemyHitReactionEndedEvent.Unregister(OnEnemyHitReactionEnded);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Unregister(OnCombatHitResolved);
            Stop();
        }

        // ------------------------------------------------------------------
        // STATE SWITCHES
        // ------------------------------------------------------------------

        /// <summary>Transitions hero to idle state (waiting for turn start).</summary>
        public void SwitchToIdleState()
        {
            SwitchState(new HeroIdleState(this));
        }

        /// <summary>Transitions hero to ready state (puzzle input enabled).</summary>
        public void SwitchToReadyState()
        {
            SwitchState(new HeroReadyState(this));
        }

        /// <summary>Transitions hero to attack state with the resolved action data.</summary>
        /// <param name="execution">Action execution data resolved from puzzle match.</param>
        public void SwitchToAttackState(HeroActionExecution execution)
        {
            SwitchState(new HeroAttackState(
                this,
                execution.AttackVariant,
                execution.ActionIntensity));
        }

        /// <summary>Transitions hero to taking-hit state to process incoming damage.</summary>
        /// <param name="damage">Damage amount already applied via Health system.</param>
        /// <param name="direction">Direction of the incoming hit for animation purposes.</param>
        public void SwitchToTakingHitState(HitDirection direction)
        {
            SwitchState(new HeroTakingHitState(this, direction));
        }

        /// <summary>Transitions hero to defeated state when health reaches zero.</summary>
        public void SwitchToDefeatedState()
        {
            SwitchState(new HeroDefeatedState(this));
        }

        /// <summary>
        /// Raises HeroTurnEnded event so TurnSystem can advance to the next phase.
        /// </summary>
        public void FinishHeroTurn()
        {
            _eventHub.HeroEvents.HeroTurnEndedEvent.Raise(new HeroTurnEndedPayload());
        }

        // ------------------------------------------------------------------
        // BEHAVIOUR -> SIGNAL FORWARDING
        // ------------------------------------------------------------------

        /// <summary>
        /// Called by HeroAnimationEventProxy at the authored hit frame.
        /// Forwards to the active state as HeroAttackHitFrameSignal.
        /// </summary>
        /// <param name="direction">Hit direction encoded in the animation event.</param>
        public void NotifyAttackHitFrame(HitDirection direction)
        {
            Notify(new HeroAttackHitFrameSignal(direction));
        }

        /// <summary>
        /// Called by HeroAttackSequenceEndedBehaviour when the full attack
        /// sequence animation completes. Forwards as HeroAttackSequenceEndedSignal.
        /// </summary>
        public void NotifyAttackSequenceEnded()
        {
            Notify(new HeroAttackSequenceEndedSignal());
        }

        /// <summary>
        /// Called by behaviour when a single attack clip within a combo ends.
        /// Forwards as HeroAttackClipEndedSignal.
        /// </summary>
        public void NotifyAttackClipEnded()
        {
            Notify(new HeroAttackClipEndedSignal());
        }

        /// <summary>
        /// Called by HeroHitReactionEndedBehaviour when the hero's own hit
        /// reaction animation completes.
        /// Forwards as a local signal so HeroTakingHitState can exit.
        /// EventHub event is raised by HeroComponent, not here.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            Notify(new HeroHitReactionEndedSignal());
        }

        // ------------------------------------------------------------------
        // EVENT HANDLERS
        // ------------------------------------------------------------------

        /// <summary>
        /// Receives EnemyHitReactionEnded from EventHub.
        /// Converts it to a local signal so HeroAttackState can advance
        /// to the next combo step after the enemy finishes reacting.
        /// </summary>
        private void OnEnemyHitReactionEnded(EnemyHitReactionEndedPayload payload)
        {
            Notify(new HeroTargetHitReactionEndedSignal(payload));
        }

        /// <summary>
        /// Receives CombatHitResolved from EventHub.
        /// Converts it to a local signal so HeroAttackState can detect
        /// mid-combo target death and trigger a forced sequence finish.
        /// </summary>
        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            Notify(new HeroCombatHitResolvedSignal(payload));
        }
    }
}