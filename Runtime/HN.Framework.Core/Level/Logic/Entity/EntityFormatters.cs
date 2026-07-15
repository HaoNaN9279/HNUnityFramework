using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Level.Logic.Entity
{
    /// <summary>
    /// Entity 模块的 MemoryPack 格式化器注册入口。
    /// 调用 <see cref="RegisterAll"/> 注册所有格式化器以支持序列化。
    /// </summary>
    public static class EntityFormatters
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有 Entity 格式化器。幂等操作。
        /// 应在程序启动时（MemoryPack 初始化阶段）调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            MemoryPackFormatterProvider.Register(new EntityFormatter());
        }

        /// <summary>
        /// Entity 格式化器（引用类型，序列化 EntityId/EntityDefId/OwnerClientId）。
        /// </summary>
        [Preserve]
        internal sealed class EntityFormatter : MemoryPackFormatter<Entity>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref Entity? value)
            {
                if (value == null)
                {
                    writer.WriteNullObjectHeader();
                    return;
                }

                writer.WriteObjectHeader(3);
                writer.WriteUnmanaged(value.EntityId);
                writer.WriteUnmanaged(value.EntityDefId);
                writer.WriteUnmanaged(value.OwnerClientId);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref Entity? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }

                uint entityId = 0;
                int entityDefId = 0;
                int ownerClientId = -1;

                if (count >= 1) reader.ReadUnmanaged(out entityId);
                if (count >= 2) reader.ReadUnmanaged(out entityDefId);
                if (count >= 3) reader.ReadUnmanaged(out ownerClientId);

                value = new Entity();
                value.Initialize(entityId, entityDefId, ownerClientId);
            }
        }
    }
}
