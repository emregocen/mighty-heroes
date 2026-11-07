using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle
{
    /// <summary>
    /// Overlay camera that draws only the hit VFX layer on top of the 3D scene,
    /// so hit effects are never hidden inside or behind character meshes,
    /// whatever the camera angle.
    ///
    /// Must be a child of the main camera with a zero local transform. On Awake
    /// it removes the VFX layer from the main camera and configures itself to
    /// render only that layer after the main camera with a depth-only clear.
    /// Every LateUpdate it copies the main camera's lens (FOV, clip planes) so
    /// Cinemachine lens changes stay in sync.
    ///
    /// Built-in render pipeline only (URP would use a Render Objects feature or
    /// camera stacking instead). Screen Space Overlay UI still draws above it.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class HitVfxOverlayCamera : MonoBehaviour
    {
        // FIELDS

        [Tooltip("Layer that hit VFX instances are moved to. Must match PooledHitVfxPlayer.")]
        [SerializeField] private string _vfxLayerName = "HitVfx";

        [Tooltip("Camera this overlay follows. Defaults to the parent camera.")]
        [SerializeField] private Camera _sourceCamera;

        private Camera _overlayCamera;

        // UNITY CALLBACKS

        private void Awake()
        {
            _overlayCamera = GetComponent<Camera>();

            if (_sourceCamera == null && transform.parent != null)
                _sourceCamera = transform.parent.GetComponent<Camera>();

            int vfxLayer = LayerMask.NameToLayer(_vfxLayerName);

            if (_sourceCamera == null || vfxLayer < 0)
            {
                GameLogger.Log(LogSourceType.BattleSystem,
                    $"HitVfxOverlayCamera: missing source camera or layer '{_vfxLayerName}', overlay disabled.");
                enabled = false;
                _overlayCamera.enabled = false;
                return;
            }

            int vfxMask = 1 << vfxLayer;

            _sourceCamera.cullingMask &= ~vfxMask;

            _overlayCamera.cullingMask = vfxMask;
            _overlayCamera.clearFlags = CameraClearFlags.Depth;
            _overlayCamera.depth = _sourceCamera.depth + 1f;

            CopyLens();
        }

        private void LateUpdate()
        {
            CopyLens();
        }

        // PRIVATE METHODS

        /// <summary>Copies projection settings from the source camera.</summary>
        private void CopyLens()
        {
            _overlayCamera.fieldOfView = _sourceCamera.fieldOfView;
            _overlayCamera.nearClipPlane = _sourceCamera.nearClipPlane;
            _overlayCamera.farClipPlane = _sourceCamera.farClipPlane;
            _overlayCamera.orthographic = _sourceCamera.orthographic;
            _overlayCamera.orthographicSize = _sourceCamera.orthographicSize;
        }
    }
}
