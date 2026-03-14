using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Payloads;
using MightyHeroes.Gameplay.Combat.Types;
using MightyHeroes.Gameplay.Enemy.Payloads;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Gameplay.Combat
{
    /// <summary>
    /// Minimal combat resolution system.
    ///
    /// Single responsibility: receive AttackHit intent -> resolve damage -> apply -> publish result.
    ///
    /// Target selection is fully delegated to IBattleField.
    /// This system has zero knowledge of animations, FSMs, formation layout,
    /// or which enemy is in which line.
    /// </summary>
    public sealed class CombatSystem : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly ICombatResolver _combatResolver;
        private readonly ICombatEntity _heroEntity;
        private readonly IBattleField _battleField;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates a new combat system instance and registers event listeners.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="combatResolver">Damage calculation resolver.</param>
        /// <param name="heroEntity">Hero combat entity.</param>
        /// <param name="battleField">Formation query for target selection and enemy lookup.</param>
        public CombatSystem(
            EventHub eventHub,
            ICombatResolver combatResolver,
            ICombatEntity heroEntity,
            IBattleField battleField)
        {
            _eventHub = eventHub;
            _combatResolver = combatResolver;
            _heroEntity = heroEntity;
            _battleField = battleField;

            RegisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Unregisters all event listeners and marks the system as disposed.
        /// Must be called when the battle ends to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            UnregisterEvents();
            _isDisposed = true;
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            _eventHub.HeroEvents.HeroAttackHitEvent.Register(OnHeroAttackHit);
            _eventHub.EnemyEvents.EnemyAttackHitEvent.Register(OnEnemyAttackHit);
        }

        private void UnregisterEvents()
        {
            _eventHub.HeroEvents.HeroAttackHitEvent.Unregister(OnHeroAttackHit);
            _eventHub.EnemyEvents.EnemyAttackHitEvent.Unregister(OnEnemyAttackHit);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Handles hero attack hit events.
        /// Queries BattleFieldManager for the current primary target,
        /// resolves and applies damage, then publishes CombatHitResolved.
        /// </summary>
        private void OnHeroAttackHit(HeroAttackHitPayload payload)
        {
            if (_isDisposed)
                return;

            ICombatAttacker attacker = _heroEntity as ICombatAttacker;

            if (attacker == null)
            {
                GameLogger.Log(LogSourceType.CombatSystem,
                    "CombatSystem.OnHeroAttackHit: hero is not a valid attacker.");
                return;
            }

            ICombatTarget target = _battleField.GetCurrentTarget();

            if (target == null || !target.IsAlive)
            {
                GameLogger.Log(LogSourceType.CombatSystem,
                    "CombatSystem.OnHeroAttackHit: no alive target found.");

                _eventHub.CombatEvents.CombatHitResolvedEvent.Raise(
                    new CombatHitResolvedPayload(
                        attackerId: attacker.EntityId,
                        targetId: -1,
                        stepIndex: payload.StepIndex,
                        totalSteps: payload.TotalSteps,
                        damageApplied: false,
                        targetWasAlive: false,
                        targetIsAliveAfter: false));

                return;
            }

            bool wasAlive = target.IsAlive;

            int damage = _combatResolver.ResolveDamage(
                attacker, target, payload.AttackIntensity);

            GameLogger.Log(LogSourceType.CombatSystem,
                $"Hero hit Enemy[{target.EntityId}] Damage={damage} Direction={payload.HitDirection}");

            target.ReceiveDamage(damage, payload.HitDirection);

            _eventHub.CombatEvents.CombatHitResolvedEvent.Raise(
                new CombatHitResolvedPayload(
                    attackerId: attacker.EntityId,
                    targetId: target.EntityId,
                    stepIndex: payload.StepIndex,
                    totalSteps: payload.TotalSteps,
                    damageApplied: true,
                    targetWasAlive: wasAlive,
                    targetIsAliveAfter: target.IsAlive));
        }

        /// <summary>
        /// Handles enemy attack hit events.
        /// Looks up the attacking enemy via BattleFieldManager, applies damage to hero.
        /// </summary>
        private void OnEnemyAttackHit(EnemyAttackHitPayload payload)
        {
            if (_isDisposed)
                return;

            ICombatAttacker attacker = _battleField.FindEnemyById(payload.EnemyId) as ICombatAttacker;
            ICombatTarget target = _heroEntity as ICombatTarget;

            if (attacker == null || target == null)
            {
                GameLogger.Log(LogSourceType.CombatSystem,
                    "CombatSystem.OnEnemyAttackHit: invalid combat role assignment.");
                return;
            }

            if (!target.IsAlive)
            {
                GameLogger.Log(LogSourceType.CombatSystem,
                    "CombatSystem.OnEnemyAttackHit ignored: hero is not alive.");
                return;
            }

            bool wasAlive = target.IsAlive;

            int damage = _combatResolver.ResolveDamage(attacker, target, payload.AttackIntensity);

            GameLogger.Log(LogSourceType.CombatSystem,
                $"Enemy[{payload.EnemyId}] hit Hero Damage={damage} Direction={payload.HitDirection}");

            target.ReceiveDamage(damage, payload.HitDirection);

            // Raise CombatHitResolved so enemy FSMs can detect hero defeat
            // and set their IsBattleEnded flag via OnCombatHitResolved.
            _eventHub.CombatEvents.CombatHitResolvedEvent.Raise(
                new CombatHitResolvedPayload(
                    attackerId: attacker.EntityId,
                    targetId: target.EntityId,
                    stepIndex: payload.StepIndex,
                    totalSteps: payload.TotalSteps,
                    damageApplied: true,
                    targetWasAlive: wasAlive,
                    targetIsAliveAfter: target.IsAlive));
        }
    }
}