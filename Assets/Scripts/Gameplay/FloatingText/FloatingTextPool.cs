using System.Collections.Generic;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Generic object pool for a single floating text prefab (world or screen view).
    /// Pre-warms PoolSizePerPrefab instances at construction so no Instantiate
    /// happens during battle — avoids per-hit GC allocation spikes.
    ///
    /// If the pool is exhausted, the oldest active instance is force-returned
    /// and reused rather than growing the pool, so worst-case memory stays bounded.
    /// </summary>
    public sealed class FloatingTextPool<TView> where TView : Component, IFloatingTextHandle
    {
        // FIELDS

        private readonly TView _prefab;
        private readonly Transform _parent;
        private readonly Queue<TView> _available = new Queue<TView>();
        private readonly List<TView> _activeInOrder = new List<TView>();

        // CONSTRUCTOR

        /// <summary>
        /// Instantiates poolSize copies of prefab under parent, all initially inactive.
        /// </summary>
        public FloatingTextPool(TView prefab, Transform parent, int poolSize)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < poolSize; i++)
            {
                TView instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _available.Enqueue(instance);
            }
        }

        // PUBLIC METHODS

        /// <summary>
        /// Returns an available instance, recycling the oldest active one if
        /// the pool is exhausted. Caller is responsible for calling Play on
        /// the returned instance.
        /// </summary>
        public TView Acquire()
        {
            TView instance;

            if (_available.Count > 0)
            {
                instance = _available.Dequeue();
            }
            else
            {
                instance = _activeInOrder[0];
                _activeInOrder.RemoveAt(0);
                instance.ForceReturn();
            }

            _activeInOrder.Add(instance);
            return instance;
        }

        /// <summary>Called by the view itself (via onReturnToPool callback) when its animation completes.</summary>
        public void Release(TView instance)
        {
            _activeInOrder.Remove(instance);

            if (!_available.Contains(instance))
                _available.Enqueue(instance);
        }
    }
}