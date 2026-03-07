using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MightyHeroes.Gameplay.Puzzle.Core;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     Processed the user input on the game grid
    /// </summary>
    public class GameGridInput
    {
        private readonly Camera camera;
        private readonly GameGrid grid;
        private readonly SelectionLine selectionLine;

        private Vector3 originalScale;

        public GameGridInput(GameGrid grid, SelectionLine selectionLine, Camera camera)
        {
            this.grid = grid;
            this.selectionLine = selectionLine;
            SelectedElements = new List<GameGridElement>();
            this.camera = camera;
        }

        public List<GameGridElement> SelectedElements { get; }

        /// <summary>
        ///     Clears the current selection
        /// </summary>
        private void Clear()
        {
            selectionLine.Clear();
            SelectedElements.Clear();

            GameEvents.OnSelectionChanged.Invoke(SelectedElements.Count);
        }

        /// <summary>
        ///     Processes the user input and waits until a selection has been made
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForSelection()
        {
            Clear();

            while (true)
            {
                yield return null;

                //Does the user touch the screen?
                if (Input.GetMouseButton(0))
                {
                    //Does the user touch an element?
                    if (InputRaycast(out var element)) ProcessGridElement(element);
                }
                //Selection finished?
                else if (SelectedElements.Count > 0)
                {
                    if (IsValidInput())
                    {
                        //Clear selection line when finished
                        selectionLine.Clear();

                        //Stop coroutine
                        yield break;
                    }

                    Clear();
                }
            }
        }

        /// <summary>
        ///     Returns true if an element is touched by the mouse, false otherwise.
        ///     Outs the touched element, null otherwise
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool InputRaycast(out GameGridElement element)
        {
            element = null;

            //Get mouse position
            Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

            //Shoot a raycast on z-level
            var hitInfo = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hitInfo)
            {
                //Try to get the gamegrid element
                element = hitInfo.transform.GetComponent<GameGridElement>();

                //Return true if the hit object is GameGridElement
                if (element != null) return true;
            }

            //Return false if nothing is hit or if the object is not GameGridElement
            return false;
        }

        /// <summary>
        ///     Processes the input on the given element
        /// </summary>
        /// <param name="element"></param>
        private void ProcessGridElement(GameGridElement element)
        {
            //No element selected yet? Select element!
            if (SelectedElements.Count == 0)
            {
                //Cache selected color
                selectionLine.Color = element.Color;
                //Add element to selection
                AddElementToSelection(element);
            }
            else
            {
                //Is the element already selected?
                if (SelectedElements.Contains(element))
                {
                    //Did the player moved back? Deselect element!
                    if (IsSecondLast(element)) DeselectLast();
                }
                //Not selected, correct color and in distance? Select element!
                else if (IsSelectable(element))
                {
                    AddElementToSelection(element);
                }
            }
        }

        /// <summary>
        ///     Returns true if the given element is the second last in the selection,
        ///     false otherwise. This is used to determine the deselection of selected elements.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsSecondLast(GameGridElement element)
        {
            return SelectedElements.Count >= 2 && element.Equals(SelectedElements[SelectedElements.Count - 2]);
        }

        /// <summary>
        ///     Returns true if the given element can be selected,
        ///     false otherwise
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsSelectable(GameGridElement element)
        {
            return HasValidColor(element) && IsInDistance(element);
        }

        /// <summary>
        ///     Returns true if the given element has the current selection color,
        ///     false otherwise
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool HasValidColor(GameGridElement element)
        {
            return element.Color == selectionLine.Color;
        }

        /// <summary>
        ///     Returns true if the given element is adjacent to the last element in the selection,
        ///     false otherwise
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsInDistance(GameGridElement element)
        {
            //Vector2 lastElementPos = this.SelectedElements.Last().transform.position;
            //return Vector2.Distance(element.transform.position, lastElementPos) < 1.5f * this.grid.CellSize;

            var lastIndex = grid.IndexOf(SelectedElements.Last());
            var newIndex = grid.IndexOf(element);

            return Mathf.Abs(lastIndex.x - newIndex.x) <= 1 && Mathf.Abs(lastIndex.y - newIndex.y) <= 1;
        }

        /// <summary>
        ///     Adds the given element to the selection
        /// </summary>
        /// <param name="selectedElements"></param>
        /// <param name="element"></param>
        private void AddElementToSelection(GameGridElement element)
        {
            // Grid elemente büyüme efekti uygula
            var sizeMultiplier = 1.2f;
            var duration = 0.5f;
            originalScale = element.transform.localScale;
            element.transform.DOKill();
            element.transform.DOScale(originalScale * sizeMultiplier, duration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // Büyüme bitince eski haline dön
                    element.transform.DOScale(originalScale, duration).SetEase(Ease.InBack);
                });

            if (SelectedElements.Count > 0)
            {
                var pitch = 1.0f + SelectedElements.Count * 0.1f;
                //GameSFX.Instance.Play(GameSFX.Instance.SelectionClip, pitch);
            }

            SelectedElements.Add(element);

            selectionLine.Color = element.Color;

            selectionLine.SetPositions(SelectedElements);

            GameEvents.OnSelectionChanged.Invoke(SelectedElements.Count);
        }

        /// <summary>
        ///     Removes the last element from the selection
        /// </summary>
        /// <param name="selectedElements"></param>
        private void DeselectLast()
        {
            SelectedElements.Remove(SelectedElements.Last());

            selectionLine.SetPositions(SelectedElements);

            GameEvents.OnSelectionChanged.Invoke(SelectedElements.Count);
        }

        /// <summary>
        ///     Returns true if more than one element is selected, false otherwise
        /// </summary>
        /// <returns></returns>
        private bool IsValidInput()
        {
            return SelectedElements.Count > 1 ? true : false;
        }
    }
}