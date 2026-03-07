using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Entity;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Hero.Presentation;
using MightyHeroes.Gameplay.Hero.StateMachine;
using MightyHeroes.Gameplay.Hero.Stats;
using MightyHeroes.Gameplay.Puzzle;

namespace MightyHeroes.Gameplay.Hero
{
    /// <summary>
    /// Unity-facing component representing the hero entity in the scene.
    ///
    /// This component is the Unity boundary layer. It knows HOW to play animations
    /// but never decides WHEN to play them. That decision always belongs to a State.
    ///
    /// Health flow:
    /// CombatSystem -> ReceiveDamage -> Health.ApplyDamage (raises HealthEvents)
    ///   -> IsAlive check -> StateMachine.SwitchToDefeatedState or SwitchToTakingHitState
    ///     -> State.Enter -> PlayTakeHit / PlayDefeat (animator called here, never before)
    ///
    /// Event raise ownership:
    /// HeroHitReactionEndedEvent is raised in HeroTakingHitState.OnHitReactionEnded,
    /// not here. NotifyHitReactionEnded only forwards the signal to the FSM.
    /// </summary>
    public sealed class HeroComponent : EntityComponent, ICombatTarget, ICombatAttacker
    {
        // CONSTANTS — Animator parameter hashes

        /// <summary>Animator int parameter selecting the attack animation variant.</summary>
        private static readonly int Anim_AttackVariant = Animator.StringToHash("AttackVariant");

        /// <summary>Animator int parameter representing attack intensity (also defines combo length).</summary>
        private static readonly int Anim_AttackIntensity = Animator.StringToHash("AttackIntensity");

        /// <summary>Animator trigger that starts the attack sequence.</summary>
        private static readonly int Anim_DoAttack = Animator.StringToHash("DoAttack");

        /// <summary>Animator int parameter representing hit direction.</summary>
        private static readonly int Anim_HitDirection = Animator.StringToHash("HitDirection");

        /// <summary>Animator trigger that plays hit reaction animation.</summary>
        private static readonly int Anim_DoHit = Animator.StringToHash("DoHit");

        /// <summary>Animator trigger that plays defeat animation.</summary>
        private static readonly int Anim_Defeat = Animator.StringToHash("Defeat");

        /// <summary>
        /// Animator trigger that advances an ongoing combo to the next step.
        /// Used for synchronization: waits for target reaction before proceeding.
        /// </summary>
        private static readonly int Anim_AdvanceCombo = Animator.StringToHash("AdvanceCombo");

        /// <summary>
        /// Animator trigger that force-finishes the combo sequence early.
        /// Used when the target dies mid-combo so remaining hit animations are skipped.
        /// </summary>
        private static readonly int Anim_ForceFinishCombo = Animator.StringToHash("ForceFinishCombo");

        // FIELDS

        /// <summary>Static hero stats assigned via inspector. Used only to initialize runtime stats.</summary>
        [SerializeField] private HeroStatsData _heroStatsData;

        /// <summary>Animator responsible for playing hero animations.</summary>
        [SerializeField] private Animator _animator;

        /// <summary>Receives animation events and forwards them to HeroStateMachine as signals.</summary>
        [SerializeField] private HeroAnimationEventProxy _animationEventProxy;

        private HeroStateMachine _stateMachine;

        // PROPERTIES

        /// <summary>Exposes the owning hero state machine.</summary>
        public HeroStateMachine StateMachine => _stateMachine;

        // PUBLIC METHODS — Initialization

        /// <summary>
        /// Battle-scope entry point. Called by BattleBootstrapper.
        ///
        /// Order matters:
        /// 1. Assign _stateMachine — required by OnInitialized.
        /// 2. base.Initialize(entityId) — assigns EntityId, calls OnInitialized.
        /// 3. InitializeHealth — requires EntityId, must follow base.Initialize.
        /// </summary>
        /// <param name="stateMachine">Battle-scoped hero state machine.</param>
        /// <param name="entityId">Unique entity identifier assigned by BattleBootstrapper.</param>
        public void Initialize(HeroStateMachine stateMachine, int entityId)
        {
            _stateMachine = stateMachine;

            base.Initialize(entityId);

            base.InitializeHealth(
                stateMachine.EventHub,
                _heroStatsData.MaxHealth,
                _heroStatsData.MaxShield);
        }

