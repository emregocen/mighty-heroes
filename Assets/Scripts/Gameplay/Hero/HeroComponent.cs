using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Animation.Interfaces;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Common.Entity;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Health.Presentation;
using MightyHeroes.Gameplay.Hero.Movement;
using MightyHeroes.Gameplay.Hero.Payloads;
using MightyHeroes.Gameplay.Hero.Presentation;
using MightyHeroes.Gameplay.Hero.StateMachine;
using MightyHeroes.Gameplay.Hero.Stats;
using MightyHeroes.Gameplay.Puzzle;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;

namespace MightyHeroes.Gameplay.Hero
{
    /// <summary>
    /// Unity-facing component representing the hero entity in the scene.
    ///
    /// This component is the Unity boundary layer. It knows HOW to play animations
    /// but never decides WHEN to play them. That decision always belongs to a State.
    ///
    /// Health flow:
    /// CombatSystem raises DamageIntentEvent
    ///   → OnDamageIntent (self-identifies by EntityId)
    ///     → Health.ApplyDamage (raises HealthEvents)
    ///       → IsAlive check
    ///         → StateMachine.SwitchToDefeatedState or SwitchToTakingHitState
    ///           → State.Enter → PlayTakeHit / PlayDefeat (animator called here, never before)
    ///
    /// Action point flow:
    /// HeroAttackState.TryConsumeActionPoint()
    ///   → HeroComponent.TryConsumeActionPoints(1)  ← overrides EntityComponent
    ///     → base.TryConsumeActionPoints(amount)    ← mutates _runtimeStats
    ///       → RefreshActionPointView()             ← updates pip HUD immediately after
    ///
    /// Event raise ownership:
    /// HeroHitReactionEndedEvent is raised in HeroTakingHitState.OnHitReactionEnded,
    /// not here. NotifyHitReactionEnded only forwards the signal to the FSM.
    /// </summary>
    public sealed class HeroComponent : EntityComponent, ICombatTarget, ICombatAttacker, IAnimationSignalReceiver
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

        /// <summary>Animator trigger that advances an ongoing combo to the next step.</summary>
        private static readonly int Anim_AdvanceCombo = Animator.StringToHash("AdvanceCombo");

        /// <summary>Animator trigger that force-finishes the combo sequence early.</summary>
        private static readonly int Anim_ForceFinishCombo = Animator.StringToHash("ForceFinishCombo");

        /// <summary>Animator trigger that plays the victory animation.</summary>
        private static readonly int Anim_Victory = Animator.StringToHash("Victory");

        /// <summary>Animator int parameter selecting the heal animation variant.</summary>
        private static readonly int Anim_HealVariant = Animator.StringToHash("HealVariant");

        /// <summary>Animator trigger that starts the heal animation.</summary>
        private static readonly int Anim_DoHeal = Animator.StringToHash("Heal");

        // FIELDS

        // FIELDS

        [Header("Data")]
        /// <summary>Static hero stats assigned via inspector. Used only to initialize runtime stats.</summary>
        [SerializeField] private HeroStatsData _heroStatsData;

        [Header("Animation")]
        /// <summary>Animator responsible for playing hero animations.</summary>
        [SerializeField] private Animator _animator;

        /// <summary>Receives animation events and forwards them to HeroStateMachine as signals.</summary>
        [SerializeField] private HeroAnimationEventProxy _animationEventProxy;

        [Header("Targeting")]
        /// <summary>Focus point used by targeting and look-at systems.</summary>
        [SerializeField] private Transform _targetFocusPoint;

        [Header("Movement")]
        [SerializeField]
        private HeroAttackMovementController _attackMovementController;
        [Header("Presentation")]
        [SerializeField] private HeroTargetFocusController _heroTargetFocusController;
        /// <summary>Plays the heal particle effect when this hero receives a heal effect request.</summary>
        [SerializeField] private HealEffectView _healEffectView;

        [Header("HUD")]
        /// <summary>Hero health bar presentation view.</summary>
        [SerializeField] private HealthBarView _healthBarView;

        /// <summary>Hero shield bar presentation view.</summary>
        [SerializeField] private ShieldBarView _shieldBarView;

