using System.Collections.Generic;
using UnityEngine;

namespace MightyHeroes.Gameplay.FloatingText
{
    /// <summary>
    /// Tracks how many floating texts have recently spawned near each entity,
    /// so simultaneous or rapid-fire texts (damage, K.O., overkill all landing
    /// within the same moment) stack vertically instead of overlapping —
    /// matching the layered look of damage/K.O./overkill callouts stacking
    /// upward in sequence.
    ///
    /// A stack resets for an entity once StackWindowSeconds has passed since
    /// its last spawn, so unrelated hits later in the battle start a fresh stack
    /// rather than climbing indefinitely.
    /// </summary>
    public sealed class FloatingTextStackTracker
    {
        // FIELDS

        private readonly float _stackWindowSeconds;
        private readonly float _stackSpacing;
        private readonly Dictionary<int, StackState> _stacks = new Dictionary<int, StackState>();

        // NESTED TYPES

        private struct StackState
        {
            public int Count;
            public float LastSpawnTime;
        }

        // CONSTRUCTOR

        /// <summary>
        /// stackWindowSeconds: how long after the last spawn a new one still
        /// counts as part of the same stack. stackSpacing: vertical world-space
        /// distance between each stacked entry.
        /// </summary>
        public FloatingTextStackTracker(float stackWindowSeconds = 0.6f, float stackSpacing = 0.5f)
        {
            _stackWindowSeconds = stackWindowSeconds;
            _stackSpacing = stackSpacing;
        }

        // PUBLIC METHODS

        /// <summary>
        /// Registers a new spawn for the given entity and returns the vertical
        /// offset (world units) this spawn should be placed at, stacking above
        /// any texts spawned for the same entity within the stack window.
        /// </summary>
        public float RegisterSpawnAndGetOffset(int entityId)
        {
            float now = Time.time;

            if (_stacks.TryGetValue(entityId, out StackState state) &&
                now - state.LastSpawnTime <= _stackWindowSeconds)
            {
                state.Count++;
            }
            else
            {
                state.Count = 0;
            }

            state.LastSpawnTime = now;
            _stacks[entityId] = state;

            return state.Count * _stackSpacing;
        }
    }
}