        // PUBLIC METHODS — Combat

        /// <summary>
        /// Receives resolved combat damage from CombatSystem.
        ///
        /// Responsibilities:
        /// 1. Apply damage through Health so HealthEvents are raised.
        /// 2. Check IsAlive and notify state machine for the correct transition.
        ///
        /// This method does NOT play animations.
        /// Animation is the entering state's responsibility, not the component's.
        /// </summary>
        /// <param name="damage">Final resolved damage value from CombatResolver.</param>
        /// <param name="hitDirection">Direction of the incoming hit, forwarded to the state.</param>
        public void ReceiveDamage(int damage, HitDirection hitDirection)
        {
            GameLogger.Log(LogSourceType.Hero,
                $"Hero.ReceiveDamage Damage={damage}, Direction={hitDirection}");

            Health.ApplyDamage(
                amount: damage,
                damageType: DamageType.Physical,
                hitDirection: hitDirection,
                sourceEntityId: 0,
                lifestealPercent: 0f);

            if (!IsAlive)
            {
                _stateMachine.SwitchToDefeatedState();
                return;
            }

            _stateMachine.SwitchToTakingHitState(hitDirection);
        }

        // PUBLIC METHODS — Animation commands (called only by States)

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
        /// Called after the target's hit reaction animation ends, ensuring synchronization.
        /// Must only be called from a State (HeroAttackState).
        /// </summary>
        /// <param name="nextStepIndex">Next combo step index (1-based).</param>
        public void AdvanceCombo(int nextStepIndex)
        {
            _animator.SetTrigger(Anim_AdvanceCombo);
        }

        /// <summary>
        /// Force-finishes the combo sequence early when the target dies mid-combo.
        /// Remaining hit animations are skipped so the hero returns to idle cleanly.
        /// Must only be called from a State (HeroAttackState).
        /// </summary>
        public void PlayForcedAttackFinish()
        {
            _animator.SetTrigger(Anim_ForceFinishCombo);
        }

        /// <summary>
        /// Plays the defeat animation.
        /// Must only be called from a State (HeroDefeatedState).
        /// </summary>
        public void PlayDefeat()
        {
            _animator.SetTrigger(Anim_Defeat);
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

        // PUBLIC METHODS — Animation event bridge

        /// <summary>
        /// Called by HeroHitReactionEndedBehaviour when hit reaction animation completes.
        /// Forwards to FSM as a signal only.
        ///
        /// Event raise responsibility belongs to HeroTakingHitState.OnHitReactionEnded,
        /// not here. Raising the event in both places causes double-fire.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            _stateMachine.NotifyHitReactionEnded();
        }

        /// <summary>
        /// Called by animator state behaviour when a single attack clip ends.
        /// Signal is routed only to HeroAttackState via HeroStateMachine.Notify.
        /// </summary>
        public void NotifyAttackClipEnded()
        {
            _stateMachine.NotifyAttackClipEnded();
        }

        /// <summary>
        /// Called by HeroAttackSequenceEndedBehaviour when the full attack sequence ends.
        /// </summary>
        public void NotifyAttackSequenceEnded()
        {
            _stateMachine.NotifyAttackSequenceEnded();
        }

        // PROTECTED METHODS

        /// <summary>
        /// Provides hero-specific stats data for base class runtime stat initialization.
        /// </summary>
        protected override EntityStatsData GetStatsData()
        {
            return _heroStatsData;
        }

        /// <summary>
        /// Called by EntityComponent.Initialize after EntityId is assigned.
        /// Starts the hero FSM and binds the animation event proxy.
        /// </summary>
        protected override void OnInitialized()
        {
            _stateMachine.Start();
            _animationEventProxy.Initialize(_stateMachine);
        }
    }
}