using UnityEngine;

namespace MightyHeroes.Battle.BattleTimer
{
    /// <summary>
    /// Drives the BattleTimer's Tick from Unity's Update loop.
    ///
    /// This MonoBehaviour exists solely so the timer (a plain C# class) can advance
    /// each frame without coupling BattleBootstrapper to runtime tick responsibilities.
    /// Spawned and bound by BattleBootstrapper; destroyed when the battle ends.
    /// </summary>
    public sealed class BattleTimerRunner : MonoBehaviour
    {
        // FIELDS

        private BattleTimer _timer;

        // UNITY CALLBACKS

        private void Update()
        {
            _timer?.Tick(Time.deltaTime);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Binds the timer instance this runner will tick.
        /// Call once after BattleBootstrapper creates the timer.
        /// </summary>
        public void Bind(BattleTimer timer)
        {
            _timer = timer;
        }
    }
}