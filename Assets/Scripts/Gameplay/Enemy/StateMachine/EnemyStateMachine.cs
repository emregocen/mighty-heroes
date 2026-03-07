using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Core.StateMachine;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.AI;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Enemy.StateMachine.Signals;
using MightyHeroes.Gameplay.Enemy.StateMachine.States;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Gameplay.Enemy.StateMachine
{
    /// <summary>
    /// Enemy finite state machine.
    ///
    /// Responsibilities:
    /// - Owns enemy state lifecycle and all state transitions.
    /// - Routes behaviour signals (animation events) into the active state.
    /// - Holds battle-lifetime EventHub subscriptions for events that must
    ///   be forwarded into the active state as signals regardless of which
    ///   state is currently active (HeroHitReactionEnded, CombatHitResolved).
    ///
    /// Design note:
    /// State-local subscriptions (turn start, action events) live inside each
    /// state's Enter/Exit. Battle-lifetime subscriptions are set up once in the
    /// constructor and torn down in Dispose.
    /// </summary>
    public sealed class EnemyStateMachine : BaseStateMachine
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly EnemyComponent _enemy;
        private readonly EnemyActionResolver _actionResolver;

        // PROPERTIES

        /// <summary>Battle-scoped event hub shared by all systems.</summary>
        public EventHub EventHub => _eventHub;

        /// <summary>Enemy entity component owned by this state machine.</summary>
        public EnemyComponent Enemy => _enemy;

        /// <summary>Resolves enemy AI decisions into action executions.</summary>
        public EnemyActionResolver ActionResolver => _actionResolver;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new enemy state machine and subscribes to battle-lifetime
        /// EventHub events required for cross-system signal routing.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub shared by all systems.</param>
        /// <param name="enemy">Enemy entity component this FSM controls.</param>
        /// <param name="actionResolver">Resolver that converts AI decisions into actions.</param>
        public EnemyStateMachine(
            EventHub eventHub,
            EnemyComponent enemy,
            EnemyActionResolver actionResolver)
        {
            _eventHub = eventHub;
            _enemy = enemy;
            _actionResolver = actionResolver;

            _eventHub.HeroEvents.HeroHitReactionEndedEvent.Register(OnHeroHitReactionEnded);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Register(OnCombatHitResolved);

            GameLogger.Log(LogSourceType.Enemy,
                $"Enemy FSM created. EventHub Hash={_eventHub.GetHashCode()}");
        }

        // PUBLIC METHODS

        /// <summary>
        /// Starts the FSM in idle state.
        /// Must be called exactly once per battle instance after construction.
        /// </summary>
        public void Start()
        {
            Start(new EnemyIdleState(this));
        }

        /// <summary>
        /// Unsubscribes all battle-lifetime EventHub listeners and stops the FSM.
        /// Must be called when the battle ends to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _eventHub.HeroEvents.HeroHitReactionEndedEvent.Unregister(OnHeroHitReactionEnded);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Unregister(OnCombatHitResolved);
            Stop();
        }

        // STATE SWITCHES

        /// <summary>Transitions enemy to idle state (waiting for turn or incoming hit).</summary>
        public void SwitchToIdleState()
        {
            SwitchState(new EnemyIdleState(this));
        }

        /// <summary>Transitions enemy to ready state (preparing to execute an action).</summary>
        public void SwitchToReadyState()
        {
            SwitchState(new EnemyReadyState(this));
        }

        /// <summary>Transitions enemy to attack state with the given variant and intensity.</summary>
        /// <param name="attackVariant">Animation variant index for the attack.</param>
        /// <param name="intensity">Attack intensity, also defines combo step count.</param>
        public void SwitchToAttackState(int attackVariant, int intensity)
        {
            SwitchState(new EnemyAttackState(this, attackVariant, intensity));
        }

        /// <summary>
        /// Transitions enemy to taking-hit state.
        /// Damage is already applied in EnemyComponent.ReceiveDamage before this is called.
        /// This state is responsible only for the animation reaction.
        /// </summary>
        /// <param name="hitDirection">Direction of the incoming hit for animation.</param>
        public void SwitchToTakingHitState(HitDirection hitDirection)
        {
            SwitchState(new EnemyTakingHitState(this, hitDirection));
        }

        /// <summary>Transitions enemy to defeated state when health reaches zero.</summary>
        public void SwitchToDefeatedState()
        {
            SwitchState(new EnemyDefeatedState(this));
        }

        /// <summary>
        /// Raises EnemyTurnEnded event so TurnSystem can advance to the next entity.
        /// </summary>
        public void FinishEnemyTurn()
        {
            _eventHub.EnemyEvents.EnemyTurnEndedEvent.Raise(
                new EnemyTurnEndedPayload(_enemy.EntityId));
        }

        // BEHAVIOUR -> SIGNAL FORWARDING

        /// <summary>
        /// Called by EnemyAnimationEventProxy at the authored hit frame.
        /// Forwards to the active state as EnemyAttackHitFrameSignal.
        /// </summary>
        /// <param name="direction">Hit direction encoded in the animation event.</param>
        public void NotifyAttackHitFrame(HitDirection direction)
        {
            Notify(new EnemyAttackHitFrameSignal(direction));
        }

        /// <summary>
        /// Called by EnemyAttackSequenceEndedBehaviour when the full attack
        /// sequence animation completes.
        /// </summary>
        public void NotifyAttackSequenceEnded()
        {
            Notify(new EnemyAttackSequenceEndedSignal());
        }

        /// <summary>
        /// Called by behaviour when a single attack clip within a combo ends.
        /// </summary>
        public void NotifyAttackClipEnded()
        {
            Notify(new EnemyAttackClipEndedSignal());
        }

        /// <summary>
        /// Called by EnemyHitReactionEndedBehaviour when this enemy's own hit
        /// reaction animation completes. Forwards as local signal to EnemyTakingHitState.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            Notify(new EnemyHitReactionEndedSignal());
        }

        // EVENT HANDLERS

        /// <summary>
        /// Receives HeroHitReactionEnded from EventHub.
        /// Converts to local signal so EnemyAttackState can advance combo
        /// after the hero finishes reacting to a hit.
        /// </summary>
        private void OnHeroHitReactionEnded(HeroHitReactionEndedPayload payload)
        {
            if (payload.HeroEntityId == _enemy.EntityId)
                return;

            Notify(new EnemyTargetHitReactionEndedSignal(payload));
        }

        /// <summary>
        /// Receives CombatHitResolved from EventHub.
        /// Converts to local signal so EnemyAttackState can detect
        /// mid-combo target death and trigger a forced sequence finish.
        /// </summary>
        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            Notify(new EnemyCombatHitResolvedSignal(payload));
        }
    }
}