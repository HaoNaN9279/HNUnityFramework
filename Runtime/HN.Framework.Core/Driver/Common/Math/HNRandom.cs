using System;

namespace HN.Framework.Core.Driver.Common.Math
{
    /// <summary>
    /// 确定性伪随机数生成器，基于 xorshift128+ 算法。
    /// 提供跨平台、跨 .NET 版本一致的随机数序列，支持种子设置与状态序列化。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 与 <see cref="System.Random"/> 不同，<see cref="HNRandom"/> 的实现在所有
    /// .NET 版本和平台上完全一致，适合需要确定性随机数的场景（回放系统、网络同步、自动化测试等）。
    /// </para>
    /// <para>
    /// 算法基于 Sebastiano Vigna 的 xorshift128+，周期为 2^128 - 1，生成速度快且通过
    /// BigCrush 统计测试。
    /// </para>
    /// <para>此类不是线程安全的。每个线程应使用独立的实例。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 使用相同种子创建两个实例，它们将产生完全相同的序列
    /// var rng1 = new HNRandom(12345UL);
    /// var rng2 = new HNRandom(12345UL);
    /// Assert.AreEqual(rng1.Next(), rng2.Next()); // true
    /// </code>
    /// </example>
    public class HNRandom
    {
        // xorshift128+ 内部状态
        private ulong m_S0;
        private ulong m_S1;

        /// <summary>
        /// 使用随机种子初始化生成器。
        /// </summary>
        public HNRandom()
            : this((ulong)Environment.TickCount ^ ((ulong)Guid.NewGuid().GetHashCode() << 32))
        {
        }

        /// <summary>
        /// 使用指定种子初始化生成器。
        /// 相同的种子始终产生相同的随机数序列。
        /// </summary>
        /// <param name="seed">初始种子值。</param>
        public HNRandom(ulong seed)
        {
            // 使用 SplitMix64 将单个种子扩展为两个 64 位状态值
            m_S0 = SplitMix64(ref seed);
            m_S1 = SplitMix64(ref seed);

            // 确保状态不全为零（xorshift 要求非零状态）
            if (m_S0 == 0 && m_S1 == 0)
            {
                m_S0 = 1;
            }
        }

        /// <summary>
        /// 使用两个 64 位值直接初始化状态。
        /// 用于从之前序列化的状态恢复。
        /// </summary>
        /// <param name="s0">第一个状态值。</param>
        /// <param name="s1">第二个状态值。</param>
        /// <exception cref="ArgumentException">当两个状态值同时为零时抛出。</exception>
        public HNRandom(ulong s0, ulong s1)
        {
            if (s0 == 0 && s1 == 0)
            {
                throw new ArgumentException("xorshift128+ 状态不能同时为零。");
            }

            m_S0 = s0;
            m_S1 = s1;
        }

        /// <summary>
        /// 生成一个非负随机整数（范围 0 到 <see cref="int.MaxValue"/>）。
        /// </summary>
        /// <returns>非负随机整数。</returns>
        public int Next()
        {
            return (int)(NextUInt64() & 0x7FFFFFFF);
        }

        /// <summary>
        /// 生成一个小于指定最大值的非负随机整数。
        /// </summary>
        /// <param name="maxValue">随机数的上限（不包含）。必须大于 0。</param>
        /// <returns>范围 [0, maxValue) 内的随机整数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 maxValue 小于或等于 0 时抛出。</exception>
        public int Next(int maxValue)
        {
            if (maxValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue 必须大于 0。");
            }

            return Next(0, maxValue);
        }

        /// <summary>
        /// 生成一个在指定范围内的随机整数。
        /// </summary>
        /// <param name="minValue">随机数的下限（包含）。</param>
        /// <param name="maxValue">随机数的上限（不包含）。必须大于 minValue。</param>
        /// <returns>范围 [minValue, maxValue) 内的随机整数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 minValue 大于或等于 maxValue 时抛出。</exception>
        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue 必须小于 maxValue。");
            }

            long range = (long)maxValue - minValue;

            // 使用拒绝采样避免模偏差
            ulong limit = (ulong.MaxValue / (ulong)range) * (ulong)range;
            ulong value;
            do
            {
                value = NextUInt64();
            } while (value >= limit);

            return (int)((long)(value % (ulong)range) + minValue);
        }

        /// <summary>
        /// 生成一个范围 [0.0, 1.0) 内的随机双精度浮点数。
        /// </summary>
        /// <returns>范围 [0.0, 1.0) 内的随机双精度浮点数。</returns>
        public double NextDouble()
        {
            // 使用高 53 位生成 double（IEEE 754 尾数位数为 52）
            return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>
        /// 生成一个范围 [0.0f, 1.0f) 内的随机单精度浮点数。
        /// </summary>
        /// <returns>范围 [0.0f, 1.0f) 内的随机单精度浮点数。</returns>
        public float NextFloat()
        {
            // 使用高 23 位生成 float（IEEE 754 尾数位数为 23）
            return (NextUInt64() >> 40) * (1.0f / (1U << 24));
        }

        /// <summary>
        /// 生成原始的 64 位无符号随机整数。
        /// </summary>
        /// <returns>随机的 64 位无符号整数。</returns>
        public ulong NextUInt64()
        {
            ulong s0 = m_S0;
            ulong s1 = m_S1;

            // xorshift128+ 核心算法
            ulong result = s0 + s1;

            s1 ^= s1 << 23;
            s1 ^= s1 >> 17;
            s1 ^= s0;
            s1 ^= s0 >> 26;

            m_S0 = s1;
            m_S1 = s0;

            return result;
        }

        /// <summary>
        /// 获取当前内部状态，用于序列化或保存。
        /// </summary>
        /// <returns>包含两个状态值的元组 (s0, s1)。</returns>
        public (ulong S0, ulong S1) GetState()
        {
            return (m_S0, m_S1);
        }

        /// <summary>
        /// 从之前保存的状态恢复生成器。
        /// </summary>
        /// <param name="s0">第一个状态值。</param>
        /// <param name="s1">第二个状态值。</param>
        /// <exception cref="ArgumentException">当两个状态值同时为零时抛出。</exception>
        public void SetState(ulong s0, ulong s1)
        {
            if (s0 == 0 && s1 == 0)
            {
                throw new ArgumentException("xorshift128+ 状态不能同时为零。");
            }

            m_S0 = s0;
            m_S1 = s1;
        }

        /// <summary>
        /// SplitMix64 算法，用于从单个种子生成高质量的 64 位值。
        /// </summary>
        /// <param name="state">当前状态引用，会被更新。</param>
        /// <returns>生成的 64 位随机值。</returns>
        private static ulong SplitMix64(ref ulong state)
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
