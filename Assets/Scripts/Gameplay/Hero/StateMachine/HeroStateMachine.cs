using MightyHeroes.Battle.TurnFlow.Payloads;
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
    /// Design note — state-local vs battle-lifetime subscriptions:
    /// State-local subscriptions (puzzle match, turn start) live inside each
    /// state's Enter/Exit. Battle-lifetime subscriptions are set up once in the
    /// constructor and torn down in Dispose.
    ///
    /// Design note — no battle-end guard in this FSM:
    /// Unlike EnemyStateMachine, HeroStateMachine has no IsBattleEnded flag or
    /// SwitchToReadyState guard. This asymmetry is intentional:
    /// When the hero dies, HeroComponent.OnDamageIntent calls SwitchToDefeatedState,
    /// which transitions the hero FSM to the terminal HeroDefeatedState immediately.
    /// BattleBootstrapper.OnEntityDied then calls TurnContext.EndBattle(), halting
    /// the TurnStateMachine. There is no re-entry path into hero active states after
    /// death, so no guard is needed. EnemyStateMachine requires its guard because
    /// enemies can receive a turn signal while already processing the hero's final
    /// attack in the same turn cycle.
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
        public HeroStateMachine(EventHub eventHub, HeroComponent hero, HeroActionResolver actionResolver)
        {
            _eventHub = eventHub;
            _hero = hero;
            _actionResolver = actionResolver;

            _eventHub.EnemyEvents.EnemyHitReactionEndedEvent.Register(OnEnemyHitReactionEnded);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Register(OnCombatHitResolved);
            _eventHub.TurnEvents.TurnBattleVictoryEvent.Register(OnBattleVictory);

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
            _eventHub.TurnEvents.TurnBattleVictoryEvent.Unregister(OnBattleVictory);

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

        /// <summary>
        /// Transitions hero to pre-ready state after turn phase starts.
        /// Pre-ready state owns AP reset, AP view activation and the pre-action delay.
        /// </summary>
        public void SwitchToPreReadyState()
        {
            SwitchState(new HeroPreReadyState(this));
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

        /// <summary>
        /// Transitions hero to defeated state with the direction of the killing blow.
        /// Damage is already applied in HeroComponent.ReceiveDamage before this is called.
        /// </summary>
        /// <param name="hitDirection">Direction of the killing hit for defeat animation.</param>
        public void SwitchToDefeatedState(HitDirection hitDirection)
        {
            SwitchState(new HeroDefeatedState(this, hitDirection));
        }

        /// <summary>
        /// Transitions hero to terminal victory state.
        /// Called when BattleVictoryEvent is received in HeroIdleState.
        /// No further state transitions occur after this point.
        /// </summary>
        public void SwitchToVictoryState()
        {
            SwitchState(new HeroVictoryState(this));
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

        /// <summary>
        /// Forwards the victory sequence ended notification into the active state as a signal.
        /// </summary>
        public void NotifyVictorySequenceEnded()
        {
            Notify(new HeroVictorySequenceEndedSignal());
        }

        /// <summary>
        /// Forwards the victory turn ended notification into the active state as a signal.
        /// </summary>
        public void NotifyVictoryTurnEnded()
        {
            Notify(new HeroVictoryTurnEndedSignal());
        }

        // ------------------------------------------------------------------
        // EVENT HANDLERS
        // ------------------------------------------------------------------

        /// <summary>
        /// Battle-lifetime subscription. Receives EnemyHitReactionEnded from EventHub
        /// and routes it into the active state as a local signal.
        /// Kept here instead of HeroAttackState to avoid reentrant unregister risk
        /// during forced finish sequences where Exit() and the event can occur in the same call stack.
        /// </summary>
        private void OnEnemyHitReactionEnded(EnemyHitReactionEndedPayload payload)
        {
            Notify(new HeroTargetHitReactionEndedSignal(payload));
        }

        /// <summary>
        /// Battle-lifetime subscription. Receives CombatHitResolved from EventHub
        /// and routes it into the active state as a local signal.
        /// Kept here instead of HeroAttackState for the same reentrant safety reason as OnEnemyHitReactionEnded.
        /// Guard ensures only events where the hero is attacker or target are forwarded.
        /// </summary>
        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            if (payload.AttackerId != _hero.EntityId && payload.TargetId != _hero.EntityId)
                return;

            Notify(new HeroCombatHitResolvedSignal(payload));
        }

        /// <summary>
        /// Battle-lifetime subscription. Receives BattleVictoryEvent from EventHub
        /// and transitions hero to terminal victory state regardless of which state is currently active.
        /// Kept here because the event must be caught in any hero state — the timing is not predictable
        /// from the hero FSM's perspective.
        /// </summary>
        private void OnBattleVictory(TurnBattleVictoryPayload payload)
        {
            SwitchToVictoryState();
        }
    }
}