using System.Collections;
using MightyHeroes.Core.EventSystem;
using MightyHeroes.Core.Logging;
using MightyHeroes.Gameplay.Ultimate.Payloads;
using Unity.Cinemachine;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Pulls the camera back and widens the field of view while the hero
    /// ultimate plays, then returns to the battle camera.
    ///
    /// On UltimateStartedEvent the ultimate camera copies the live camera pose,
    /// moves backward along its view direction and slightly up, tilts down a
    /// little and adds FieldOfViewIncrease, then raises its priority with a
    /// smooth blend. On UltimateEndedEvent it lowers the priority with a slower
    /// blend. If the ultimate ends the battle, the victory camera (priority 20)
    /// takes over directly from this camera.
    ///
    /// Brain blend settings are overridden only during the two blends and are
    /// always restored. The ultimate camera needs a CinemachineImpulseListener
    /// for camera shake during the ultimate.
    ///
    /// Initialized by BattleBootstrapper. Battle camera priority is 10, kill-cam
    /// 15, victory 20; this camera uses 16.
    /// </summary>
    public sealed class UltimateCameraController : MonoBehaviour
    {
        // CONSTANTS

        private const int PriorityActive = 16;
        private const int PriorityInactive = 0;

        // FIELDS

        [Header("Camera")]
        [Tooltip("Cinemachine camera used for the ultimate shot. It needs no Tracking or Look At target; this controller places it.")]
        [SerializeField] private CinemachineCamera _ultimateCamera;

        [Header("Framing (relative to the live battle camera)")]
        [Tooltip("Distance moved backward along the camera's view direction.")]
        [SerializeField, Min(0f)] private float _pullBackDistance = 2.5f;

        [Tooltip("Extra height above the battle camera.")]
        [SerializeField] private float _raiseHeight = 0.8f;

        [Tooltip("Extra downward pitch in degrees so the wider frame stays on the fight.")]
        [SerializeField, Range(-20f, 20f)] private float _extraPitch = 4f;

        [Tooltip("Degrees added to the battle camera's field of view.")]
        [SerializeField, Range(0f, 40f)] private float _fieldOfViewIncrease = 12f;

        [Header("Blends")]
        [SerializeField, Min(0f)] private float _blendInDuration = 0.45f;
        [SerializeField, Min(0f)] private float _blendOutDuration = 0.7f;

        [Tooltip("The ultimate camera is released after this many seconds even if the ultimate never ends.")]
        [SerializeField, Min(0.5f)] private float _maxHoldDuration = 5f;

        private EventHub _eventHub;
        private CinemachineBrain _brain;
        private Camera _outputCamera;
        private CinemachineBlendDefinition _originalDefaultBlend;
        private bool _isActive;
        private Coroutine _holdRoutine;
        private Coroutine _blendRestoreRoutine;

        // UNITY CALLBACKS

        private void OnDisable()
        {
            RestoreBrain();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Wires the controller into the battle. Called by BattleBootstrapper.
        /// </summary>
        /// <param name="eventHub">Battle-scoped event hub.</param>
        public void Initialize(EventHub eventHub)
        {
            _eventHub = eventHub;
            _outputCamera = Camera.main;
            _brain = _outputCamera != null ? _outputCamera.GetComponent<CinemachineBrain>() : null;

            if (_ultimateCamera == null || _brain == null)
            {
                GameLogger.Log(LogSourceType.BattleSystem, "UltimateCameraController: missing ultimate camera or CinemachineBrain, ultimate camera disabled.");
                return;
            }

            _ultimateCamera.Priority = PriorityInactive;
            _ultimateCamera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
            _originalDefaultBlend = _brain.DefaultBlend;

            _eventHub.HeroEvents.UltimateStartedEvent.Register(OnUltimateStarted);
            _eventHub.HeroEvents.UltimateEndedEvent.Register(OnUltimateEnded);
        }

        // PRIVATE METHODS

        private void UnregisterEvents()
        {
            if (_eventHub == null)
                return;

            _eventHub.HeroEvents.UltimateStartedEvent.Unregister(OnUltimateStarted);
            _eventHub.HeroEvents.UltimateEndedEvent.Unregister(OnUltimateEnded);
        }

        /// <summary>
        /// Places the ultimate camera behind and above the live camera pose with a
        /// wider lens, and pushes the pose into Cinemachine immediately.
        /// </summary>
        private void PlaceCamera()
        {
            Transform source = _outputCamera.transform;

            Vector3 position = source.position - source.forward * _pullBackDistance + Vector3.up * _raiseHeight;
            Quaternion rotation = Quaternion.AngleAxis(_extraPitch, source.right) * source.rotation;

            LensSettings lens = _ultimateCamera.Lens;
            lens.FieldOfView = Mathf.Clamp(_outputCamera.fieldOfView + _fieldOfViewIncrease, 1f, 179f);
            lens.Dutch = 0f;
            _ultimateCamera.Lens = lens;

            _ultimateCamera.transform.SetPositionAndRotation(position, rotation);
            _ultimateCamera.ForceCameraPosition(position, rotation);
        }

        /// <summary>Raises or lowers the ultimate camera with a temporary blend override.</summary>
        private void SetCameraActive(bool isActive, float blendDuration)
        {
            _isActive = isActive;
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
            _ultimateCamera.Priority = isActive ? PriorityActive : PriorityInactive;

            RestartRoutine(ref _blendRestoreRoutine, RestoreBlendAfter(blendDuration));
        }

        private void RestoreBrain()
        {
            if (_brain != null)
                _brain.DefaultBlend = _originalDefaultBlend;
        }

        private void RestartRoutine(ref Coroutine routine, IEnumerator body)
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(body);
        }

        private IEnumerator RestoreBlendAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            _blendRestoreRoutine = null;
            RestoreBrain();
        }

        private IEnumerator ReleaseAfterMaxHold()
        {
            yield return new WaitForSecondsRealtime(_maxHoldDuration);

            _holdRoutine = null;

            if (_isActive)
                SetCameraActive(false, _blendOutDuration);
        }

        // EVENT HANDLERS

        private void OnUltimateStarted(UltimateStartedPayload payload)
        {
            PlaceCamera();
            SetCameraActive(true, _blendInDuration);
            RestartRoutine(ref _holdRoutine, ReleaseAfterMaxHold());
        }

        private void OnUltimateEnded(UltimateEndedPayload payload)
        {
            if (_holdRoutine != null)
            {
                StopCoroutine(_holdRoutine);
                _holdRoutine = null;
            }

            if (_isActive)
                SetCameraActive(false, _blendOutDuration);
        }
    }
}
