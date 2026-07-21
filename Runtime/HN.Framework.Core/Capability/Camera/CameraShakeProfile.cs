#nullable enable

using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Camera
{
    /// <summary>
    /// 摄像机振动衰减曲线类型。
    /// </summary>
    public enum CameraShakeDecayType
    {
        /// <summary>
        /// 线性衰减：振幅随时间线性减小。
        /// </summary>
        Linear,

        /// <summary>
        /// 指数衰减：振幅随时间指数级减小。
        /// </summary>
        Exponential,

        /// <summary>
        /// 缓出衰减：开始快、结束慢的衰减曲线。
        /// </summary>
        EaseOut,

        /// <summary>
        /// 缓入衰减：开始慢、结束快的衰减曲线。
        /// </summary>
        EaseIn,

        /// <summary>
        /// 缓入缓出衰减：开始慢、中间快、结束慢的衰减曲线。
        /// </summary>
        EaseInOut
    }

    /// <summary>
    /// 摄像机振动配置文件，定义振动效果的振幅、频率、持续时间和衰减方式。
    /// </summary>
    public sealed class CameraShakeProfile : IReference
    {
        /// <summary>
        /// 振幅增益，控制振动的强度。默认值 1。
        /// </summary>
        public float AmplitudeGain { get; set; } = 1f;

        /// <summary>
        /// 频率增益，控制振动的速度。默认值 1。
        /// </summary>
        public float FrequencyGain { get; set; } = 1f;

        /// <summary>
        /// 振动持续时长（秒）。默认值 0.5。
        /// </summary>
        public float Duration { get; set; } = 0.5f;

        /// <summary>
        /// 衰减曲线类型，决定振动如何随时间减弱。默认值 EaseOut。
        /// </summary>
        public CameraShakeDecayType DecayType { get; set; } = CameraShakeDecayType.EaseOut;

        /// <summary>
        /// 清理对象状态，将所有属性重置为默认值。
        /// </summary>
        public void Clear()
        {
            AmplitudeGain = 1f;
            FrequencyGain = 1f;
            Duration = 0.5f;
            DecayType = CameraShakeDecayType.EaseOut;
        }
    }
}
