using MightyHeroes.Battle.TurnFlow.Payloads;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.UI.Battle.Views;
using UnityEngine;

namespace MightyHeroes.UI.Battle.Presenters
{
    /// <summary>
    /// Central presenter responsible for listening to battle-related events
    /// and updating UI views accordingly.
    /// This class does not contain gameplay logic.
    /// </summary>
    public sealed class BattleUIPresenter : MonoBehaviour
    {
        // 1) SERIALIZED FIELDS

        [SerializeField] private TurnPhaseBannerView _turnBannerView;

        // 2) PRIVATE FIELDS

        private EventHub _eventHub;

        // 3) INITIALIZATION

        /// <summary>
        /// Initializes presenter and registers to required event channels.
        /// </summary>
        public void Initialize(EventHub eventHub)
        {
            _eventHub = eventHub;

            RegisterEvents();
        }

        /// <summary>
        /// Registers to turn-related events.
        /// </summary>
        private void RegisterEvents()
        {
            _eventHub.TurnEvents.TurnHeroPhaseStartedEvent.Register(OnHeroTurnStarted);
            _eventHub.TurnEvents.TurnEnemyPhaseStartedEvent.Register(OnEnemyTurnStarted);
        }

        /// <summary>
        /// Unregisters from event channels to prevent memory leaks.
        /// </summary>
        private void UnregisterEvents()
        {
            if (_eventHub == null)
                return;

            _eventHub.TurnEvents.TurnHeroPhaseStartedEvent.Unregister(OnHeroTurnStarted);
            _eventHub.TurnEvents.TurnEnemyPhaseStartedEvent.Unregister(OnEnemyTurnStarted);
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        // 4) EVENT HANDLERS

        /// <summary>
        /// Called when hero turn phase starts.
        /// </summary>
        private void OnHeroTurnStarted(TurnHeroPhaseStartedPayload payload)
        {
            if (_turnBannerView == null)
                return;

            _turnBannerView.PlayHeroTurn();
        }

        /// <summary>
        /// Called when enemy turn phase starts.
        /// </summary>
        private void OnEnemyTurnStarted(TurnEnemyPhaseStartedPayload payload)
        {
            if (_turnBannerView == null)
                return;

            _turnBannerView.PlayEnemyTurn();
        }
    }
}