#nullable enable

using System;
using Cinemachine;
using HN.Framework.Core.Capability.Camera;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Camera
{
    /// <summary>
    /// 运行时虚拟摄像机引用管理。
    /// 持有 CinemachineVirtualCamera 引用，
    /// 提供 Follow/LookAt 目标绑定、预设切换入口。
    /// </summary>
    public sealed class CameraHandle : IDisposable
    {
        private readonly CinemachineVirtualCameraBase _vcam;

        /// <summary>获取此摄像机的名称。</summary>
        public string Name { get; }

        /// <summary>获取原始 CinemachineVirtualCameraBase 引用。</summary>
        public CinemachineVirtualCameraBase VirtualCamera => _vcam;

        /// <summary>
        /// 创建一个摄像机句柄。
        /// </summary>
        /// <param name="name">摄像机名称。</param>
        /// <param name="vcam">Cinemachine 虚拟摄像机。</param>
        /// <exception cref="ArgumentNullException">任一参数为 null 时抛出。</exception>
        public CameraHandle(string name, CinemachineVirtualCameraBase vcam)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _vcam = vcam ?? throw new ArgumentNullException(nameof(vcam));
        }

        /// <summary>设置 Follow 目标。</summary>
        /// <param name="follow">要跟随的 Transform。</param>
        public void SetFollow(Transform follow) => _vcam.Follow = follow;

        /// <summary>设置 LookAt 目标。</summary>
        /// <param name="lookAt">要注视的 Transform。</param>
        public void SetLookAt(Transform lookAt) => _vcam.LookAt = lookAt;

        /// <summary>
        /// 将 CameraPreset 的数据应用到 VCam。
        /// 设置 FOV、近远裁面、Priority。
        /// </summary>
        /// <param name="preset">摄像机预设。</param>
        public void ApplyPreset(CameraPreset preset)
        {
            if (preset == null) return;
            _vcam.Priority = preset.Priority;

            // FOV 和裁面需要访问 CinemachineVirtualCamera 的 Lens
            if (_vcam is CinemachineVirtualCamera vcam)
            {
                var lens = vcam.m_Lens;
                lens.FieldOfView = preset.FieldOfView;
                lens.NearClipPlane = preset.NearClipPlane;
                lens.FarClipPlane = preset.FarClipPlane;
                vcam.m_Lens = lens; // Lens 是 struct，需要重新赋值
            }
        }

        /// <summary>
        /// 获取 VCam 上的 Cinemachine 组件。
        /// </summary>
        /// <typeparam name="T">组件类型。</typeparam>
        /// <returns>找到的组件，如果 VCam 不是 CinemachineVirtualCamera 则返回 null。</returns>
        public T? GetCinemachineComponent<T>() where T : CinemachineComponentBase
        {
            if (_vcam is CinemachineVirtualCamera vcam)
                return vcam.GetCinemachineComponent<T>();
            return null;
        }

        public void Dispose()
        {
            // 仅清理引用，不销毁 VCam（所有权由场景/预制体管理）
        }
    }
}
