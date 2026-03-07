using System.Collections;
using MightyHeroes.Gameplay.Common.Actions;
using MightyHeroes.Gameplay.Puzzle.Extensions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Puzzle.Grid
{
    /// <summary>
    ///     An elements  in the grid
    /// </summary>
    public class GameGridElement : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float CellSize = 1f;

        /// <summary>
        ///     Gets whether the element is moving or not
        /// </summary>
        public bool IsMoving { get; private set; }

        /// <summary>
        ///     Returns whether the element is active or not
        /// </summary>
        public bool IsSpawned => gameObject.activeSelf;

        /// <summary>
        ///     Gets or sets the color of the element
        /// </summary>
        public Color Color
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = value;
        }

        public ActionType TileType { get; set; }

        /// <summary>
        ///     Moves the elements to the given target position (world coordinates)
        ///     in the given time
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="time"></param>
        public void Move(Vector3 targetPos, float time)
        {
            IsMoving = true;

            StartCoroutine(transform.MoveCoroutine(targetPos, time, () => { IsMoving = false; }));
        }

        /// <summary>
        ///     Activates the element and plays its spawn animation
        /// </summary>
        public void Spawn()
        {
            gameObject.SetActive(true);

            var startScale = Vector2.one * 0.1f;
            var endScale = Vector2.one * CellSize;

            StartCoroutine(transform.ScaleCoroutine(startScale, endScale, 0.25f));
        }

        /// <summary>
        ///     Plays the despawn animation and deactivates the element afterwards
        /// </summary>
        public void Despawn(GameObject despawnEffect, GameGrid grid)
        {
            var startScale = Vector2.one * CellSize;
            var endScale = Vector2.one * 0.1f;

            /*
            StartCoroutine(this.transform.ScaleCoroutine(startScale, endScale, 0.25f, () =>
            {
                this.gameObject.SetActive(false);
            }));
            */
            if (despawnEffect != null)
            {
                // 1. Elemanın dünya pozisyonunu al
                var worldPos = transform.position;
                worldPos += grid.PuzzleCamera.transform.forward * 0.1f;
                // 2. Dünya pozisyonunu puzzle kameraya göre ekran koordinatına çevir
                var screenPos = grid.PuzzleCamera.WorldToScreenPoint(worldPos);

                // 3. Sonra tekrar puzzle kameradan world koordinatına çevir
                var cameraWorldPos = grid.PuzzleCamera.ScreenToWorldPoint(screenPos);

                // 4. Efekti bu pozisyonda oluştur
                var effect = Instantiate(despawnEffect, cameraWorldPos, Quaternion.identity);

                // 5. Layer eşitle
                SetLayerRecursively(effect, gameObject.layer);

                // 4. Eğer child objeler varsa hepsini recursive olarak layer’a geçir
                SetLayerRecursively(effect, gameObject.layer);

                // 5. Particle sistemin kendi süresi dolunca yok olması için güvence
                var ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                    Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                else
                    Destroy(effect, 2f); // fallback
            }

            transform.DOScaleCustom(startScale, endScale, 0.25f, () => { gameObject.SetActive(false); });

            /*
            transform.DOKill();
            transform.DOScale(1.2f, 0.25f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // Büyüme bitince eski haline dön
                    transform.DOScale(0.1f, 0.25f)
                        .SetEase(Ease.InBack)
                        .OnComplete(()=> this.gameObject.SetActive(false));
                });
            */
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
        }

        public void SetCellSize(float cellSize)
        {
            CellSize = cellSize;
        }

        private IEnumerator PlayDespawnEffect(GameGrid grid, GameObject despawnEffect)
        {
            // 1. Elemanın dünya pozisyonunu al
            var worldPos = transform.position;
            worldPos += grid.PuzzleCamera.transform.forward * 0.1f;
            // 2. Dünya pozisyonunu puzzle kameraya göre ekran koordinatına çevir
            var screenPos = grid.PuzzleCamera.WorldToScreenPoint(worldPos);

            // 3. Sonra tekrar puzzle kameradan world koordinatına çevir
            var cameraWorldPos = grid.PuzzleCamera.ScreenToWorldPoint(screenPos);

            // 4. Efekti bu pozisyonda oluştur
            var effect = Instantiate(despawnEffect, cameraWorldPos, Quaternion.identity);

            // 5. Layer eşitle
            SetLayerRecursively(effect, gameObject.layer);

            // 4. Eğer child objeler varsa hepsini recursive olarak layer’a geçir
            SetLayerRecursively(effect, gameObject.layer);

            // 5. Particle sistemin kendi süresi dolunca yok olması için güvence
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(effect, 2f); // fallback
            yield return new WaitForSeconds(0.25f);
        }
    }
}