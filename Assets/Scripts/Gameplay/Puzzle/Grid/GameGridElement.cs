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
        // CONSTANTS

        /// <summary>Icon shadow tint (UI Shadow "Effect Color" equivalent).</summary>
        private static readonly Color IconShadowEffectColor = new Color(0f, 0f, 0f, 0.6f);

        /// <summary>Icon shadow offset in tile units (UI Shadow "Effect Distance" equivalent).</summary>
        private static readonly Vector2 IconShadowEffectDistance = new Vector2(0.02f, -0.025f);

        // FIELDS

        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Renderer for the action icon. Must sort above the body renderer.")]
        [SerializeField] private SpriteRenderer _iconRenderer;

        [Tooltip("Renderer for the icon shadow. Uses the icon sprite; must sort between the body and the icon.")]
        [SerializeField] private SpriteRenderer _iconShadowRenderer;

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
        /// Applies a tile type: body tint, action type, icon and optional icon shadow.
        /// The shadow reuses the icon sprite with a constant color and offset, so it
        /// works with any icon. An empty icon hides both renderers.
        /// </summary>
        /// <param name="elementType">Tile type definition from GameGrid.</param>
        public void ApplyType(GameGridElementType elementType)
        {
            Color = elementType.Color;
            TileType = elementType.TileType;

            ApplyOverlay(_iconRenderer, elementType.Icon, elementType.IconColor);

            Sprite shadowSprite = elementType.ShowIconShadow ? elementType.Icon : null;
            ApplyOverlay(_iconShadowRenderer, shadowSprite, IconShadowEffectColor);
            PlaceIconShadow();
        }

        /// <summary>
        /// Replaces the tile body sprite (used for random body variants).
        /// A null sprite keeps the current one.
        /// </summary>
        /// <param name="bodySprite">Grayscale body sprite.</param>
        public void SetBodySprite(Sprite bodySprite)
        {
            if (bodySprite != null)
                spriteRenderer.sprite = bodySprite;
        }

        /// <summary>Positions the shadow at the icon position plus the constant effect distance.</summary>
        private void PlaceIconShadow()
        {
            if (_iconShadowRenderer == null || _iconRenderer == null)
                return;

            Vector3 iconPosition = _iconRenderer.transform.localPosition;
            _iconShadowRenderer.transform.localPosition = new Vector3(iconPosition.x + IconShadowEffectDistance.x, iconPosition.y + IconShadowEffectDistance.y, iconPosition.z);
        }

        /// <summary>Sets sprite and tint on an optional overlay renderer, hiding it when no sprite is given.</summary>
        private static void ApplyOverlay(SpriteRenderer overlayRenderer, Sprite sprite, Color color)
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.sprite = sprite;
            overlayRenderer.color = color;
            overlayRenderer.enabled = sprite != null;
        }

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