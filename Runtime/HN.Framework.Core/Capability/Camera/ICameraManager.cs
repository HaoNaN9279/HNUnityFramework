#nullable enable

namespace HN.Framework.Core.Capability.Camera
{
    /// <summary>
    /// 摄像机管理器接口，负责管理摄像机预设的注册、切换和振动效果。
    /// Unity 层通过 Cinemachine Brain + VCam Priority 机制实现。
    /// CameraManager 不实现 ITickable，Cinemachine 自管理生命周期。
    /// </summary>
    public interface ICameraManager
    {
        /// <summary>
        /// 注册摄像机预设。预设包含 FOV、裁面、优先级、混合时间等配置。
        /// </summary>
        /// <param name="preset">要注册的摄像机预设</param>
        void RegisterPreset(CameraPreset preset);

        /// <summary>
        /// 设置为当前活跃的摄像机。
        /// Unity 层通过提高对应 VCam 的 Priority 实现。
        /// </summary>
        /// <param name="name">摄像机预设名称</param>
        void SetActiveCamera(string name);

        /// <summary>
        /// 获取当前活跃摄像机名称。
        /// </summary>
        /// <returns>当前活跃摄像机名称，如果没有则返回 null。</returns>
        string? GetActiveCamera();

        /// <summary>
        /// 混合切换到指定摄像机。
        /// </summary>
        /// <param name="name">目标摄像机预设名称</param>
        /// <param name="blendTime">混合过渡时长（秒）</param>
        void BlendToCamera(string name, float blendTime);

        /// <summary>
        /// 在指定摄像机上触发振动效果。
        /// </summary>
        /// <param name="cameraName">摄像机预设名称</param>
        /// <param name="profile">振动配置文件</param>
        void Shake(string cameraName, CameraShakeProfile profile);

        /// <summary>
        /// 停止指定摄像机的振动效果。
        /// </summary>
        /// <param name="cameraName">摄像机预设名称</param>
        void StopShake(string cameraName);

        /// <summary>
        /// 尝试获取已注册的预设。
        /// </summary>
        /// <param name="name">预设名称</param>
        /// <param name="preset">输出参数，返回找到的预设</param>
        /// <returns>如果找到预设则返回 true，否则返回 false。</returns>
        bool TryGetPreset(string name, out CameraPreset preset);

        /// <summary>
        /// 移除已注册的预设。
        /// </summary>
        /// <param name="name">要移除的预设名称</param>
        void RemovePreset(string name);
    }
}
