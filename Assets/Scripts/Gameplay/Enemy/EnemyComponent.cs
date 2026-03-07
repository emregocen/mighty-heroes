using UnityEngine;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Common.Entity;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Common.Stats;
using MightyHeroes.Gameplay.Enemy.Presentation;
using MightyHeroes.Gameplay.Enemy.StateMachine;
using MightyHeroes.Gameplay.Enemy.Stats;
using MightyHeroes.Gameplay.Health;

namespace MightyHeroes.Gameplay.Enemy
{
    /// <summary>
    /// Unity-facing component representing the enemy entity in the scene.
    ///
    /// This component is the Unity boundary layer. It knows HOW to play animations
    /// but never decides WHEN to play them. That decision always belongs to a State.
    ///
    /// Health flow (mirrors HeroComponent exactly):
    /// CombatSystem -> ReceiveDamage -> Health.ApplyDamage (raises HealthEvents)
    ///   -> IsAlive check -> StateMachine.SwitchToDefeatedState or SwitchToTakingHitState
    ///     -> State.Enter -> PlayTakeHit / PlayDefeat (animator called here, never before)
    /// </summary>
    public sealed class EnemyComponent : EntityComponent, ICombatTarget, ICombatAttacker
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

        /// <summary>Animator trigger that advances the combo to the next step.</summary>
        private static readonly int Anim_AdvanceCombo = Animator.StringToHash("AdvanceCombo");

        /// <summary>
        /// Animator trigger that forces the attack animation to finish
        /// when the enemy is defeated before the animation naturally ends.
        /// </summary>
        private static readonly int Anim_ForceFinishCombo = Animator.StringToHash("ForceFinishCombo");

        // FIELDS

        /// <summary>Static enemy stats assigned via inspector. Used only to initialize runtime stats.</summary>
        [SerializeField] private EnemyStatsData _enemyStatsData;

        /// <summary>Animator responsible for playing enemy animations.</summary>
        [SerializeField] private Animator _animator;

        /// <summary>Receives animation events and forwards them to EnemyStateMachine as signals.</summary>
        [SerializeField] private EnemyAnimationEventProxy _animationEventProxy;

        private EnemyStateMachine _stateMachine;

        // PROPERTIES

        /// <summary>Exposes the owning enemy state machine.</summary>
        public EnemyStateMachine StateMachine => _stateMachine;

        // PUBLIC METHODS — Initialization

        /// <summary>
        /// Battle-scope entry point. Called by BattleBootstrapper.
        ///
        /// Order matters:
        /// 1. Assign _stateMachine — required by OnInitialized.
        /// 2. base.Initialize(entityId) — assigns EntityId, calls OnInitialized.
        /// 3. InitializeHealth — requires EntityId, must follow base.Initialize.
        /// </summary>
        /// <param name="stateMachine">Battle-scoped enemy state machine.</param>
        /// <param name="entityId">Unique entity identifier assigned by BattleBootstrapper.</param>
        public void Initialize(EnemyStateMachine stateMachine, int entityId)
        {
            _stateMachine = stateMachine;

            base.Initialize(entityId);
            base.InitializeHealth(_stateMachine.EventHub, _enemyStatsData.MaxHealth, _enemyStatsData.MaxShield);
        }

        // PUBLIC METHODS — Combat

        /// <summary>
        /// Receives resolved combat damage from CombatSystem.
        ///
        /// Responsibilities (identical pattern to HeroComponent.ReceiveDamage):
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
            GameLogger.Log(LogSourceType.Enemy,
                $"Enemy.ReceiveDamage Damage={damage}, Direction={hitDirection}");

            Health.ApplyDamage(damage, DamageType.Physical, hitDirection, 0, 0f);

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
        /// </summary>
        /// <param name="attackVariant">Attack animation variant index.</param>
        /// <param name="attackIntensity">Attack intensity level (also defines combo length).</param>
        public void PlayAttack(int attackVariant, int attackIntensity)
        {
            _animator.SetInteger(Anim_AttackVariant, attackVariant);
            _animator.SetInteger(Anim_AttackIntensity, attackIntensity);
            _animator.SetTrigger(Anim_DoAttack);
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
        /// Plays the defeat animation.
        /// Must only be called from a State (EnemyDefeatedState).
        /// </summary>
        public void PlayDefeat()
        {
            _animator.SetTrigger(Anim_Defeat);
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

        // PUBLIC METHODS — Animation event bridge

        /// <summary>
        /// Called by EnemyHitReactionEndedBehaviour when hit reaction animation completes.
        /// Forwards to FSM as signal only.
        ///
        /// Event raise responsibility belongs to EnemyTakingHitState.OnHitReactionEnded.
        /// Raising the event here would cause double-fire.
        /// </summary>
        public void NotifyHitReactionEnded()
        {
            _stateMachine.NotifyHitReactionEnded();
        }

        /// <summary>
        /// Called by animation state behaviour when a single attack clip ends.
        /// Signal is routed only to EnemyAttackState via EnemyStateMachine.Notify.
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

        // PROTECTED METHODS

        /// <summary>
        /// Provides enemy-specific stats data for base class runtime stat initialization.
        /// </summary>
        protected override EntityStatsData GetStatsData()
        {
            return _enemyStatsData;
        }

        /// <summary>
        /// Called by EntityComponent.Initialize after EntityId is assigned.
        /// Binds the animation event proxy to the state machine.
        /// </summary>
        protected override void OnInitialized()
        {
            _animationEventProxy.Initialize(_stateMachine);
        }
    }
}