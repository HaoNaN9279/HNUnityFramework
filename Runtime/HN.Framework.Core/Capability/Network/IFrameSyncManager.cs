using System;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 帧同步管理器接口，定义帧同步的核心契约。
    /// 继承 <see cref="ITickable"/>，通过固定帧率驱动逻辑帧更新。
    /// 服务端通过 <see cref="SubmitInput"/> 收集客户端输入，
    /// 在 <see cref="OnFrameStart"/> 中触发确定性 Tick。
    /// </summary>
    public interface IFrameSyncManager : ITickable
    {
        /// <summary>
        /// 获取当前逻辑帧号，从 1 开始递增。
        /// </summary>
        ulong CurrentFrame { get; }

        /// <summary>
        /// 获取或设置逻辑帧率（tick/s），默认 15。
        /// 运行时修改后将在下一次 Tick 生效。
        /// </summary>
        int FrameRate { get; set; }

        /// <summary>
        /// 获取或设置帧输入缓冲容量，默认 3。
        /// 决定客户端可提前发送的帧数。
        /// </summary>
        int BufferSize { get; set; }

        /// <summary>
        /// 获取或设置是否启用帧同步。
        /// 关闭时 <see cref="Tick"/> 不驱动逻辑帧。
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 提交客户端输入到帧缓冲。
        /// 服务端模式下由 <see cref="LockstepNetworkDriver"/> 调用，
        /// 用于收集所有客户端的帧输入。
        /// </summary>
        /// <param name="clientId">客户端 ID</param>
        /// <param name="input">帧输入数据</param>
        void SubmitInput(int clientId, FrameInput input);

        /// <summary>
        /// 尝试获取指定帧号的输入数据。
        /// 用于确定性 Tick 中的状态计算。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <param name="input">输出参数，帧输入数据</param>
        /// <returns>是否存在该帧的输入</returns>
        bool TryGetInput(ulong frameNumber, out FrameInput input);

        /// <summary>
        /// 获取指定帧号的校验和。
        /// 用于客户端与服务端的帧同步一致性比对。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <returns>该帧的校验和值</returns>
        ulong GetChecksum(ulong frameNumber);

        /// <summary>
        /// 注册指定帧号的校验和。
        /// 服务端计算后调用，供客户端比对。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <param name="checksum">校验和值</param>
        void RegisterChecksum(ulong frameNumber, ulong checksum);

        /// <summary>
        /// 每帧开始时触发，参数为当前帧号。
        /// 订阅者在此事件中执行确定性状态更新。
        /// </summary>
        event Action<ulong> OnFrameStart;
    }
}