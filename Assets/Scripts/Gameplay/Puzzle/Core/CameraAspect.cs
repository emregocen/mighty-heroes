using System;
using MightyHeroes.Gameplay.Puzzle.Extensions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Core
{
    /// <summary>
    ///     Adjusts the attached Camera to have a specified minimum width
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraAspect : MonoBehaviour
    {
        [SerializeField] private float minWidth = 6.75f;

        private Vector2 screenDimensions = Vector2.zero;

        /// <summary>
        ///     Reference to the camera
        /// </summary>
        private Camera Camera { get; set; }

        private void Start()
        {
            UpdateCameraAspect();
        }

        private void Update()
        {
            //Detect and handle screen size changes
            if (screenDimensions.x != Screen.width || screenDimensions.y != Screen.height) UpdateCameraAspect();
        }

        private void ApplyAspectRatio()
        {
            //Reset Camera aspect
            Camera = GetComponent<Camera>();
            Camera.rect = new Rect(0, 0, 1, 1);
            Camera.ResetAspect();

            //Get the current values of the camera
            var cameraWidth = Camera.GetWidth();

            //Is the available width smaller than expected? Apply a letterbox
            if (cameraWidth < minWidth) ApplyLetterbox(cameraWidth / minWidth);
        }

        /// <summary>
        ///     Sets how many percent of the screen height should be used by the camera
        /// </summary>
        /// <param name="ratio"></param>
        private void ApplyLetterbox(float ratio)
        {
            var rect = new Rect(0, 0, 1, 1);

            //Apply ratio to the height
            rect.height *= ratio;

            rect.y = (1 - rect.height) / 2;

            //Round values to 4 digits after dot
            rect.y = (float)Math.Round(rect.y, 5);
            rect.height = (float)Math.Round(rect.height, 5);

            Camera.rect = rect;
        }

        private void UpdateCameraAspect()
        {
            screenDimensions = new Vector2
            {
                x = Screen.width,
                y = Screen.height
            };

            ApplyAspectRatio();
        }
    }
}