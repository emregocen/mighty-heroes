using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Battle.Formation;
using MightyHeroes.Gameplay.Battle.TurnFlow.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Targeting
{
    /// <summary>
    /// Keeps the target marker on the enemy the hero currently targets.
    /// The marker stays hidden during the intro, appears with the first turn,
    /// follows every target change and is removed when the battle ends.
    /// </summary>
    public sealed class TargetMarkerPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly BattleFormation _battleFormation;
        private readonly TargetMarkerView _view;

        private bool _isBattleActive;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter, hides the marker and subscribes to target and turn events.
        /// </summary>
        /// <param name="eventHub">Battle event hub.</param>
        /// <param name="battleFormation">Formation that owns the current target.</param>
        /// <param name="view">Marker view to drive.</param>
        public TargetMarkerPresenter(EventHub eventHub, BattleFormation battleFormation, TargetMarkerView view)
        {
            _eventHub = eventHub;
            _battleFormation = battleFormation;
            _view = view;

            _view.Hide();

            _battleFormation.CurrentTargetFocusChanged += OnCurrentTargetFocusChanged;
            _eventHub.TurnEvents.TurnStartedEvent.Register(OnTurnStarted);
            _eventHub.TurnEvents.TurnBattleVictoryEvent.Register(OnBattleVictory);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Register(OnBattleDefeated);
        }

        // PUBLIC METHODS

        /// <summary>Unsubscribes from all events.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _battleFormation.CurrentTargetFocusChanged -= OnCurrentTargetFocusChanged;
            _eventHub.TurnEvents.TurnStartedEvent.Unregister(OnTurnStarted);
            _eventHub.TurnEvents.TurnBattleVictoryEvent.Unregister(OnBattleVictory);
            _eventHub.TurnEvents.TurnBattleDefeatedEvent.Unregister(OnBattleDefeated);

            _isDisposed = true;
        }

        // PRIVATE METHODS

        /// <summary>Shows the marker on the given focus point, or hides it when there is none.</summary>
        private void ApplyTarget(Transform targetFocus)
        {
            if (_view == null)
                return;

            if (!_isBattleActive || targetFocus == null)
            {
                _view.Hide();
                return;
            }

            _view.Show(targetFocus);
        }

        /// <summary>Stops presenting and hides the marker for the rest of the battle.</summary>
        private void EndPresentation()
        {
            _isBattleActive = false;

            if (_view != null)
                _view.Hide();
        }

        // EVENT HANDLERS

        private void OnCurrentTargetFocusChanged(Transform targetFocus)
        {
            ApplyTarget(targetFocus);
        }

        private void OnTurnStarted(TurnStartedPayload payload)
        {
            if (_isBattleActive || !payload.IsFirstTurn)
                return;

            _isBattleActive = true;
            ApplyTarget(_battleFormation.CurrentTargetFocus);
        }

        private void OnBattleVictory(TurnBattleVictoryPayload payload)
        {
            EndPresentation();
        }

        private void OnBattleDefeated(TurnBattleDefeatedPayload payload)
        {
            EndPresentation();
        }
    }
}
