using System.Collections.Generic;
using FixedMathSharp;
using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// FrameInput 模块的 MemoryPack 格式化器注册入口。
    /// 调用 <see cref="RegisterAll"/> 注册所有格式化器以支持序列化。
    /// </summary>
    public static class FrameInputFormatters
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有 FrameInput 格式化器。幂等操作。
        /// 应在程序启动时（MemoryPack 初始化阶段）调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            MemoryPackFormatterProvider.Register(new FrameInputFormatter());
        }

        /// <summary>
        /// FrameInput 格式化器（值类型，序列化 FrameNumber + Actions 字典）。
        /// </summary>
        [Preserve]
        internal sealed class FrameInputFormatter : MemoryPackFormatter<FrameInput>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FrameInput value)
            {
                writer.WriteUnmanaged(value.FrameNumber);

                if (value.Actions == null)
                {
                    writer.WriteUnmanaged(0);
                    return;
                }

                writer.WriteUnmanaged(value.Actions.Count);
                foreach (var kvp in value.Actions)
                {
                    writer.WriteString(kvp.Key);
                    writer.WriteUnmanaged(kvp.Value.m_rawValue);
                }
            }

            public override void Deserialize(ref MemoryPackReader reader, ref FrameInput value)
            {
                reader.ReadUnmanaged(out ulong frameNumber);

                int count;
                reader.ReadUnmanaged(out count);

                var actions = new Dictionary<string, Fixed64>(count);
                for (int i = 0; i < count; i++)
                {
                    string key = reader.ReadString() ?? string.Empty;
                    long rawValue;
                    reader.ReadUnmanaged(out rawValue);
                    actions[key] = new Fixed64(rawValue);
                }

                value = new FrameInput(frameNumber) { Actions = actions };
            }
        }
    }
}
