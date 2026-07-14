using System;
using System.Collections.Generic;
using FishNet;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability.Network.Messages;
using MemoryPack;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 帧同步网络驱动，桥接 <see cref="IFrameSyncManager"/> 与 FishNet 网络传输。
    /// 纯 C# 类，不继承 MonoBehaviour，通过 <see cref="FishNetMessageBus"/> 收发帧同步消息。
    /// 
    /// 职责：
    /// - 客户端：发送本地帧输入到服务端；接收服务端广播的帧数据和校验和
    /// - 服务端：接收所有客户端的帧输入；在每个逻辑帧开始广播帧数据和校验和
    /// </summary>
    public class LockstepNetworkDriver
    {
        private readonly IFrameSyncManager m_frameSyncManager;
        private readonly FishNetMessageBus m_messageBus;
        private readonly int m_localClientId;
        private bool m_isInitialized;

        /// <summary>
        /// 当前客户端本地的帧输入队列，在服务端广播帧数据前暂存。
        /// </summary>
        private readonly FrameInputBuffer m_pendingInputs;

        /// <summary>
        /// 获取是否已初始化。
        /// </summary>
        public bool IsInitialized => m_isInitialized;

        /// <summary>
        /// 创建帧同步网络驱动器。
        /// </summary>
        /// <param name="frameSyncManager">帧同步管理器实例</param>
        /// <param name="messageBus">FishNet 消息总线</param>
        /// <param name="localClientId">本地客户端 ID，用于标识帧输入来源</param>
        public LockstepNetworkDriver(IFrameSyncManager frameSyncManager, FishNetMessageBus messageBus, int localClientId = 0)
        {
            m_frameSyncManager = frameSyncManager ?? throw new ArgumentNullException(nameof(frameSyncManager));
            m_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            m_localClientId = localClientId;
            m_pendingInputs = new FrameInputBuffer(frameSyncManager.BufferSize);
        }

        /// <summary>
        /// 初始化帧同步网络驱动，注册消息处理器和帧事件回调。
        /// </summary>
        public void Initialize()
        {
            if (m_isInitialized)
                return;

            // 注册消息接收处理器
            m_messageBus.RegisterHandler<FrameInputMessage>(OnFrameInputReceived);
            m_messageBus.RegisterHandler<FrameDataMessage>(OnFrameDataReceived);

            // 订阅帧开始事件
            m_frameSyncManager.OnFrameStart += OnFrameStart;

            m_isInitialized = true;
        }

        /// <summary>
        /// 提交本地客户端的帧输入。将输入发送到服务端并暂存到本地缓冲。
        /// </summary>
        /// <param name="input">帧输入数据</param>
        public void SubmitLocalInput(FrameInput input)
        {
            if (!m_isInitialized)
                return;

            // 发送到服务端
            var message = new FrameInputMessage(m_localClientId, input);
            m_messageBus.SendToServer(message);

            // 暂存到本地缓冲（用于回放/校验）
            m_pendingInputs.Enqueue(input);
        }

        /// <summary>
        /// 关闭帧同步网络驱动，注销所有消息处理器和事件回调。
        /// </summary>
        public void Shutdown()
        {
            if (!m_isInitialized)
                return;

            m_frameSyncManager.OnFrameStart -= OnFrameStart;
            m_messageBus.UnregisterHandler<FrameInputMessage>(OnFrameInputReceived);
            m_messageBus.UnregisterHandler<FrameDataMessage>(OnFrameDataReceived);
            m_pendingInputs.Clear();
            m_isInitialized = false;
        }

        /// <summary>
        /// 帧开始事件回调。
        /// 服务端：收集所有客户端的输入 → 计算校验和 → 广播帧数据
        /// </summary>
        /// <param name="frameNumber">当前逻辑帧号</param>
        private void OnFrameStart(ulong frameNumber)
        {
            if (!InstanceFinder.IsServer)
                return;

            // 收集该帧所有客户端的输入
            // 通过 TryGetInput 获取所有输入的聚合校验和
            if (m_frameSyncManager.TryGetInput(frameNumber, out FrameInput input))
            {
                // 计算输入的校验和
                byte[] serializedInput = MemoryPackSerializer.Serialize(input);
                ulong checksum = new XorChecksumProvider().Compute(serializedInput);
                m_frameSyncManager.RegisterChecksum(frameNumber, checksum);

                // 广播帧数据到所有客户端
                var frameData = new FrameDataMessage(frameNumber, new[] { input }, checksum);
                m_messageBus.SendToAll(frameData);
            }
        }

        /// <summary>
        /// 收到客户端帧输入消息时的回调（服务端）。
        /// </summary>
        /// <param name="message">帧输入消息</param>
        private void OnFrameInputReceived(FrameInputMessage message)
        {
            m_frameSyncManager.SubmitInput(message.ClientId, message.Input);
        }

        /// <summary>
        /// 收到服务端帧数据广播时的回调（客户端）。
        /// 将接收到的帧输入写入同步管理器，并比对校验和。
        /// </summary>
        /// <param name="message">帧数据消息</param>
        private void OnFrameDataReceived(FrameDataMessage message)
        {
            // 将所有客户端的输入写入同步管理器
            if (message.Inputs != null)
            {
                for (int i = 0; i < message.Inputs.Length; i++)
                {
                    m_frameSyncManager.SubmitInput(i, message.Inputs[i]);
                }
            }

            // 注册服务端的校验和
            m_frameSyncManager.RegisterChecksum(message.FrameNumber, message.Checksum);

            // 比对本地校验和与服务端校验和
            CompareChecksum(message.FrameNumber, message.Checksum);
        }

        /// <summary>
        /// 比对指定帧号的本地校验和与服务端校验和。
        /// 不一致时输出警告日志。
        /// </summary>
        /// <param name="frameNumber">帧号</param>
        /// <param name="serverChecksum">服务端校验和</param>
        private void CompareChecksum(ulong frameNumber, ulong serverChecksum)
        {
            if (m_pendingInputs.TryGet(frameNumber, out FrameInput localInput))
            {
                byte[] serializedInput = MemoryPackSerializer.Serialize(localInput);
                ulong localChecksum = new XorChecksumProvider().Compute(serializedInput);

                if (localChecksum != serverChecksum)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[Lockstep] Frame {frameNumber}: Checksum mismatch! " +
                        $"Local=0x{localChecksum:X16}, Server=0x{serverChecksum:X16}. " +
                        "Possible desync detected.");
                }
            }
        }
    }
}