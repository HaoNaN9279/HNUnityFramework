using UnityEngine;

namespace HN.Framework.Unity.Capability.Cutscene
{
    [AddComponentMenu("HN Framework/Cutscene/CutsceneCameraHook")]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CutsceneCameraHook : MonoBehaviour
    {
        [Tooltip("过场摄像机名称，用于 CutsceneManager 查找和切换")]
        public string CameraName = "CutsceneCamera";

        [Tooltip("优先级（Cinemachine VCam Priority），越高越优先")]
        public int Priority = 10;

        private UnityEngine.Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        public UnityEngine.Camera GetCamera()
        {
            if (_camera == null)
                _camera = GetComponent<UnityEngine.Camera>();
            return _camera;
        }

        private void Reset()
        {
            CameraName = gameObject.name;
        }

        private void OnDrawGizmosSelected()
        {
            var cam = GetCamera();
            if (cam == null) return;

            Gizmos.color = Color.cyan;
            var dir = transform.forward;
            // Simple camera frustum visualization
            float aspect = cam.aspect;
            Gizmos.DrawFrustum(transform.position, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, aspect);
        }
    }
}