using System.Collections.Generic;
using FixedMathSharp;
using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Capability.Network.Messages
{
    /// <summary>
    /// 消息类型格式化器注册器，为 MessageBase 子类手动注册 MemoryPack 格式化器。
    /// （MemoryPack 源生成器未在 asmdef 中配置时，需手动注册。）
    /// </summary>
    public static class MessageFormatters
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有消息类型的格式化器。幂等操作。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered)
                return;
            _isRegistered = true;

            MemoryPackFormatterProvider.Register(new ClientConnectedMessageFormatter());
            MemoryPackFormatterProvider.Register(new ClientDisconnectedMessageFormatter());
            MemoryPackFormatterProvider.Register(new ServerReadyMessageFormatter());
            MemoryPackFormatterProvider.Register(new FrameInputMessageFormatter());
            MemoryPackFormatterProvider.Register(new FrameDataMessageFormatter());
        }

        [Preserve]
        private sealed class ClientConnectedMessageFormatter : MemoryPackFormatter<ClientConnectedMessage>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClientConnectedMessage? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteObjectHeader(1);
                writer.WriteValue(value.ClientId);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref ClientConnectedMessage? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }
                int clientId = 0;
                if (count >= 1) clientId = reader.ReadValue<int>();
                value = new ClientConnectedMessage(clientId);
            }
        }

        [Preserve]
        private sealed class ClientDisconnectedMessageFormatter : MemoryPackFormatter<ClientDisconnectedMessage>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ClientDisconnectedMessage? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteObjectHeader(2);
                writer.WriteValue(value.ClientId);
                writer.WriteValue(value.Reason);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref ClientDisconnectedMessage? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }
                int clientId = 0;
                string reason = string.Empty;
                if (count >= 1) clientId = reader.ReadValue<int>();
                if (count >= 2) reason = reader.ReadString()!;
                value = new ClientDisconnectedMessage(clientId, reason);
            }
        }

        [Preserve]
        private sealed class ServerReadyMessageFormatter : MemoryPackFormatter<ServerReadyMessage>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref ServerReadyMessage? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteObjectHeader(0);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref ServerReadyMessage? value)
            {
                if (!reader.TryReadObjectHeader(out _))
                {
                    value = null;
                    return;
                }
                value = new ServerReadyMessage();
            }
        }

        [Preserve]
        private sealed class FrameInputMessageFormatter : MemoryPackFormatter<FrameInputMessage>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FrameInputMessage? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteObjectHeader(2);
                writer.WriteValue(value.ClientId);
                WriteFrameInput(ref writer, value.Input);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref FrameInputMessage? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }
                int clientId = 0;
                FrameInput input = default;
                if (count >= 1) clientId = reader.ReadValue<int>();
                if (count >= 2) input = ReadFrameInput(ref reader);
                value = new FrameInputMessage(clientId, input);
            }
        }

        [Preserve]
        private sealed class FrameDataMessageFormatter : MemoryPackFormatter<FrameDataMessage>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FrameDataMessage? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteObjectHeader(3);
                writer.WriteUnmanaged(value.FrameNumber);
                writer.WriteValue(value.Checksum);

                // 手动序列化 FrameInput 数组
                if (value.Inputs == null)
                {
                    writer.WriteUnmanaged(0);
                }
                else
                {
                    writer.WriteUnmanaged(value.Inputs.Length);
                    for (int i = 0; i < value.Inputs.Length; i++)
                    {
                        WriteFrameInput(ref writer, value.Inputs[i]);
                    }
                }
            }

            public override void Deserialize(ref MemoryPackReader reader, ref FrameDataMessage? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }
                ulong frameNumber = 0;
                ulong checksum = 0;
                FrameInput[] inputs = null;
                if (count >= 1) reader.ReadUnmanaged(out frameNumber);
                if (count >= 2) checksum = reader.ReadValue<ulong>();
                if (count >= 3)
                {
                    int length = 0;
                    reader.ReadUnmanaged(out length);
                    inputs = new FrameInput[length];
                    for (int i = 0; i < length; i++)
                    {
                        inputs[i] = ReadFrameInput(ref reader);
                    }
                }
                value = new FrameDataMessage(frameNumber, inputs, checksum);
            }
        }

        /// <summary>
        /// 辅助方法：写入 FrameInput 到 MemoryPack 流。
        /// [ulong FrameNumber, int actionsCount, (string key, long rawValue)[]...]
        /// </summary>
        private static void WriteFrameInput<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, FrameInput input)
            where TBufferWriter : class, System.Buffers.IBufferWriter<byte>
        {
            writer.WriteUnmanaged(input.FrameNumber);

            if (input.Actions == null)
            {
                writer.WriteUnmanaged(0);
                return;
            }

            writer.WriteUnmanaged(input.Actions.Count);
            foreach (var kvp in input.Actions)
            {
                writer.WriteValue(kvp.Key);
                writer.WriteUnmanaged(kvp.Value.m_rawValue);
            }
        }

        /// <summary>
        /// 辅助方法：从 MemoryPack 流读取 FrameInput。
        /// </summary>
        private static FrameInput ReadFrameInput(ref MemoryPackReader reader)
        {
            ulong frameNumber = 0;
            reader.ReadUnmanaged(out frameNumber);

            int count = 0;
            reader.ReadUnmanaged(out count);

            var actions = new Dictionary<string, Fixed64>(count);
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                long rawValue;
                reader.ReadUnmanaged(out rawValue);
                actions[key] = new Fixed64(rawValue);
            }

            return new FrameInput(frameNumber) { Actions = actions };
        }
    }
}
