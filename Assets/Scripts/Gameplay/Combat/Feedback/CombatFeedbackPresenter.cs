using System;
using System.Collections.Generic;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Interfaces;
using MightyHeroes.Gameplay.Combat.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Turns resolved combat hits into presentation feedback: camera shake and
    /// hit VFX (hit-stop follows in the next step).
    ///
    /// Listens to CombatHitResolvedEvent, which CombatSystem raises once per
    /// hit after damage is applied. For hero hits the payload's TotalSteps is
    /// the action tier, so strength scales with match size:
    /// - final combo hit uses the tier's shake force and VFX,
    /// - earlier combo hits use a reduced shake and the VFX of the tier below,
    /// - a killing hit multiplies the shake by KillingHitShakeMultiplier.
    /// Enemy hits on the hero use a single fixed shake and VFX.
    ///
    /// Shaker and VFX player are both optional; a null dependency simply
    /// disables that part of the feedback.
    ///
    /// Target positions are captured on CombatDamageIntentEvent, before damage
    /// is applied. A killing hit removes the enemy from the formation before
    /// CombatHitResolvedEvent fires, so the captured position is the fallback
    /// that keeps VFX on killing hits.
    ///
    /// Constructed and disposed by BattleBootstrapper, scoped to one battle.
    /// </summary>
    public sealed class CombatFeedbackPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly CombatFeedbackData _data;
        private readonly ICameraShaker _cameraShaker;
        private readonly IHitVfxPlayer _hitVfxPlayer;
        private readonly ICombatEntityLocator _entityLocator;
        private readonly int _heroEntityId;
        private readonly Dictionary<int, Vector3> _lastTargetPositions = new Dictionary<int, Vector3>();
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter and subscribes to resolved hit events.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="data">Feedback tuning values.</param>
        /// <param name="cameraShaker">Camera shake implementation, or null to disable shake.</param>
        /// <param name="hitVfxPlayer">Hit VFX implementation, or null to disable hit VFX.</param>
        /// <param name="entityLocator">Resolves target positions for hit VFX.</param>
        /// <param name="heroEntityId">Hero runtime id, used to tell hero hits from enemy hits.</param>
        public CombatFeedbackPresenter(EventHub eventHub, CombatFeedbackData data, ICameraShaker cameraShaker, IHitVfxPlayer hitVfxPlayer, ICombatEntityLocator entityLocator, int heroEntityId)
        {
            _eventHub = eventHub;
            _data = data;
            _cameraShaker = cameraShaker;
            _hitVfxPlayer = hitVfxPlayer;
            _entityLocator = entityLocator;
            _heroEntityId = heroEntityId;

            _eventHub.CombatEvents.CombatDamageIntentEvent.Register(OnCombatDamageIntent);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Register(OnCombatHitResolved);
        }

        // PUBLIC METHODS

        /// <summary>Unregisters event listeners. Must be called when the battle ends.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.CombatEvents.CombatDamageIntentEvent.Unregister(OnCombatDamageIntent);
            _eventHub.CombatEvents.CombatHitResolvedEvent.Unregister(OnCombatHitResolved);
            _lastTargetPositions.Clear();
            _isDisposed = true;
        }

        // PRIVATE METHODS

        /// <summary>Plays camera shake for the hit, if a shaker is assigned.</summary>
        private void PlayShake(CombatHitResolvedPayload payload, bool isHeroHit, bool isFinalHit)
        {
            if (_cameraShaker == null)
                return;

            if (!isHeroHit)
            {
                _cameraShaker.Shake(_data.EnemyHitShakeForce);
                return;
            }

            float force = GetTierValue(_data.HeroHitShakeForces, payload.TotalSteps);

            if (!isFinalHit)
                force *= _data.NonFinalHitShakeMultiplier;

            if (payload.TargetWasAlive && !payload.TargetIsAliveAfter)
                force *= _data.KillingHitShakeMultiplier;

            _cameraShaker.Shake(force);
        }

        /// <summary>
        /// Plays the hit VFX set at the target's chest height, pulled toward the
        /// camera so it is not hidden inside the body. Final hero hits use their
        /// tier's set, earlier combo hits the set of the tier below.
        /// </summary>
        private void PlayHitVfx(CombatHitResolvedPayload payload, bool isHeroHit, bool isFinalHit)
        {
            if (_hitVfxPlayer == null || _entityLocator == null)
                return;

            if (!_entityLocator.TryGetPosition(payload.TargetId, out Vector3 targetPosition) && !_lastTargetPositions.TryGetValue(payload.TargetId, out targetPosition))
                return;

            CombatFeedbackData.HitVfxSet vfxSet;

            if (isHeroHit)
            {
                int tier = isFinalHit ? payload.TotalSteps : payload.TotalSteps - 1;
                vfxSet = GetTierValue(_data.HeroHitVfxTiers, tier);
            }
            else
            {
                vfxSet = _data.EnemyHitVfx;
            }

            if (vfxSet == null || vfxSet.Prefabs == null)
                return;

            Vector3 spawnPosition = targetPosition + Vector3.up * _data.HitVfxHeightOffset;

            foreach (GameObject prefab in vfxSet.Prefabs)
                _hitVfxPlayer.Play(prefab, spawnPosition, _data.HitVfxTowardCameraOffset, vfxSet.Scale, _data.HitVfxLifetime);
        }

        /// <summary>Returns the per-tier value (index tier - 1), clamped to the array bounds.</summary>
        private T GetTierValue<T>(T[] values, int tier)
        {
            if (values == null || values.Length == 0)
                return default;

            int index = Math.Clamp(tier - 1, 0, values.Length - 1);
            return values[index];
        }

        // EVENT HANDLERS

        /// <summary>
        /// Captures the target position while the target is still in the formation.
        /// </summary>
        private void OnCombatDamageIntent(CombatDamageIntentPayload payload)
        {
            if (_entityLocator != null && _entityLocator.TryGetPosition(payload.TargetEntityId, out Vector3 position))
                _lastTargetPositions[payload.TargetEntityId] = position;
        }

        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            if (!payload.DamageApplied)
                return;

            bool isHeroHit = payload.AttackerId == _heroEntityId;
            bool isFinalHit = payload.StepIndex >= payload.TotalSteps;

            PlayShake(payload, isHeroHit, isFinalHit);
            PlayHitVfx(payload, isHeroHit, isFinalHit);
        }
    }
}
