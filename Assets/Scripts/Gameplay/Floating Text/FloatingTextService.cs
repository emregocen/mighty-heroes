using MightyHeroes.Gameplay.FloatingText.Types;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Central entry point for spawning floating combat text.
    ///
    /// Owns two pools, both on the screen-space HUD canvas:
    /// - anchored views (damage, heal, shield, buff, K.O., overkill) that follow
    ///   an entity's world position on screen;
    /// - fixed screen views (combo) at a fixed HUD anchor.
    /// Exposes a small, semantic API so callers (FloatingTextPresenter) never
    /// touch pools or views directly.
    ///
    /// Anchored spawns are stacked per entity via FloatingTextStackTracker
    /// so simultaneous or rapid-fire texts (e.g. damage immediately followed
    /// by K.O. and overkill on the same death) stack vertically rather than
    /// overlapping.
    ///
    /// Constructed and owned by BattleBootstrapper, scoped to a single battle.
    /// </summary>
    public sealed class FloatingTextService
    {
        // FIELDS

        private readonly FloatingTextConfig _config;
        private readonly FloatingTextPool<FloatingTextAnchoredView> _anchoredPool;
        private readonly FloatingTextPool<FloatingTextScreenView> _screenPool;
        private readonly FloatingTextStackTracker _stackTracker;

        // CONSTRUCTOR

        /// <summary>
        /// Creates both pools under the given screen-space canvas parent.
        /// </summary>
        /// <param name="config">Per-type settings, pool size and stack settings.</param>
        /// <param name="anchoredPrefab">Prefab for world-anchored texts.</param>
        /// <param name="screenPrefab">Prefab for fixed screen texts (combo).</param>
        /// <param name="screenParent">Full-screen RectTransform on the HUD canvas.</param>
        public FloatingTextService(FloatingTextConfig config, FloatingTextAnchoredView anchoredPrefab, FloatingTextScreenView screenPrefab, Transform screenParent)
        {
            _config = config;

            _anchoredPool = new FloatingTextPool<FloatingTextAnchoredView>(anchoredPrefab, screenParent, config.PoolSizePerPrefab);
            _screenPool = new FloatingTextPool<FloatingTextScreenView>(screenPrefab, screenParent, config.PoolSizePerPrefab);
            _stackTracker = new FloatingTextStackTracker(config.StackWindowSeconds, config.StackSpacing);
        }

        // PUBLIC METHODS

        /// <summary>Shows a damage number above worldPosition, stacked per entityId.</summary>
        public void ShowDamage(int entityId, Vector3 worldPosition, int amount)
        {
            PlayWorldStacked(entityId, FloatingTextType.Damage, worldPosition, amount.ToString());
        }

        /// <summary>
        /// Shows the damage absorbed by shield above worldPosition, stacked per entityId.
        /// Uses the Absorbed style (shield color) so it reads differently from health damage.
        /// </summary>
        public void ShowAbsorbed(int entityId, Vector3 worldPosition, int amount)
        {
            PlayWorldStacked(entityId, FloatingTextType.Absorbed, worldPosition, $"-{amount}");
        }

        /// <summary>Shows shield gained in battle ("+N", shield color) above worldPosition, stacked per entityId.</summary>
        public void ShowShieldGain(int entityId, Vector3 worldPosition, int amount)
        {
            PlayWorldStacked(entityId, FloatingTextType.ShieldGain, worldPosition, $"+{amount}");
        }

        /// <summary>Shows a granted attack buff ("ATK +N%") above worldPosition, stacked per entityId.</summary>
        public void ShowAttackBuff(int entityId, Vector3 worldPosition, int percent)
        {
            PlayWorldStacked(entityId, FloatingTextType.AttackBuff, worldPosition, $"ATK +{percent}%");
        }

        /// <summary>Shows a heal number above worldPosition, stacked per entityId.</summary>
        public void ShowHeal(int entityId, Vector3 worldPosition, int amount)
        {
            PlayWorldStacked(entityId, FloatingTextType.Heal, worldPosition, $"+{amount}");
        }

        /// <summary>Shows "K.O." above worldPosition, stacked per entityId.</summary>
        public void ShowKnockOut(int entityId, Vector3 worldPosition)
        {
            PlayWorldStacked(entityId, FloatingTextType.KnockOut, worldPosition, "K.O.");
        }

        /// <summary>Shows "OVERKILL" above worldPosition, stacked per entityId (typically lands just above K.O.).</summary>
        public void ShowOverkill(int entityId, Vector3 worldPosition)
        {
            PlayWorldStacked(entityId, FloatingTextType.Overkill, worldPosition, "OVERKILL");
        }

        /// <summary>Shows a combo callout ("COMBO x2", etc.) at the fixed screen-space combo anchor.</summary>
        public void ShowCombo(string text)
        {
            FloatingTextConfig.TypeSettings settings = _config.GetSettings(FloatingTextType.Combo);
            FloatingTextScreenView view = _screenPool.Acquire();

            view.Play(text, settings, OnScreenViewReturned);
        }

        // PRIVATE METHODS

        /// <summary>
        /// Spawns an anchored text at worldPosition with the per-entity stack
        /// offset (canvas pixels), so texts landing close together in time stack
        /// upward instead of overlapping.
        /// </summary>
        private void PlayWorldStacked(int entityId, FloatingTextType type, Vector3 worldPosition, string text)
        {
            float stackOffset = _stackTracker.RegisterSpawnAndGetOffset(entityId);
            FloatingTextConfig.TypeSettings settings = _config.GetSettings(type);
            FloatingTextAnchoredView view = _anchoredPool.Acquire();

            view.PlayAt(worldPosition, stackOffset, text, settings, OnAnchoredViewReturned);
        }

        private void OnAnchoredViewReturned(IFloatingTextHandle handle)
        {
            if (handle is FloatingTextAnchoredView view)
                _anchoredPool.Release(view);
        }

        private void OnScreenViewReturned(IFloatingTextHandle handle)
        {
            if (handle is FloatingTextScreenView view)
                _screenPool.Release(view);
        }
    }
}