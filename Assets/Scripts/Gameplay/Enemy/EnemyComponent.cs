using UnityEngine;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Entity;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.Presentation;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using MightyHeroes.Gameplay.Enemy.Stats;
using MightyHeroes.UI.Battle.Views;
using MightyHeroes.Gameplay.Animation.Interfaces;

namespace MightyHeroes.Gameplay.Enemy
{
    /// <summary>
    /// Unity-facing component representing the enemy entity in the scene.
    ///
    /// This component is the Unity boundary layer. It knows HOW to play animations
    /// but never decides WHEN to play them. That decision always belongs to a State.
    ///
    /// Health flow (mirrors HeroComponent exactly):
    /// CombatSystem raises DamageIntentEvent
    ///   → OnDamageIntent (self-identifies by EntityId)
    ///     → Health.ApplyDamage (raises HealthEvents)
    ///       → IsAlive check
    ///         → StateMachine.SwitchToDefeatedState or SwitchToTakingHitState
    ///           → State.Enter → PlayTakeHit / PlayDefeat (animator called here, never before)
    ///
    /// Action point flow:
    /// EnemyAttackState.TryConsumeActionPoint()
    ///   → EnemyComponent.TryConsumeActionPoints(1)  ← overrides EntityComponent
    ///     → base.TryConsumeActionPoints(amount)     ← mutates _runtimeStats
    ///       → RefreshActionPointView()              ← updates pip HUD immediately after
    ///
    /// Event raise ownership:
    /// EnemyHitReactionEndedEvent is raised in EnemyTakingHitState.OnHitReactionEnded,
    /// not here. NotifyHitReactionEnded only forwards the signal to the FSM.
    /// </summary>
    public sealed class EnemyComponent : EntityComponent, ICombatTarget, ICombatAttacker, IAnimationSignalReceiver
    {
        // CONSTANTS

        /// <summary>Animator int parameter selecting the attack animation variant.</summary>
        private static readonly int Anim_AttackVariant = Animator.StringToHash("AttackVariant");

        /// <summary>Animator int parameter representing attack intensity (also defines combo length).</summary>
        private static readonly int Anim_AttackIntensity = Animator.StringToHash("AttackIntensity");

        /// <summary>Animator trigger that starts the attack sequence.</summary>
        private static readonly int Anim_DoAttack = Animator.StringToHash("Attack");

        /// <summary>Animator int parameter representing hit direction.</summary>
        private static readonly int Anim_HitDirection = Animator.StringToHash("HitDirection");

        /// <summary>Animator trigger that plays hit reaction animation.</summary>
        private static readonly int Anim_DoHit = Animator.StringToHash("TakeHit");

        /// <summary>Animator trigger that plays defeat animation.</summary>
        private static readonly int Anim_Defeat = Animator.StringToHash("Defeat");

        /// <summary>Animator trigger that advances the combo to the next step.</summary>
        private static readonly int Anim_AdvanceCombo = Animator.StringToHash("AdvanceCombo");

        /// <summary>Animator trigger that forces the attack animation to finish early.</summary>
        private static readonly int Anim_ForceFinishCombo = Animator.StringToHash("ForceFinishCombo");

        // FIELDS

        /// <summary>Static enemy stats assigned via inspector. Used only to initialize runtime stats.</summary>
        [SerializeField] private EnemyStatsData _enemyStatsData;

        /// <summary>Animator responsible for playing enemy animations.</summary>
        [SerializeField] private Animator _animator;

        /// <summary>Receives animation events and forwards them to EnemyStateMachine as signals.</summary>
        [SerializeField] private EnemyAnimationEventProxy _animationEventProxy;

        [Header("HUD")]
        /// <summary>Enemy health bar presentation view.</summary>
        [SerializeField] private HealthBarView _healthBarView;

        /// <summary>Enemy shield bar presentation view.</summary>
        [SerializeField] private ShieldBarView _shieldBarView;

