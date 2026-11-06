using System;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Combat.Payloads;

namespace MightyHeroes.Gameplay.Combat.Feedback
{
    /// <summary>
    /// Turns resolved combat hits into presentation feedback (camera shake
    /// today; tier-scaled VFX and hit-stop in the following steps).
    ///
    /// Listens to CombatHitResolvedEvent, which CombatSystem raises once per
    /// hit after damage is applied. For hero hits the payload's TotalSteps is
    /// the action tier, so strength scales with match size:
    /// - final combo hit uses the tier force,
    /// - earlier combo hits use tier force * NonFinalHitShakeMultiplier,
    /// - a killing hit is multiplied by KillingHitShakeMultiplier.
    /// Enemy hits on the hero use a single fixed force.
    ///
    /// Constructed and disposed by BattleBootstrapper, scoped to one battle.
    /// </summary>
    public sealed class CombatFeedbackPresenter : IDisposable
    {
        // FIELDS

        private readonly EventHub _eventHub;
        private readonly CombatFeedbackData _data;
        private readonly ICameraShaker _cameraShaker;
        private readonly int _heroEntityId;
        private bool _isDisposed;

        // CONSTRUCTOR

        /// <summary>
        /// Creates the presenter and subscribes to resolved hit events.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        /// <param name="data">Feedback tuning values.</param>
        /// <param name="cameraShaker">Camera shake implementation.</param>
        /// <param name="heroEntityId">Hero runtime id, used to tell hero hits from enemy hits.</param>
        public CombatFeedbackPresenter(EventHub eventHub, CombatFeedbackData data, ICameraShaker cameraShaker, int heroEntityId)
        {
            _eventHub = eventHub;
            _data = data;
            _cameraShaker = cameraShaker;
            _heroEntityId = heroEntityId;

            _eventHub.CombatEvents.CombatHitResolvedEvent.Register(OnCombatHitResolved);
        }

        // PUBLIC METHODS

        /// <summary>Unregisters event listeners. Must be called when the battle ends.</summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _eventHub.CombatEvents.CombatHitResolvedEvent.Unregister(OnCombatHitResolved);
            _isDisposed = true;
        }

        // PRIVATE METHODS

        /// <summary>Returns the shake force for a resolved hit.</summary>
        private float ResolveShakeForce(CombatHitResolvedPayload payload)
        {
            if (payload.AttackerId != _heroEntityId)
                return _data.EnemyHitShakeForce;

            float force = GetTierValue(_data.HeroHitShakeForces, payload.TotalSteps);
            bool isFinalHit = payload.StepIndex >= payload.TotalSteps;

            if (!isFinalHit)
                force *= _data.NonFinalHitShakeMultiplier;

            if (payload.TargetWasAlive && !payload.TargetIsAliveAfter)
                force *= _data.KillingHitShakeMultiplier;

            return force;
        }

        /// <summary>Returns the per-tier value (index tier - 1), clamped to the array bounds.</summary>
        private float GetTierValue(float[] values, int tier)
        {
            if (values == null || values.Length == 0)
                return 0f;

            int index = Math.Clamp(tier - 1, 0, values.Length - 1);
            return values[index];
        }

        // EVENT HANDLERS

        private void OnCombatHitResolved(CombatHitResolvedPayload payload)
        {
            if (!payload.DamageApplied)
                return;

            _cameraShaker.Shake(ResolveShakeForce(payload));
        }
    }
}
