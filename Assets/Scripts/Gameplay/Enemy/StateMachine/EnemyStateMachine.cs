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
        private readonly int _heroEntityId;
        private bool _isBattleEnded;

        // PROPERTIES

        /// <summary>Battle-scoped event hub shared by all systems.</summary>
        public EventHub EventHub => _eventHub;

        /// <summary>Enemy entity component owned by this state machine.</summary>
        public EnemyComponent Enemy => _enemy;

        /// <summary>Resolves enemy AI decisions into action executions.</summary>
        public EnemyActionResolver ActionResolver => _actionResolver;

        /// <summary>
        /// True when the hero has been defeated.
        /// Set via CombatHitResolvedEvent — no external context injection required.
        /// When true, SwitchToReadyState redirects to idle to prevent post-battle actions.
        /// </summary>
        public bool IsBattleEnded => _isBattleEnded;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new enemy state machine and subscribes to battle-lifetime
        /// EventHub events required for cross-system signal routing.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub shared by all systems.</param>
        /// <param name="enemy">Enemy entity component this FSM controls.</param>
        /// <param name="actionResolver">Resolver that converts AI decisions into actions.</param>
        /// <param name="heroEntityId">Entity ID of the hero, used to detect hero defeat.</param>
        public EnemyStateMachine(
            EventHub eventHub,
            EnemyComponent enemy,
            EnemyActionResolver actionResolver,
            int heroEntityId)
        {
            _eventHub = eventHub;
            _enemy = enemy;
            _actionResolver = actionResolver;
            _heroEntityId = heroEntityId;

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

        /// <summary>
        /// Transitions enemy to ready state (preparing to execute an action).
        ///
        /// Battle-end guard: if the hero has been defeated, redirects to idle
        /// instead of ready to prevent this enemy from acting after the battle is over.
        ///
        /// Design decision — why this guard exists here and not in HeroStateMachine:
        /// Hero defeat is detected via CombatHitResolvedEvent in OnCombatHitResolved.
        /// That event arrives while the enemy may still be mid-turn (attack animation
        /// in progress), so the guard must be checked at the point of re-entry into
        /// ReadyState rather than at the event itself.
        ///
        /// HeroStateMachine has no equivalent guard because hero death immediately
        /// transitions the hero FSM to the terminal HeroDefeatedState, and
        /// BattleBootstrapper.OnEntityDied calls TurnContext.EndBattle() which halts
        /// the TurnStateMachine. There is no re-entry path in HeroStateMachine that
        /// requires a guard — the asymmetry is intentional.
        /// </summary>
        public void SwitchToReadyState()
        {
            if (_isBattleEnded)
            {
                GameLogger.Log(LogSourceType.Enemy,
                    $"Enemy[{_enemy.EntityId}] SwitchToReadyState blocked: battle ended.");
                SwitchState(new EnemyIdleState(this));
                return;
            }

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

        /// <summary>
        /// Transitions enemy to defeated state with the direction of the killing blow.
        /// Damage is already applied in EnemyComponent.ReceiveDamage before this is called.
        /// </summary>
        /// <param name="hitDirection">Direction of the killing hit for defeat animation.</param>
        public void SwitchToDefeatedState(HitDirection hitDirection)
        {
            SwitchState(new EnemyDefeatedState(this, hitDirection));
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
        ///
        /// No entity-id filter here: HeroHitReactionEndedPayload does not carry
        /// attacker information. All enemy FSMs receive this signal and forward it.
        /// The active EnemyAttackState uses its own step-synchronization guards
        /// (HitFrameFiredForStep, LastHitStep) to ignore signals that do not
        /// belong to the current combo step.
        /// </summary>
        private void OnHeroHitReactionEnded(HeroHitReactionEndedPayload payload)
        {
            Notify(new EnemyTargetHitReactionEndedSignal(payload));
        }

        /// <summary>
        /// Receives CombatHitResolved from EventHub.
        /// Has two independent responsibilities:
        ///
        /// 1. IsBattleEnded flag — set when hero is the defeated target.
        ///    No entity-id filter here: ALL enemies must detect hero death
        ///    regardless of whether this enemy was the attacker.
        ///
        /// 2. Signal forwarding — only forward when this enemy is the attacker
        ///    or this enemy is the target. Events from hero-vs-other-enemy
        ///    or future companion/PvP actors are irrelevant to this FSM.
        /// </summary>
        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            if (!payload.TargetIsAliveAfter && payload.TargetId == _heroEntityId)
            {
                _isBattleEnded = true;
                GameLogger.Log(LogSourceType.Enemy,
                    $"Enemy[{_enemy.EntityId}]: Hero defeated. IsBattleEnded set.");
            }

            if (payload.AttackerId != _enemy.EntityId && payload.TargetId != _enemy.EntityId)
                return;

            Notify(new EnemyCombatHitResolvedSignal(payload));
        }
    }
}