        /// <summary>Hero action point presentation view.</summary>
        [SerializeField] private ActionPointView _actionPointView;

        /// <summary>Reference to the hero state machine. Assigned by BattleBootstrapper during Initialize().</summary>
        private HeroStateMachine _stateMachine;

        /// <summary>Reference to the battle-scoped event hub. Cached from stateMachine.EventHub on Initialize().</summary>
        private EventHub _eventHub;

        // PROPERTIES

        /// <summary>Exposes the target focus point used by look-at and targeting systems.</summary>
        public Transform TargetFocusPoint => _targetFocusPoint;

        /// <summary>Exposes the owning hero state machine.</summary>
        public HeroStateMachine StateMachine => _stateMachine;


        /// <summary>
        /// Exposes the attack movement controller for state-level event subscriptions.
        /// HeroRetreatState subscribes to RetreatCompleted to know when DOTween finishes.
        /// </summary>
        public HeroAttackMovementController AttackMovementController => _attackMovementController;

        /// <summary>
        /// Indicates whether the player can currently change the active combat target.
        /// </summary>
        public bool CanChangeTarget { get; private set; }

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
        /// <param name="stateMachine">Battle-scoped hero state machine.</param>
        /// <param name="entityId">Unique entity identifier assigned by BattleBootstrapper.</param>
        public void Initialize(HeroStateMachine stateMachine, int entityId)
        {
            GameLogger.Log(LogSourceType.Hero, "HeroComponent.Initialize called.");

            _stateMachine = stateMachine;
            _eventHub = stateMachine.EventHub;

            base.Initialize(entityId);
            base.InitializeHealth(_eventHub, _heroStatsData.MaxHealth, _heroStatsData.MaxShield);
            
            _healEffectView?.Initialize(_eventHub, EntityId);

            RegisterEvents();
            InitializeHud();
        }

        /// <summary>
        /// Starts the hero FSM. Must be called by BattleBootstrapper after Initialize().
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
        /// Resets action points at the start of a new hero turn and
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
        /// Advances the current combo to the next step.
        /// Must only be called from a State (HeroAttackState).
        /// </summary>
        /// <param name="nextStepIndex">Next combo step index (1-based).</param>
        public void AdvanceCombo(int nextStepIndex)
        {
            _animator.SetTrigger(Anim_AdvanceCombo);
        }

        /// <summary>
        /// Plays hit reaction animation.
        /// Must only be called from a State (HeroTakingHitState).
        /// </summary>
        /// <param name="hitDirection">Direction of the incoming hit.</param>
        public void PlayTakeHit(HitDirection hitDirection)
        {
            _animator.SetInteger(Anim_HitDirection, (int)hitDirection);
            _animator.SetTrigger(Anim_DoHit);
        }

        /// <summary>
        /// Starts the attack animation sequence.
        /// Must only be called from a State (HeroAttackState).
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
        /// Force-finishes the combo sequence early when the target dies mid-combo.
        /// Must only be called from a State (HeroAttackState).
        /// </summary>
        public void PlayForcedAttackFinish()
        {
            _animator.SetTrigger(Anim_ForceFinishCombo);
        }

        /// <summary>
        /// Plays the defeat animation toward the direction of the killing blow.
        /// Must only be called from HeroDefeatedState.
        /// </summary>
        /// <param name="hitDirection">Direction of the killing hit for animation.</param>
        public void PlayDefeat(HitDirection hitDirection)
        {
            _animator.SetInteger(Anim_HitDirection, (int)hitDirection);
            _animator.SetTrigger(Anim_Defeat);
        }

        /// <summary> Plays the victory animation.
        /// Must only be called from HeroVictoryState.
        /// </summary>
        // HeroComponent.PlayVictoryAnimation():
        public void PlayVictoryAnimation()
        {
            _attackMovementController?.NotifyVictoryStarted();
            _heroTargetFocusController?.NotifyVictoryStarted();
            _animator.SetTrigger(Anim_ForceFinishCombo);
            _animator.SetTrigger(Anim_Victory);
        }