        /// <summary>Enemy action point presentation view.</summary>
        [SerializeField] private ActionPointView _actionPointView;

        /// <summary>Reference to the enemy state machine. Assigned by BattleBootstrapper during Initialize().</summary>
        private EnemyStateMachine _stateMachine;

        /// <summary>Reference to the battle-scoped event hub. Cached from stateMachine.EventHub on Initialize().</summary>
        private EventHub _eventHub;

        // PROPERTIES

        /// <summary>Exposes the owning enemy state machine.</summary>
        public EnemyStateMachine StateMachine => _stateMachine;

        // PUBLIC METHODS — Initialization

        /// <summary>
        /// Battle-scope entry point. Called by BattleBootstrapper.
        ///
        /// Order matters:
        /// 1. Assign _stateMachine and cache _eventHub — required by OnInitialized.
        /// 2. base.Initialize(entityId) — assigns EntityId, calls OnInitialized.
        /// 3. InitializeHealth — requires EntityId, must follow base.Initialize.
        /// 4. RegisterEvents — requires EntityId to filter DamageIntentEvent.
        ///
        /// FSM is NOT started here. Call StartStateMachine() explicitly after Initialize()
        /// so BattleBootstrapper owns the full actor boot sequence for all entity types.
        /// </summary>
        /// <param name="stateMachine">Battle-scoped enemy state machine.</param>
        /// <param name="entityId">Unique entity identifier assigned by BattleBootstrapper.</param>
        public void Initialize(EnemyStateMachine stateMachine, int entityId)
        {
            _stateMachine = stateMachine;
            _eventHub = stateMachine.EventHub;

            base.Initialize(entityId);
            base.InitializeHealth(_eventHub, _enemyStatsData.MaxHealth, _enemyStatsData.MaxShield);

            RegisterEvents();
            InitializeHud();
        }

        /// <summary>
        /// Starts the enemy FSM. Must be called by BattleBootstrapper after Initialize().
        /// Separated from Initialize so the bootstrapper controls the exact start order
        /// for all entity types (hero, enemy, companion) uniformly.
        /// </summary>
        public void StartStateMachine()
        {
            _stateMachine.Start();
        }

        /// <summary>
        /// Unregisters EventHub listeners.
        /// Called by BattleBootstrapper.OnDestroy.
        /// </summary>
        public void Dispose()
        {
            UnregisterEvents();
        }

        // PUBLIC METHODS — Action Points

        /// <summary>
        /// Attempts to consume the specified amount of action points and
        /// immediately refreshes the AP HUD to reflect the new value.
        ///
        /// This override is the single correct place to update ActionPointView
        /// on consumption. State classes must not call RefreshActionPointView
        /// directly; they call this method via EntityComponent's public API.
        /// </summary>
        /// <param name="amount">Action point cost to consume.</param>
        /// <returns>True if the action points were consumed successfully.</returns>
        public new bool TryConsumeActionPoints(int amount)
        {
            bool consumed = base.TryConsumeActionPoints(amount);

            if (consumed)
                RefreshActionPointView();

            return consumed;
        }

        /// <summary>
        /// Resets action points at the start of a new enemy turn and
        /// immediately refreshes the AP HUD to reflect the restored value.
        ///
        /// This override is the single correct place to update ActionPointView
        /// on reset. Mirrors TryConsumeActionPoints override by the same pattern.
        /// </summary>
        public new void ResetActionPoints()
        {
            base.ResetActionPoints();
            RefreshActionPointView();
        }

        // PUBLIC METHODS — Animation Commands

        /// <summary>
        /// Plays hit reaction animation.
        /// Must only be called from a State (EnemyTakingHitState).
        /// </summary>
        /// <param name="hitDirection">Direction of the incoming hit.</param>
        public void PlayTakeHit(HitDirection hitDirection)
        {
            _animator.SetInteger(Anim_HitDirection, (int)hitDirection);
            _animator.SetTrigger(Anim_DoHit);
        }

