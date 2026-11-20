using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Puzzle.Grid;
using MightyHeroes.Gameplay.Ultimate.Payloads;

namespace MightyHeroes.Gameplay.Ultimate
{
    /// <summary>
    /// Makes the ultimate's affinity tiles glow on the puzzle grid while the
    /// hero's gauge is full, so the player sees which tiles fire the ultimate.
    ///
    /// Listens to UltimateGaugeChangedEvent for the hero: a full gauge
    /// highlights the affinity tile type, any other value clears it (the gauge
    /// empties when the ultimate fires). The puzzle only knows "highlight this
    /// tile type"; it does not know about ultimates.
    ///
    /// Constructed and disposed by BattleBootstrapper, scoped to one battle.
    /// </summary>
    public sealed class UltimateTileHighlightPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly GameGrid _grid;
        private readonly int _heroEntityId;
        private readonly ActionType _affinityTileType;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter, applies the current gauge state and subscribes to gauge changes.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="grid">Puzzle grid whose tiles are highlighted.</param>
        /// <param name="heroEntityId">Hero runtime id; events of other entities are ignored.</param>
        /// <param name="gauge">Hero ultimate gauge; provides the affinity tile type and the initial state.</param>
        public UltimateTileHighlightPresenter(EventHub eventHub, GameGrid grid, int heroEntityId, UltimateGauge gauge)
        {
            _eventHub = eventHub;
            _grid = grid;
            _heroEntityId = heroEntityId;
            _affinityTileType = gauge.Data.AffinityActionType;

            ApplyHighlight(gauge.IsFull);

            _eventHub.HeroEvents.UltimateGaugeChangedEvent.Register(OnUltimateGaugeChanged);
        }

        // PUBLIC METHODS

        /// <summary>Unregisters event listeners and clears the highlight. Must be called when the battle ends.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.HeroEvents.UltimateGaugeChangedEvent.Unregister(OnUltimateGaugeChanged);
            ApplyHighlight(false);
            _isDisposed = true;
        }

        // PRIVATE METHODS

        private void ApplyHighlight(bool isHighlighted)
        {
            if (_grid != null)
                _grid.SetHighlightedTileType(isHighlighted ? _affinityTileType : ActionType.None);
        }

        // EVENT HANDLERS

        private void OnUltimateGaugeChanged(UltimateGaugeChangedPayload payload)
        {
            if (payload.EntityId != _heroEntityId)
                return;

            ApplyHighlight(payload.IsFull);
        }
    }
}