        /// <summary>
        /// Starts the heal animation and fades LookAt IK out so the heal clip
        /// is not bent toward the current target.
        /// Shared by all support actions until dedicated clips exist.
        /// Must only be called from a State (HeroSupportState).
        /// </summary>
        /// <param name="healVariant">Heal animation variant index.</param>
        public void PlayHeal(int healVariant)
        {
            _heroTargetFocusController?.NotifyHealStarted();
            _animator.SetInteger(Anim_HealVariant, healVariant);
            _animator.SetTrigger(Anim_DoHeal);
        }

        /// <summary>
        /// Applies self-heal to the hero's own health and refreshes the health bar.
        /// Must only be called from a State (HeroSupportState).
        /// </summary>
        /// <param name="amount">Heal amount to apply.</param>
        public void ApplyHeal(int amount)
        {
            Health.Heal(amount, sourceEntityId: EntityId);
            RefreshHealthView(true);
        }

        /// <summary>
        /// Grants absorb shield to the hero (capped by MaxShield) and refreshes the shield bar.
        /// Shield persists until depleted by incoming damage.
        /// Must only be called from a State (HeroSupportState).
        /// </summary>
        /// <param name="amount">Shield amount to grant.</param>
        public void ApplyShield(int amount)
        {
            Health.AddShield(amount, sourceEntityId: EntityId);
            RefreshShieldView(true);
        }

        // PUBLIC METHODS — HUD

        /// <summary>
        /// Activates or dims the AP view to indicate turn ownership.
        /// Called by HeroPreReadyState.Enter (activate) and HeroIdleState.Enter (dim).
        /// </summary>
        /// <param name="isActive">True when the hero turn is active.</param>
        public void SetActionPointViewTurnActive(bool isActive)
        {
            _actionPointView?.SetTurnActive(isActive);
        }

        /// <summary>
        /// Re-applies initial HUD values. Called from BattleBootstrapper.Start to
        /// guarantee all view Awake() calls have completed before values are pushed.
        /// </summary>
        public void RefreshHud()
        {
            RefreshHealthView(false);
            RefreshShieldView(false);
            RefreshActionPointView();
            SetActionPointViewTurnActive(false);
        }

        // PUBLIC METHODS — Puzzle

        /// <summary>
        /// Enables or disables puzzle input based on turn state.
        /// Must only be called from a State (HeroReadyState, HeroIdleState).
        /// </summary>
        /// <param name="isPlayable">True to enable puzzle input.</param>
        public void SetPuzzlePlayable(bool isPlayable)
        {
            PuzzleController.Instance.IsPlayable = isPlayable;
        }

        // PUBLIC METHODS — Targeting

        /// <summary>
        /// Enables or disables player target selection input.
        /// </summary>
        /// <param name="canChangeTarget">
        /// True if the player can currently change combat target.
        /// </param>
        public void SetCanChangeTarget(bool canChangeTarget)
        {
            CanChangeTarget = canChangeTarget;
        }

        /// <summary>
        /// Cancels an in-progress retreat immediately without completing it.
        /// Called by HeroStateMachine.OnBattleVictory before switching to VictoryState
        /// so the DOTween tween is killed and RetreatCompleted never fires.
        /// </summary>
        public void CancelRetreat()
        {
            _attackMovementController?.CancelRetreat();
        }

        // PUBLIC METHODS — Animation Event Bridge

