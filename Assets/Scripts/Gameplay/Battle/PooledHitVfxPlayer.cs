using System.Collections;
using System.Collections.Generic;
using MightyHeroes.Gameplay.Combat.Feedback;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// IHitVfxPlayer that keeps one pool per VFX prefab under this transform.
    ///
    /// Instances are created on first use and reused afterwards, so repeated
    /// hits do not allocate. Every particle system in a new instance is switched
    /// to Hierarchy scaling so the uniform scale from CombatFeedbackData affects
    /// the whole effect (Synty prefabs ship with Local scaling).
    ///
    /// Each effect is pulled toward the main camera by the requested offset (so it
    /// is not hidden inside the target's mesh) and faces the camera, which keeps
    /// flat slash effects readable.
    /// Looping effects are stopped and cleared when their lifetime ends.
    /// </summary>
    public sealed class PooledHitVfxPlayer : MonoBehaviour, IHitVfxPlayer
    {
        // FIELDS

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, ParticleSystem[]> _particleCache = new Dictionary<GameObject, ParticleSystem[]>();
        private Camera _camera;

        // UNITY CALLBACKS

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Plays one pooled instance of the prefab at position, pulled toward and
        /// facing the camera, and returns it to its pool after lifetime seconds.
        /// </summary>
        public void Play(GameObject prefab, Vector3 position, float towardCameraOffset, float scale, float lifetime)
        {
            if (prefab == null)
                return;

            if (_camera == null)
                _camera = Camera.main;

            GameObject instance = AcquireInstance(prefab);
            Transform instanceTransform = instance.transform;

            if (_camera != null)
            {
                Vector3 toCamera = (_camera.transform.position - position).normalized;
                position += toCamera * towardCameraOffset;
                instanceTransform.rotation = Quaternion.LookRotation(toCamera);
            }

            instanceTransform.position = position;
            instanceTransform.localScale = Vector3.one * scale;

            instance.SetActive(true);

            foreach (ParticleSystem particle in _particleCache[instance])
            {
                particle.Clear(true);
                particle.Play(true);
            }

            StartCoroutine(ReleaseAfter(prefab, instance, lifetime));
        }

        // PRIVATE METHODS

        /// <summary>Returns a pooled instance or creates a new one for the prefab.</summary>
        private GameObject AcquireInstance(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _pools.Add(prefab, pool);
            }

            if (pool.Count > 0)
                return pool.Dequeue();

            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);

            ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule main = particle.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.playOnAwake = false;
            }

            _particleCache.Add(instance, particles);
            return instance;
        }

        /// <summary>Stops the instance after lifetime seconds and returns it to its pool.</summary>
        private IEnumerator ReleaseAfter(GameObject prefab, GameObject instance, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);

            foreach (ParticleSystem particle in _particleCache[instance])
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            instance.SetActive(false);
            _pools[prefab].Enqueue(instance);
        }
    }
}
