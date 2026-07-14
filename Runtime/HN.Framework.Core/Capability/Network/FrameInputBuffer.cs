using System;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 帧输入环形缓冲区，按帧号存储 <see cref="FrameInput"/>。
    /// 固定容量，入队超出容量时覆盖最旧数据。
    /// 提供 O(1) 的按帧号查询（基于顺序帧号假设）和过期清理。
    /// </summary>
    public class FrameInputBuffer
    {
        private readonly FrameInput[] m_buffer;
        private int m_head;
        private int m_tail;
        private int m_count;

        /// <summary>
        /// 获取缓冲区容量。
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// 获取当前元素数量。
        /// </summary>
        public int Count => m_count;

        /// <summary>
        /// 获取缓冲区是否已满。
        /// </summary>
        public bool IsFull => m_count >= Capacity;

        /// <summary>
        /// 创建指定容量的帧输入环形缓冲区。
        /// </summary>
        /// <param name="capacity">缓冲区容量，必须大于 0</param>
        /// <exception cref="ArgumentException">capacity 小于等于 0 时抛出</exception>
        public FrameInputBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));

            Capacity = capacity;
            m_buffer = new FrameInput[capacity];
            m_head = 0;
            m_tail = 0;
            m_count = 0;
        }

        /// <summary>
        /// 入队一个帧输入。如果缓冲区已满，覆盖最旧的帧。
        /// </summary>
        /// <param name="input">帧输入数据</param>
        public void Enqueue(FrameInput input)
        {
            m_buffer[m_tail] = input;
            m_tail = (m_tail + 1) % Capacity;

            if (m_count >= Capacity)
            {
                m_head = (m_head + 1) % Capacity;
            }
            else
            {
                m_count++;
            }
        }

        /// <summary>
        /// 尝试获取指定帧号的帧输入。
        /// 基于顺序帧号假设（无间隔递增），计算 O(1) 索引。
        /// </summary>
        /// <param name="frameNumber">目标帧号</param>
        /// <param name="input">输出参数，帧输入数据</param>
        /// <returns>是否存在该帧的输入</returns>
        public bool TryGet(ulong frameNumber, out FrameInput input)
        {
            if (m_count == 0)
            {
                input = default;
                return false;
            }

            ulong oldestFrame = m_buffer[m_head].FrameNumber;
            ulong newestFrame = m_buffer[(m_tail - 1 + Capacity) % Capacity].FrameNumber;

            if (frameNumber < oldestFrame || frameNumber > newestFrame)
            {
                input = default;
                return false;
            }

            int offset = (int)(frameNumber - oldestFrame);
            int index = (m_head + offset) % Capacity;
            input = m_buffer[index];
            return true;
        }

        /// <summary>
        /// 尝试获取最新的帧输入（即最后一个入队的帧）。
        /// </summary>
        /// <param name="input">输出参数，最新的帧输入</param>
        /// <returns>缓冲区非空时返回 true</returns>
        public bool TryGetLatest(out FrameInput input)
        {
            if (m_count <= 0)
            {
                input = default;
                return false;
            }

            int latestIndex = (m_tail - 1 + Capacity) % Capacity;
            input = m_buffer[latestIndex];
            return true;
        }

        /// <summary>
        /// 清理指定帧号之前的所有帧数据（含目标帧号）。
        /// </summary>
        /// <param name="frameNumber">清理的截止帧号（含）</param>
        public void ClearBefore(ulong frameNumber)
        {
            while (m_count > 0)
            {
                int index = m_head % Capacity;
                if (m_buffer[index].FrameNumber <= frameNumber)
                {
                    m_head = (m_head + 1) % Capacity;
                    m_count--;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
        public void Clear()
        {
            m_head = 0;
            m_tail = 0;
            m_count = 0;
            Array.Clear(m_buffer, 0, Capacity);
        }
    }
}