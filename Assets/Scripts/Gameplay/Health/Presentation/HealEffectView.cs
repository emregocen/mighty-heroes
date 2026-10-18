using MightyHeroes.Core.EventSystem;
using MightyHeroes.Gameplay.Health.Payloads;
using UnityEngine;

namespace MightyHeroes.Gameplay.Health.Presentation
{
    /// <summary>
    /// Spawns a heal effect prefab once under a spawn anchor and replays it whenever a heal
    /// effect request targets the owning entity. The source prefab is never modified; playback
    /// settings that would break replaying are overridden on the spawned instance at runtime.
    /// </summary>
    public sealed class HealEffectView : MonoBehaviour
    {
        // FIELDS

        [Header("Effect")]
        [SerializeField] private GameObject _effectPrefab;
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField] private Vector3 _localPosition = Vector3.zero;

        private EventHub _eventHub;
        private GameObject _effectInstance;
        private ParticleSystem[] _particleSystems;
        private int _entityId;

        // UNITY CALLBACKS

        private void Awake()
        {
            SpawnEffectInstance();
        }

        private void OnDestroy()
        {
            if (_eventHub == null)
                return;

            _eventHub.HealthEvents.HealEffectRequestedEvent.Unregister(OnHealEffectRequested);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Subscribes to heal effect requests for the given entity.
        /// </summary>
        /// <param name="eventHub">Event hub used for gameplay event subscriptions.</param>
        /// <param name="entityId">Identifier of the entity that owns this view.</param>
        public void Initialize(EventHub eventHub, int entityId)
        {
            if (eventHub == null)
                return;

            _eventHub = eventHub;
            _entityId = entityId;

            _eventHub.HealthEvents.HealEffectRequestedEvent.Register(OnHealEffectRequested);
        }

        // PRIVATE METHODS

        /// <summary>
        /// Instantiates the effect prefab under the spawn anchor and prepares it for replaying.
        /// The instance is stopped and deactivated until the first request arrives.
        /// </summary>
        private void SpawnEffectInstance()
        {
            if (_effectPrefab == null)
                return;

            Transform parent = _spawnAnchor != null ? _spawnAnchor : transform;

            _effectInstance = Instantiate(_effectPrefab, parent);
            _effectInstance.transform.localPosition = _localPosition;
            _effectInstance.transform.localRotation = Quaternion.identity;

            _particleSystems = _effectInstance.GetComponentsInChildren<ParticleSystem>(true);

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem system = _particleSystems[i];
                ParticleSystem.MainModule main = system.main;

                main.playOnAwake = false;
                main.loop = false;
                main.stopAction = ParticleSystemStopAction.None;

                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            _effectInstance.SetActive(false);
        }

        // EVENT HANDLERS

        /// <summary>
        /// Restarts every particle system of the spawned effect when the request targets this entity.
        /// </summary>
        /// <param name="payload">Heal effect request payload.</param>
        private void OnHealEffectRequested(HealEffectRequestedPayload payload)
        {
            if (payload.TargetEntityId != _entityId || _effectInstance == null)
                return;

            if (!_effectInstance.activeSelf)
                _effectInstance.SetActive(true);

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem system = _particleSystems[i];
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(false);
            }
        }
    }
}