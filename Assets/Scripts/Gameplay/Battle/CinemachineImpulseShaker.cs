using MightyHeroes.Gameplay.Combat.Feedback;
using Unity.Cinemachine;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// ICameraShaker implementation backed by a CinemachineImpulseSource.
    ///
    /// The impulse only moves cameras that carry a CinemachineImpulseListener
    /// extension, so the battle camera (and later the ultimate camera) must
    /// have one. Shake shape and duration are authored on the impulse source's
    /// Impulse Definition; only the force comes from code.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public sealed class CinemachineImpulseShaker : MonoBehaviour, ICameraShaker
    {
        // FIELDS

        [SerializeField] private CinemachineImpulseSource _impulseSource;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_impulseSource == null)
                _impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Generates one impulse with the given force. Zero or less plays nothing.
        /// </summary>
        /// <param name="force">Impulse force.</param>
        public void Shake(float force)
        {
            if (force <= 0f || _impulseSource == null)
                return;

            _impulseSource.GenerateImpulseWithForce(force);
        }
    }
}
