using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 帧同步管理器核心实现，实现 <see cref="IFrameSyncManager"/> 接口。
    /// 通过固定帧率累加器驱动逻辑帧 Tick，管理帧输入缓冲和校验和。
    /// </summary>
    public sealed class LockstepManager : IFrameSyncManager
    {
        private int m_frameRate;
        private int m_bufferSize;
        private bool m_isEnabled;
        private ulong m_currentFrame;

        /// <summary>
        /// 帧输入环形缓冲，存储来自所有客户端的帧输入。
        /// </summary>
        private readonly FrameInputBuffer m_inputBuffer;

        /// <summary>
        /// 校验和提供器。
        /// </summary>
        private readonly IChecksumProvider m_checksumProvider;

        /// <summary>
        /// 帧校验和注册表，存储每帧的校验和值。
        /// </summary>
        private readonly Dictionary<ulong, ulong> m_checksumRegistry;

        /// <summary>
        /// 时间累加器，用于固定帧率驱动。
        /// </summary>
        private double m_accumulatedTime;

        /// <summary>
        /// 帧间隔（秒）。
        /// </summary>
        private double m_frameInterval;

        /// <summary>
        /// 追帧保护上限。
        /// </summary>
        private int m_maxCatchUpFrames;

        /// <summary>
        /// 获取当前逻辑帧号，从 1 开始递增。
        /// </summary>
        public ulong CurrentFrame => m_currentFrame;

        /// <summary>
        /// 获取或设置逻辑帧率（tick/s），默认 15。
        /// 运行时修改后将在下一次 Tick 生效。
        /// </summary>
        public int FrameRate
        {
            get => m_frameRate;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Frame rate must be greater than 0.", nameof(value));
                m_frameRate = value;
                m_frameInterval = 1.0 / value;
            }
        }

        /// <summary>
        /// 获取或设置帧输入缓冲容量，默认 3。
        /// 决定客户端可提前发送的帧数。
        /// </summary>
        public int BufferSize
        {
            get => m_bufferSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Buffer size must be greater than 0.", nameof(value));
                m_bufferSize = value;
                m_maxCatchUpFrames = value * 3;
            }
        }

        /// <summary>
        /// 获取或设置是否启用帧同步。
        /// 关闭时 <see cref="Tick"/> 不驱动逻辑帧。
        /// </summary>
        public bool IsEnabled
        {
            get => m_isEnabled;
            set => m_isEnabled = value;
        }

        /// <summary>
        /// 每帧开始时触发，参数为当前帧号。
        /// 订阅者在此事件中执行确定性状态更新。
        /// </summary>
        public event Action<ulong> OnFrameStart;

        /// <summary>
        /// 创建帧同步管理器。
        /// </summary>
        /// <param name="frameRate">逻辑帧率（tick/s），默认 15</param>
        /// <param name="bufferSize">帧输入缓冲容量，默认 3</param>
        public LockstepManager(int frameRate = 15, int bufferSize = 3)
        {
            if (frameRate <= 0)
                throw new ArgumentException("Frame rate must be greater than 0.", nameof(frameRate));
            if (bufferSize <= 0)
                throw new ArgumentException("Buffer size must be greater than 0.", nameof(bufferSize));

            m_frameRate = frameRate;
            m_bufferSize = bufferSize;
            m_frameInterval = 1.0 / frameRate;
            m_maxCatchUpFrames = bufferSize * 3;
            m_currentFrame = 0;
            m_isEnabled = true;
            m_accumulatedTime = 0.0;

            m_inputBuffer = new FrameInputBuffer(bufferSize);
            m_checksumProvider = new XorChecksumProvider();
            m_checksumRegistry = new Dictionary<ulong, ulong>();
        }

        /// <summary>
        /// 每帧驱动逻辑帧更新。
        /// 使用累加器模式实现固定帧率：累加实际耗时直至达到帧间隔。
        /// </summary>
        /// <param name="deltaTime">本帧的实际耗时（秒）</param>
        public void Tick()
        {
            if (!m_isEnabled)
                return;

            m_accumulatedTime += HNLogicTime.DeltaTime;
            int framesProcessed = 0;

            while (m_accumulatedTime >= m_frameInterval)
            {
                m_accumulatedTime -= m_frameInterval;
                m_currentFrame++;

                // 更新 HNLogicTime
                HNLogicTime.Time += m_frameInterval;
                HNLogicTime.DeltaTime = m_frameInterval;
                HNLogicTime.LogicFrameCount = m_currentFrame;

                // 触发帧开始事件
                OnFrameStart?.Invoke(m_currentFrame);

                framesProcessed++;

                // 追帧保护：防止严重掉帧时无限追赶导致死亡螺旋
                if (framesProcessed >= m_maxCatchUpFrames)
                {
                    m_accumulatedTime = 0.0;
                    break;
                }
            }
        }

        /// <summary>
        /// 帧同步管理器不实现 LateTick 逻辑。
        /// </summary>
        public void LateTick()
        {
            // LockstepManager 不需要 LateTick 逻辑
        }

        /// <summary>
        /// 提交客户端输入到帧缓冲。
        /// 服务端模式下由 <see cref="LockstepNetworkDriver"/> 调用，
        /// 用于收集所有客户端的帧输入。
        /// </summary>
        /// <param name="clientId">客户端 ID（当前仅用于日志，缓冲按帧号存储）</param>
        /// <param name="input">帧输入数据</param>
        public void SubmitInput(int clientId, FrameInput input)
        {
            m_inputBuffer.Enqueue(input);
        }

        /// <summary>
        /// 尝试获取指定帧号的输入数据。
        /// 用于确定性 Tick 中的状态计算。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <param name="input">输出参数，帧输入数据</param>
        /// <returns>是否存在该帧的输入</returns>
        public bool TryGetInput(ulong frameNumber, out FrameInput input)
        {
            return m_inputBuffer.TryGet(frameNumber, out input);
        }

        /// <summary>
        /// 获取指定帧号的校验和。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <returns>该帧的校验和值，不存在时返回 0</returns>
        public ulong GetChecksum(ulong frameNumber)
        {
            if (m_checksumRegistry.TryGetValue(frameNumber, out ulong checksum))
                return checksum;
            return 0UL;
        }

        /// <summary>
        /// 注册指定帧号的校验和。
        /// 服务端计算后调用，供客户端比对。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <param name="checksum">校验和值</param>
        public void RegisterChecksum(ulong frameNumber, ulong checksum)
        {
            m_checksumRegistry[frameNumber] = checksum;
        }

        /// <summary>
        /// 重置帧同步管理器状态。包括帧号、时间累加器、输入缓冲和校验和注册表。
        /// </summary>
        public void Reset()
        {
            m_currentFrame = 0;
            m_accumulatedTime = 0.0;
            m_inputBuffer.Clear();
            m_checksumRegistry.Clear();
        }
    }
}