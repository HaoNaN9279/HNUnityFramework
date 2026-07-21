namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 可插拔的校验和计算器接口。
    /// 用于帧同步中客户端与服务端的帧状态一致性比对。
    /// </summary>
    public interface IChecksumProvider
    {
        /// <summary>
        /// 计算指定数据的校验和。
        /// 相同输入必须产生相同输出（确定性）。
        /// </summary>
        /// <param name="data">要计算校验和的数据</param>
        /// <returns>64 位校验和值</returns>
        ulong Compute(byte[] data);

        /// <summary>
        /// 比对数据与期望的校验和是否一致。
        /// 等效于 <c>Compute(data) == expected</c>。
        /// </summary>
        /// <param name="data">要校验的数据</param>
        /// <param name="expected">期望的校验和值</param>
        /// <returns>是否匹配</returns>
        bool Compare(byte[] data, ulong expected);
    }

    /// <summary>
    /// 默认的 XOR 校验和实现。
    /// 按 8 字节分块，逐块 XOR 聚合为 64 位校验和。
    /// 轻量且确定性，适用于高频帧同步比对。
    /// </summary>
    public sealed class XorChecksumProvider : IChecksumProvider
    {
        /// <summary>
        /// 计算数据的 XOR 校验和。按 8 字节分块逐块 XOR 聚合。
        /// </summary>
        /// <param name="data">要计算校验和的数据</param>
        /// <returns>64 位校验和值</returns>
        public ulong Compute(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0UL;

            ulong checksum = 0UL;
            int i = 0;

            // 按 8 字节分块处理
            for (; i + 8 <= data.Length; i += 8)
            {
                ulong block = (ulong)data[i]
                    | ((ulong)data[i + 1] << 8)
                    | ((ulong)data[i + 2] << 16)
                    | ((ulong)data[i + 3] << 24)
                    | ((ulong)data[i + 4] << 32)
                    | ((ulong)data[i + 5] << 40)
                    | ((ulong)data[i + 6] << 48)
                    | ((ulong)data[i + 7] << 56);
                checksum ^= block;
            }

            // 处理剩余字节
            ulong remaining = 0UL;
            for (int r = 0; i < data.Length; i++, r++)
            {
                remaining |= (ulong)data[i] << (r * 8);
            }
            checksum ^= remaining;

            return checksum;
        }

        /// <summary>
        /// 比对数据与期望的校验和是否一致。
        /// </summary>
        /// <param name="data">要校验的数据</param>
        /// <param name="expected">期望的校验和值</param>
        /// <returns>是否匹配</returns>
        public bool Compare(byte[] data, ulong expected)
        {
            return Compute(data) == expected;
        }
    }
}