        /// <summary>
        /// Called by HeroHitReactionEndedBehaviour when hit reaction animation completes.
        /// Forwards to FSM as a signal only. Event raise belongs to HeroTakingHitState.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            _stateMachine.NotifyHitReactionEnded();
        }

        /// <summary>
        /// Called by animator state behaviour when a single attack clip ends.
        /// </summary>
        public void NotifyAttackClipEnded()
        {
            _stateMachine.NotifyAttackClipEnded();
        }

        /// <summary>
        /// Called by AttackSequenceStartedBehaviour when the full attack sequence begins.
        /// </summary>
        public void NotifyAttackSequenceStarted()
        {
            GameLogger.Log(LogSourceType.Hero,
                $"NotifyAttackSequenceStarted — heroRoot={_attackMovementController?.transform.position}");
            _attackMovementController?.NotifyAttackStarted();
        }

        /// <summary>
        /// Called by AttackSequenceEndedBehaviour when the full attack sequence ends.
        /// Starts the retreat movement via the controller, then signals the FSM to
        /// transition to HeroRetreatState. The FSM state will subscribe to
        /// RetreatCompleted to know when movement actually finishes.
        /// </summary>
        public void NotifyAttackSequenceEnded()
        {
            _attackMovementController?.NotifyAttackEnded();
            _eventHub.HeroEvents.HeroAttackSequenceEndedEvent.Raise(new HeroAttackSequenceEndedPayload());
            _stateMachine.NotifyAttackSequenceEnded();
        }

        /// <summary> 
        /// Called by HeroAttackHitBehaviour when the hit frame of the attack animation is reached.
        /// </summary> <param name="hitDirection">Direction of the hit, encoded as an integer value of HitDirection.</param>
        public void NotifyAttackHit(HitDirection hitDirection)
        {
            _stateMachine.NotifyAttackHitFrame(hitDirection);
        }

        /// <summary>
        /// Called by VictorySequenceEndedBehaviour when the victory animation clip finishes.
        /// Forwards to the active state via the FSM signal system.
        /// </summary>
        public void NotifyVictorySequenceEnded()
        {
            _stateMachine.NotifyVictorySequenceEnded();
        }

        /// <summary>
        /// Called by VictoryTurnEndedBehaviour when the turn animation clip finishes.
        /// Forwards to the active state via the FSM signal system.
        /// </summary>
        public void NotifyVictoryTurnEnded()
        {
            GameLogger.Log(LogSourceType.Hero, "HeroComponent: NotifyVictoryTurnEnded called.");

            _stateMachine.NotifyVictoryTurnEnded();
        }

        /// <summary>
        /// Called by HeroAttackMovementController once the hero reaches their cached
        /// origin position. Forwards to FSM as HeroRetreatEndedSignal.
        /// </summary>
        private void NotifyRetreatCompleted()
        {
            _stateMachine.NotifyRetreatEnded();
        }

        /// <summary>
        /// Called by HealSequenceEndedBehaviour when the heal animation clip finishes.
        /// Restores LookAt IK, then forwards to the FSM as a signal only.
        /// </summary>
        public void NotifyHealSequenceEnded()
        {
            _heroTargetFocusController?.NotifyHealEnded();
            _stateMachine.NotifyHealSequenceEnded();
        }

        // PROTECTED METHODS

        /// <summary>Provides hero-specific stats data for base class runtime stat initialization.</summary>
        protected override EntityStatsData GetStatsData()
        {
            return _heroStatsData;
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
        /// Initializes all hero HUD views with correct runtime values at battle start.
        /// AP view starts dimmed — hero turn is not active at battle start.
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
        /// Registers listeners for relevant EventHub events.
        /// </summary>
        private void RegisterEvents()
        {
            _eventHub.CombatEvents.CombatDamageIntentEvent.Register(OnDamageIntent);

            // Wire movement controller event into FSM signal chain.
            if (_attackMovementController != null)
                _attackMovementController.RetreatCompleted += OnRetreatCompleted;
        }

        /// <summary>
        /// Unregisters listeners for EventHub events.
        /// </summary>
        private void UnregisterEvents()
        {
            _eventHub.CombatEvents.CombatDamageIntentEvent.Unregister(OnDamageIntent);

            if (_attackMovementController != null)
                _attackMovementController.RetreatCompleted -= OnRetreatCompleted;
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

            _actionPointView.SetValue(CurrentActionPoints, _heroStatsData.BaseActionPoints);
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

            GameLogger.Log(LogSourceType.Hero,
                $"Hero.OnDamageIntent Damage={payload.DamageAmount} Direction={payload.HitDirection}");

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

        /// <summary>
        /// Called by HeroAttackMovementController when the DOTween retreat reaches
        /// the cached origin. Forwards to FSM as a signal via HeroStateMachine.
        /// Follows the same pattern as NotifyHitReactionEnded and NotifyAttackSequenceEnded:
        /// MonoBehaviour events never reach states directly.
        /// </summary>
        private void OnRetreatCompleted()
        {
            _stateMachine.NotifyRetreatEnded();
        }
    }
}