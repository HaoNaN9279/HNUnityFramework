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
    }
}
