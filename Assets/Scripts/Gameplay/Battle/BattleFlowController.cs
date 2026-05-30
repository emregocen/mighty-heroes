using System;
using MightyHeroes.Battle.BattleTimer;
using MightyHeroes.Battle.BattleTimer.Payloads;
using MightyHeroes.Battle.TurnFlow;
using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Health.Payloads;
using MightyHeroes.Gameplay.Hero;
using MightyHeroes.Gameplay.Hero.Payloads;

namespace MightyHeroes.Core.Battle
{
    /// <summary>
    /// Listens to battle-end signals and orchestrates the resulting state transitions.
    ///
    /// Responsibilities:
    ///  - Entity death: hero death triggers defeat; enemy death decrements alive count
    ///    and arms the all-enemies-defeated victory flag.
    ///  - Hero attack sequence end: drains the armed victory flag so the transition
    ///    happens once the combo call stack has fully unwound.
    ///  - Battle timer expired: stops the timer and triggers defeat.
    ///
    /// This class replaces the death-listening responsibility previously held by
    /// BattleBootstrapper. Bootstrapper now only wires this controller and disposes it.
    /// </summary>
    public sealed class BattleFlowController : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly TurnContext _turnContext;
        private readonly TurnStateMachine _turnStateMachine;
        private readonly BattleFormation _battleFormation;
        private readonly HeroComponent _hero;
        private readonly BattleTimer _battleTimer;

        private bool _allEnemiesDefeatedPending;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Constructs the controller and registers all battle-end listeners.
        /// The battle timer parameter may be null when the level has no timer config.
        /// </summary>
        public BattleFlowController(
            EventHub eventHub,
            TurnContext turnContext,
            TurnStateMachine turnStateMachine,
            BattleFormation battleFormation,
            HeroComponent hero,
            BattleTimer battleTimer)
        {
            _eventHub = eventHub;
            _turnContext = turnContext;
            _turnStateMachine = turnStateMachine;
            _battleFormation = battleFormation;
            _hero = hero;
            _battleTimer = battleTimer;

            RegisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>Unregisters all event handlers. Safe to call multiple times.</summary>
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
            _eventHub.HealthEvents.EntityDiedEvent.Register(OnEntityDied);
            _eventHub.HeroEvents.HeroAttackSequenceEndedEvent.Register(OnHeroAttackSequenceEnded);
            _eventHub.BattleTimerEvents.ExpiredEvent.Register(OnBattleTimerExpired);
        }

        private void UnregisterEvents()
        {
            _eventHub.HealthEvents.EntityDiedEvent.Unregister(OnEntityDied);
            _eventHub.HeroEvents.HeroAttackSequenceEndedEvent.Unregister(OnHeroAttackSequenceEnded);
            _eventHub.BattleTimerEvents.ExpiredEvent.Unregister(OnBattleTimerExpired);
        }

        private void EndBattleWithDefeat()
        {
            _turnContext.EndBattle();
            _battleTimer?.Stop();
            _turnStateMachine.SwitchToDefeatedState(_turnContext.AliveEnemyCount);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Hero death triggers immediate defeat.
        /// Enemy death decrements alive count and arms the victory pending flag —
        /// the actual transition fires from OnHeroAttackSequenceEnded so the combo
        /// call stack can unwind cleanly first.
        /// </summary>
        private void OnEntityDied(EntityDiedPayload payload)
        {
            if (_turnContext == null)
                return;

            if (_hero != null && payload.EntityId == _hero.EntityId)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    $"BattleFlowController hero died. EntityId={payload.EntityId}");
                EndBattleWithDefeat();
                return;
            }

            _battleFormation.NotifyEnemyDefeated(payload.EntityId);
            _turnContext.UnregisterEnemy();

            GameLogger.Log(LogSourceType.BattleSystem,
                $"BattleFlowController enemy died. EntityId={payload.EntityId} AliveCount={_turnContext.AliveEnemyCount}");

            if (_turnContext.AliveEnemyCount > 0)
                return;

            _turnContext.EndBattle();
            _battleTimer?.Stop();
            _allEnemiesDefeatedPending = true;
        }

        /// <summary>
        /// Drains the victory pending flag once the hero's last attack sequence has ended.
        /// Raises TurnAllEnemiesDefeatedEvent so TurnStateMachine can safely switch to victory.
        /// </summary>
        private void OnHeroAttackSequenceEnded(HeroAttackSequenceEndedPayload payload)
        {
            if (!_allEnemiesDefeatedPending)
                return;

            _allEnemiesDefeatedPending = false;

            _eventHub.TurnEvents.AllEnemiesDefeatedEvent.Raise(
                new TurnAllEnemiesDefeatedPayload(_turnStateMachine.Context.TurnIndex));
        }

        /// <summary>
        /// Timer ran out before the hero could clear all enemies — trigger defeat.
        /// </summary>
        private void OnBattleTimerExpired(BattleTimerExpiredPayload payload)
        {
            GameLogger.Log(LogSourceType.BattleSystem,
                "BattleFlowController timer expired. Triggering defeat.");

            EndBattleWithDefeat();
        }
    }
}