        /// <summary>
        /// Starts the attack animation sequence.
        /// Must only be called from a State (EnemyAttackState).
        ///
        /// AdvanceCombo trigger is reset before starting a new sequence to prevent
        /// a stale trigger from a previous combo from firing immediately on the
        /// first clip of the new attack.
        /// </summary>
        /// <param name="attackVariant">Attack animation variant index.</param>
        /// <param name="attackIntensity">Attack intensity level (also defines combo length).</param>
        public void PlayAttack(int attackVariant, int attackIntensity)
        {
            _animator.SetInteger(Anim_AttackVariant, attackVariant);
            _animator.SetInteger(Anim_AttackIntensity, attackIntensity);
            _animator.ResetTrigger(Anim_AdvanceCombo);
            _animator.SetTrigger(Anim_DoAttack);
        }

        /// <summary>
        /// Advances the current combo to the next step.
        /// Must only be called from a State (EnemyAttackState).
        /// </summary>
        /// <param name="nextStepIndex">Next combo step index (1-based).</param>
        public void AdvanceCombo(int nextStepIndex)
        {
            _animator.SetTrigger(Anim_AdvanceCombo);
        }

        /// <summary>
        /// Forces the attack animation to finish when the target dies mid-combo.
        /// Must only be called from a State (EnemyAttackState).
        /// </summary>
        public void PlayForcedAttackFinish()
        {
            _animator.SetTrigger(Anim_ForceFinishCombo);
        }

        /// <summary>
        /// Plays the defeat animation toward the direction of the killing blow.
        /// Must only be called from EnemyDefeatedState.
        /// </summary>
        /// <param name="hitDirection">Direction of the killing hit for animation.</param>
        public void PlayDefeat(HitDirection hitDirection)
        {
            _animator.SetInteger(Anim_HitDirection, (int)hitDirection);
            _animator.SetTrigger(Anim_Defeat);
        }

        // PUBLIC METHODS — HUD

        /// <summary>
        /// Activates or dims the AP view to indicate turn ownership.
        /// Called by EnemyPreReadyState.Enter (activate) and EnemyIdleState.Enter (dim).
        /// </summary>
        /// <param name="isActive">True when this enemy's turn is active.</param>
        public void SetActionPointViewTurnActive(bool isActive)
        {
            _actionPointView?.SetTurnActive(isActive);
        }

        // PUBLIC METHODS — Animation Event Bridge

        /// <summary>
        /// Called by EnemyHitReactionEndedBehaviour when hit reaction animation completes.
        /// Forwards to FSM as signal only. Event raise belongs to EnemyTakingHitState.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            _stateMachine.NotifyHitReactionEnded();
        }

        /// <summary>
        /// Called by animation state behaviour when a single attack clip ends.
        /// </summary>
        public void NotifyAttackClipEnded()
        {
            _stateMachine.NotifyAttackClipEnded();
        }

        /// <summary>
        /// Called by EnemyAttackSequenceEndedBehaviour when the full attack sequence ends.
        /// </summary>
        public void NotifyAttackSequenceEnded()
        {
            _stateMachine.NotifyAttackSequenceEnded();
        }

        /// <summary>
        /// Called by AttackHitBehaviour when the hit frame of the attack animation is reached.
        /// </summary>
        /// <param name="hitDirection">Direction of the hit.</param>
        public void NotifyAttackHit(HitDirection hitDirection)
        {
            _stateMachine.NotifyAttackHitFrame(hitDirection);
        }

        // PROTECTED METHODS

        /// <summary>Provides enemy-specific stats data for base class runtime stat initialization.</summary>
        protected override EntityStatsData GetStatsData()
        {
            return _enemyStatsData;
        }

        /// <summary>
        /// Called by EntityComponent.Initialize after EntityId is assigned.
        /// Binds the animation event proxy to the state machine.
        /// FSM is NOT started here — StartStateMachine() must be called explicitly by BattleBootstrapper.
        /// </summary>
        protected override void OnInitialized()
        {
            _animationEventProxy.Initialize(_stateMachine);
        }

