using System.Collections;
using System.Collections.Generic;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Puzzle.Core;
using MightyHeroes.Gameplay.Puzzle.Extensions;
using MightyHeroes.Gameplay.Puzzle.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     Despawns and respawns elements in the game grid
    /// </summary>
    public class GameGridSpawning
    {
        private readonly GameGrid grid;

        public GameGridSpawning(GameGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        ///     Respawns all despawned elements
        /// </summary>
        /// <returns></returns>
        public IEnumerator RespawnElements()
        {
            //GameSFX.Instance.Play(GameSFX.Instance.SpawnClip);
            /* düzeltme
            foreach (GameGridElement element in this.grid.Elements)
            {
                if (element.IsSpawned == false)
                {
                    GameGridElementType elementType = this.grid.ElementTypes.GetRandom();
                    element.Color = elementType.Color;
                    element.ElementType = elementType.ElementType;

                    element.Spawn();
                }
            }

            yield return new WaitForSeconds(0.4f);
            */
            for (var row = 0; row < grid.RowCount; row++)
                for (var col = 0; col < grid.ColumnCount; col++)
                {
                    var idx = grid.GridPositionToIndex(new Vector2Int(col, row));
                    var element = grid.Elements[idx];

                    if (!element.IsSpawned)
                    {
                        // pozisyonu kesin olarak set et
                        element.transform.position = grid.GridToWorldPosition(col, row);

                        var elementType = grid.ElementTypes.GetRandom();
                        element.Color = elementType.Color;
                        element.TileType = elementType.TileType;

                        element.Spawn();
                    }
                }

            yield return new WaitForSeconds(0.4f);
        }

        /// <summary>
        ///     Despawns the given list of elements
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public IEnumerator Despawn(List<GameGridElement> elements)
        {
            //GameSFX.Instance.Play(GameSFX.Instance.DespawnClip);
            GameLogger.Log(LogSourceType.PuzzleSystem, $"TilesMatched event invoked via EventHub: Type={elements[0].TileType}, Count={elements.Count}");


            foreach (var gridElement in elements) gridElement.Despawn(grid.despawnEffects[0], grid);

            yield return new WaitForSeconds(0.4f);

            PuzzleController.Instance.MovesAvailable--;

            GameEvents.OnElementsDespawned.Invoke(elements[0].TileType, elements.Count);

            if (PuzzleController.Instance.EventHub is null) GameLogger.Error(LogSourceType.PuzzleSystem, "PuzzleController EventHub is null");

            // Event payload'ını hazırla
            var payload = new PuzzleTilesMatchedPayload(
                elements[0].TileType,
                elements.Count // match count
            );

            // EventHub üzerinden event yayınla
            PuzzleController.Instance.EventHub.PuzzleEvents.PuzzleTilesMatchedEvent.Raise(payload);

        }
    }
}