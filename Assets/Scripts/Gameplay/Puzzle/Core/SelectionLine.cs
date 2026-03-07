using System.Collections.Generic;
using System.Linq;
using MightyHeroes.Gameplay.Puzzle.Grid;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Core
{
    /// <summary>
    ///     Behaviour rendering the connection line between selected elements
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SelectionLine : MonoBehaviour
    {
        private Color color;
        private LineRenderer lineRenderer;

        /// <summary>
        ///     Gets or sets the color of the line
        /// </summary>
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                lineRenderer.startColor = value;
                lineRenderer.endColor = value;
            }
        }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            // düzeltme
            lineRenderer.useWorldSpace = true;
            //lineRenderer.sortingLayerName = "Default"; // sprite'ların layer'ı neyse onu ver
            lineRenderer.sortingOrder = 50; // sprite'ların üstünde çizsin
        }

        /// <summary>
        ///     Updates the line for the given list of selected elements
        /// </summary>
        /// <param name="selectedElements"></param>
        /*
        public void SetPositions(List<GameGridElement> selectedElements)
        {
            this.lineRenderer.positionCount = selectedElements.Count;

            for (int i = 0; i < selectedElements.Count; i++)
            {
                // düzeltme
                Vector3 pos = selectedElements[i].transform.position;
                pos.z = -0.1f; // veya sprite'ların z'sine göre küçük bir offset
                lineRenderer.SetPosition(i, pos);

                //this.lineRenderer.SetPosition(i, selectedElements[i].transform.position);
            }
        }
        */
        public void SetPositions(List<GameGridElement> selectedElements)
        {
            if (selectedElements == null || selectedElements.Count == 0)
            {
                Clear();
                return;
            }

            var active = selectedElements.Where(e => e != null /*&& e.IsSpawned*/).ToList();
            if (active.Count == 0)
            {
                Clear();
                return;
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = active.Count;

            for (var i = 0; i < active.Count; i++)
            {
                var pos = active[i].transform.position;
                pos.z = 0f; // çizgiyi kameranın önüne al
                lineRenderer.SetPosition(i, pos);
            }
        }

        /// <summary>
        ///     Clears the line
        /// </summary>
        public void Clear()
        {
            lineRenderer.positionCount = 0;
        }
    }
}