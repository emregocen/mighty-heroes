using System.Collections.Generic;
using MightyHeroes.Core.Logging;
using MightyHeroes.Core.TurnSystem.Payloads;
using MightyHeroes.Core.TurnSystem.Types;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Enemy;
using MightyHeroes.Gameplay.Enemy.Payloads;

namespace MightyHeroes.Core.TurnSystem.States
{
    /// <summary>
    /// Controls the enemy action phase of the turn.
    /// Enemies act strictly one at a time, front line before back line.
    ///
    /// Enemy ordering is derived from IFormationQuery.Lines:
    /// lines are iterated front-to-back (index 0 first),
    /// within each line enemies are iterated left-to-right.
    /// This guarantees front-line enemies always act before back-line enemies.
    ///
    /// Turn ownership:
    /// TurnEnemyStartedEvent is raised with the active enemy's id.
    /// Each EnemyIdleState listens and only the matching enemy transitions to ReadyState.
    /// Non-active enemies remain in Idle and cannot act out of turn.
    /// </summary>
    public sealed class TurnEnemyState : TurnBaseState
    {
        // FIELDS

        private readonly List<EnemyComponent> _enemies = new List<EnemyComponent>(8);
        private int _currentEnemyIndex;

        // PROPERTIES

        public override TurnStateType StateType => TurnStateType.TurnEnemy;

        // CONSTRUCTOR

        public TurnEnemyState(TurnStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        // PUBLIC METHODS

        public override void Enter()
        {
            RegisterEvents();
            BuildEnemyList();

            _currentEnemyIndex = 0;

            GameLogger.Log(LogSourceType.TurnSystem,
                $"TurnEnemyState entered. EnemyCount={_enemies.Count}");

            if (_enemies.Count == 0)
            {
                CompleteEnemyPhase();
                return;
            }

            StartCurrentEnemy();
        }

        public override void Exit()
        {
            UnregisterEvents();

            GameLogger.Log(LogSourceType.TurnSystem, "TurnEnemyState exited.");
        }

        // PRIVATE METHODS

        private void RegisterEvents()
        {
            StateMachine.EventHub.EnemyEvents
                .EnemyTurnEndedEvent
                .Register(OnEnemyTurnEnded);
        }

        private void UnregisterEvents()
        {
            StateMachine.EventHub.EnemyEvents
                .EnemyTurnEndedEvent
                .Unregister(OnEnemyTurnEnded);
        }

        /// <summary>
        /// Builds the ordered enemy list from IFormationQuery.
        /// Iterates lines front-to-back, within each line left-to-right.
        /// Only alive, active enemies are included.
        /// Action points are reset for each enemy at this point.
        /// </summary>
        private void BuildEnemyList()
        {
            _enemies.Clear();

            foreach (BattleLine line in StateMachine.Formation.Lines)
            {
                foreach (EnemyComponent enemy in line.Enemies)
                {
                    if (enemy == null || !enemy.IsAlive)
                        continue;

                    if (!enemy.gameObject.activeInHierarchy)
                        continue;

                    enemy.ResetActionPoints();
                    _enemies.Add(enemy);
                }
            }

            GameLogger.Log(LogSourceType.TurnSystem,
                $"TurnEnemyState.BuildEnemyList: {_enemies.Count} enemies queued.");
        }

        private void StartCurrentEnemy()
        {
            EnemyComponent enemy = _enemies[_currentEnemyIndex];

            GameLogger.Log(LogSourceType.TurnSystem,
                $"TurnEnemyState: Starting enemy turn EntityId={enemy.EntityId}");

            StateMachine.EventHub.TurnEvents
                .TurnEnemyStartedEvent
                .Raise(new TurnEnemyStartedPayload(enemy.EntityId));
        }

        private void CompleteEnemyPhase()
        {
            GameLogger.Log(LogSourceType.TurnSystem,
                "TurnEnemyState: All enemies completed their turns.");

            StateMachine.SwitchToTurnEndState();
        }

        // EVENT HANDLERS

        /// <summary>
        /// Called when the active enemy signals its turn is complete.
        /// Advances to the next enemy or completes the phase if all are done.
        /// Stray events from non-active enemies are ignored.
        /// </summary>
        private void OnEnemyTurnEnded(EnemyTurnEndedPayload payload)
        {
            if (_currentEnemyIndex < 0 || _currentEnemyIndex >= _enemies.Count)
                return;

            EnemyComponent currentEnemy = _enemies[_currentEnemyIndex];

            if (payload.EnemyId != currentEnemy.EntityId)
            {
                GameLogger.Log(LogSourceType.TurnSystem,
                    $"TurnEnemyState: Ignored EnemyTurnEnded EntityId={payload.EnemyId}. Expected={currentEnemy.EntityId}");
                return;
            }

            _currentEnemyIndex++;

            GameLogger.Log(LogSourceType.TurnSystem,
                $"TurnEnemyState: EnemyTurnEnded EntityId={payload.EnemyId}. NextIndex={_currentEnemyIndex}");

            if (_currentEnemyIndex >= _enemies.Count)
            {
                CompleteEnemyPhase();
                return;
            }

            StartCurrentEnemy();
        }
    }
}