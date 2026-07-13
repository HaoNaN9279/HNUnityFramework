#nullable enable

using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Camera
{
    /// <summary>
    /// 摄像机预设，包含 FOV、裁面、优先级、混合时间等配置。
    /// 预设通过名称索引，由 ICameraManager 注册和管理。
    /// </summary>
    public sealed class CameraPreset : IReference
    {
        /// <summary>
        /// 预设名称，用于字典索引。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 视野角度（Field of View），默认值 60。
        /// </summary>
        public float FieldOfView { get; set; } = 60f;

        /// <summary>
        /// 近裁面距离，默认值 0.3。
        /// </summary>
        public float NearClipPlane { get; set; } = 0.3f;

        /// <summary>
        /// 远裁面距离，默认值 1000。
        /// </summary>
        public float FarClipPlane { get; set; } = 1000f;

        /// <summary>
        /// 优先级，对应 Cinemachine Priority。值越大优先级越高，默认值 10。
        /// </summary>
        public int Priority { get; set; } = 10;

        /// <summary>
        /// 混合过渡时长（秒），默认值 0.5。
        /// </summary>
        public float BlendTime { get; set; } = 0.5f;

        /// <summary>
        /// 创建一个具有指定名称的摄像机预设。
        /// </summary>
        /// <param name="name">预设名称</param>
        public CameraPreset(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 创建具有默认值的摄像机预设。
        /// </summary>
        public CameraPreset()
        {
            Name = string.Empty;
        }

        /// <summary>
        /// 清理对象状态，将所有属性重置为默认值。
        /// </summary>
        public void Clear()
        {
            Name = string.Empty;
            FieldOfView = 60f;
            NearClipPlane = 0.3f;
            FarClipPlane = 1000f;
            Priority = 10;
            BlendTime = 0.5f;
        }
    }
}