        // PRIVATE METHODS

        /// <summary>
        /// Initializes all enemy HUD views with correct runtime values at battle start.
        /// AP view starts dimmed — enemy turn is not active at battle start.
        /// Called once from Initialize() after health and stats are ready.
        /// </summary>
        private void InitializeHud()
        {
            RefreshHealthView(false);
            RefreshShieldView(false);
            RefreshActionPointView();
            SetActionPointViewTurnActive(false);
        }

        /// <summary>
        /// Registers listeners for combat events relevant to the enemy component.
        /// </summary>
        private void RegisterEvents()
        {
            _eventHub.CombatEvents.CombatDamageIntentEvent.Register(OnDamageIntent);
        }

        /// <summary>
        /// Unregisters listeners for combat events. Called by Dispose().
        /// </summary>
        private void UnregisterEvents()
        {
            _eventHub.CombatEvents.CombatDamageIntentEvent.Unregister(OnDamageIntent);
        }

        /// <summary>
        /// Refreshes the health bar view from the current runtime health state.
        /// </summary>
        /// <param name="animate">Whether the presentation should animate the change.</param>
        private void RefreshHealthView(bool animate)
        {
            if (_healthBarView == null || Health == null)
                return;

            _healthBarView.SetValue(Health.CurrentHealth, Health.MaxHealth, animate);
        }

        /// <summary>
        /// Refreshes the shield bar view from the current runtime shield state.
        /// </summary>
        /// <param name="animate">Whether the presentation should animate the change.</param>
        private void RefreshShieldView(bool animate)
        {
            if (_shieldBarView == null || Health == null)
                return;

            _shieldBarView.SetValue(Health.CurrentShield, Health.MaxShield, animate);
            _shieldBarView.SetVisible(Health.MaxShield > 0 && Health.CurrentShield > 0);
        }

        /// <summary>
        /// Refreshes the action point pip view from the current runtime AP state.
        /// Called on initialization and after every successful AP consumption or reset.
        /// </summary>
        private void RefreshActionPointView()
        {
            if (_actionPointView == null)
                return;

            _actionPointView.SetValue(CurrentActionPoints, _enemyStatsData.BaseActionPoints);
            _actionPointView.SetVisible(true);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Listens to DamageIntentEvent and self-identifies by EntityId.
        ///
        /// Applies damage through Health so HealthEvents are raised,
        /// then notifies the state machine for the correct FSM transition.
        /// This method does NOT play animations — that is the entering state's responsibility.
        /// </summary>
        private void OnDamageIntent(CombatDamageIntentPayload payload)
        {
            if (payload.TargetEntityId != EntityId)
                return;

            GameLogger.Log(LogSourceType.Enemy,
                $"Enemy.OnDamageIntent Damage={payload.DamageAmount} Direction={payload.HitDirection}");

            Health.ApplyDamage(
                amount: payload.DamageAmount,
                damageType: payload.DamageType,
                hitDirection: payload.HitDirection,
                sourceEntityId: payload.AttackerEntityId,
                lifestealPercent: 0f);

            RefreshHealthView(true);
            RefreshShieldView(true);

            _eventHub.CombatEvents.CombatHitResolvedEvent.Raise(
                new CombatHitResolvedPayload(
                    attackerId: payload.AttackerEntityId,
                    targetId: EntityId,
                    stepIndex: payload.StepIndex,
                    totalSteps: payload.TotalSteps,
                    damageApplied: true,
                    targetWasAlive: true,
                    targetIsAliveAfter: IsAlive));

            if (!IsAlive)
            {
                _stateMachine.SwitchToDefeatedState(payload.HitDirection);
                return;
            }

            _stateMachine.SwitchToTakingHitState(payload.HitDirection);
        }
    }
}