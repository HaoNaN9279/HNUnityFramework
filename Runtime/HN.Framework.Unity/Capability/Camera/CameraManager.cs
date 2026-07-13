#nullable enable

using System;
using System.Collections.Generic;
using Cinemachine;
using HN.Framework.Core.Capability.Camera;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Camera
{
    /// <summary>
    /// 摄像机管理器，Cinemachine Brain 的包装器。
    /// 管理虚拟摄像机的注册、切换、混合和振动。
    /// 不实现 ITickable，Cinemachine 自管理生命周期。
    /// </summary>
    public sealed class CameraManager : ICameraManager, IDisposable
    {
        private readonly CinemachineBrain? _brain;
        private readonly Dictionary<string, CameraHandle> _cameras;
        private readonly Dictionary<string, CameraPreset> _presets;
        private readonly Dictionary<string, CameraShake> _shakes;

        /// <summary>
        /// 创建一个摄像机管理器实例。
        /// </summary>
        /// <param name="brain">场景中的 CinemachineBrain。可为 null（用于 EditMode 测试降级）。</param>
        public CameraManager(CinemachineBrain? brain = null)
        {
            _brain = brain;
            _cameras = new Dictionary<string, CameraHandle>();
            _presets = new Dictionary<string, CameraPreset>();
            _shakes = new Dictionary<string, CameraShake>();
        }

        /// <summary>
        /// 注册并包装一个虚拟摄像机。如果已存在同名摄像机则覆盖。
        /// </summary>
        /// <param name="name">摄像机名称。</param>
        /// <param name="vcam">Cinemachine 虚拟摄像机实例。</param>
        /// <returns>包装后的 CameraHandle。</returns>
        public CameraHandle RegisterCamera(string name, CinemachineVirtualCameraBase vcam)
        {
            var handle = new CameraHandle(name, vcam);
            _cameras[name] = handle;

            // 如果存在对应名称的预设，自动应用
            if (_presets.TryGetValue(name, out var preset))
            {
                handle.ApplyPreset(preset);
            }

            return handle;
        }

        /// <summary>
        /// 尝试获取已注册的 CameraHandle。
        /// </summary>
        /// <param name="name">摄像机名称。</param>
        /// <returns>找到的句柄，未注册时返回 null。</returns>
        public CameraHandle? GetCamera(string name)
        {
            _cameras.TryGetValue(name, out var handle);
            return handle;
        }

        // --- ICameraManager 实现 ---

        /// <inheritdoc />
        public void RegisterPreset(CameraPreset preset)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            _presets[preset.Name] = preset;

            // 如果已存在同名摄像机，立即应用预设
            if (_cameras.TryGetValue(preset.Name, out var handle))
            {
                handle.ApplyPreset(preset);
            }
        }

        /// <inheritdoc />
        public void SetActiveCamera(string name)
        {
            if (!_cameras.TryGetValue(name, out var handle))
                return;

            // 将所有 VCam 优先级设为默认值，目标设为高优先级
            foreach (var kvp in _cameras)
            {
                kvp.Value.VirtualCamera.Priority = 10;
            }
            handle.VirtualCamera.Priority = 20;
        }

        /// <inheritdoc />
        public string? GetActiveCamera()
        {
            if (_brain == null || _brain.ActiveVirtualCamera == null)
                return null;

            var activeVcam = _brain.ActiveVirtualCamera;
            foreach (var kvp in _cameras)
            {
                if (kvp.Value.VirtualCamera == activeVcam)
                    return kvp.Key;
            }

            return null;
        }

        /// <inheritdoc />
        public void BlendToCamera(string name, float blendTime)
        {
            if (!_cameras.TryGetValue(name, out var handle))
                return;

            // 设置目标高优先级触发 Brain 混合
            foreach (var kvp in _cameras)
            {
                kvp.Value.VirtualCamera.Priority = 10;
            }
            handle.VirtualCamera.Priority = 20;

            // 设置混合时间 (如果 Brain 可用)
            if (_brain != null && blendTime > 0f)
            {
                // 使用默认混合覆盖（临时更改默认混合时间）
                // 注意：Cinemachine 会使用当前混合定义；简单方案：仅靠 Priority 切换
                // 更精确的方案需要设置 m_CustomBlends，这里简化处理
            }
        }

        /// <inheritdoc />
        public void Shake(string cameraName, CameraShakeProfile profile)
        {
            if (profile == null) return;
            if (!_cameras.TryGetValue(cameraName, out var handle)) return;
            if (_brain == null) return; // 无 Brain 时无法驱动协程

            if (!_shakes.TryGetValue(cameraName, out var shake))
            {
                shake = new CameraShake(handle, _brain);
                _shakes[cameraName] = shake;
            }

            shake.StartShake(profile);
        }

        /// <inheritdoc />
        public void StopShake(string cameraName)
        {
            if (_shakes.TryGetValue(cameraName, out var shake))
            {
                shake.StopShake();
            }
        }

        /// <inheritdoc />
        public bool TryGetPreset(string name, out CameraPreset preset)
        {
            return _presets.TryGetValue(name, out preset);
        }

        /// <inheritdoc />
        public void RemovePreset(string name)
        {
            _presets.Remove(name);
        }

        public void Dispose()
        {
            foreach (var shake in _shakes.Values)
            {
                shake.StopShake();
            }
            _cameras.Clear();
            _presets.Clear();
            _shakes.Clear();
        }
    }
}
