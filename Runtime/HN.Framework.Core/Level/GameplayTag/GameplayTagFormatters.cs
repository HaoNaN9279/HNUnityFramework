#nullable enable

using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Level
{
    /// <summary>
    /// GameplayTag 模块的 MemoryPack 格式化器注册入口。
    /// 调用 <see cref="RegisterAll"/> 注册所有格式化器以支持序列化。
    /// </summary>
    public static class GameplayTagFormatters
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有 GameplayTag 格式化器。幂等操作。
        /// 应在程序启动时（MemoryPack 初始化阶段）调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered)
            {
                return;
            }

            _isRegistered = true;

            MemoryPackFormatterProvider.Register(new GameplayTagFormatter());
            MemoryPackFormatterProvider.Register(new TagQueryFormatter());
            MemoryPackFormatterProvider.Register(new GameplayTagContainerFormatter());
        }

        /// <summary>
        /// GameplayTag 格式化器（值类型，直接序列化 TableIndex 和 InstanceId）。
        /// </summary>
        [Preserve]
        internal sealed class GameplayTagFormatter : MemoryPackFormatter<GameplayTag>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref GameplayTag value)
            {
                writer.WriteUnmanaged(value.TableIndex);
                writer.WriteUnmanaged(value.InstanceId);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref GameplayTag value)
            {
                int tableIndex, instanceId;
                reader.ReadUnmanaged(out tableIndex);
                reader.ReadUnmanaged(out instanceId);
                value = new GameplayTag(tableIndex, instanceId);
            }
        }

        /// <summary>
        /// TagQuery 格式化器（引用类型，序列化扁平节点数组）。
        /// </summary>
        [Preserve]
        internal sealed class TagQueryFormatter : MemoryPackFormatter<TagQuery>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref TagQuery? value)
            {
                if (value == null)
                {
                    writer.WriteNullObjectHeader();
                    return;
                }

                var nodes = value.GetNodesForSerialization();
                writer.WriteObjectHeader(1);
                writer.WriteUnmanaged(nodes.Length);
                for (int i = 0; i < nodes.Length; i++)
                {
                    writer.WriteUnmanaged((byte)nodes[i].Type);
                    writer.WriteUnmanaged(nodes[i].Tag.TableIndex);
                    writer.WriteUnmanaged(nodes[i].Tag.InstanceId);
                    writer.WriteUnmanaged(nodes[i].ChildCount);
                }
            }

            public override void Deserialize(ref MemoryPackReader reader, ref TagQuery? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }

                FlatNode[] nodes = System.Array.Empty<FlatNode>();
                if (count >= 1)
                {
                    reader.ReadUnmanaged(out int length);
                    nodes = new FlatNode[length];
                    for (int i = 0; i < length; i++)
                    {
                        reader.ReadUnmanaged(out byte typeByte);
                        reader.ReadUnmanaged(out int tableIndex);
                        reader.ReadUnmanaged(out int instanceId);
                        reader.ReadUnmanaged(out int childCount);

                        nodes[i] = new FlatNode
                        {
                            Type = (QueryNodeType)typeByte,
                            Tag = new GameplayTag(tableIndex, instanceId),
                            ChildCount = childCount
                        };
                    }
                }

                value = TagQuery.CreateFromNodes(nodes);
            }
        }

        /// <summary>
        /// GameplayTagContainer 格式化器（引用类型，序列化内部标签 TableIndex 集）。
        /// </summary>
        [Preserve]
        internal sealed class GameplayTagContainerFormatter : MemoryPackFormatter<GameplayTagContainer>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref GameplayTagContainer? value)
            {
                if (value == null)
                {
                    writer.WriteNullObjectHeader();
                    return;
                }

                var tags = value.GetTagsForSerialization();
                writer.WriteObjectHeader(1);
                writer.WriteUnmanaged(tags.Length);
                foreach (var t in tags)
                {
                    writer.WriteUnmanaged(t);
                }
            }

            public override void Deserialize(ref MemoryPackReader reader, ref GameplayTagContainer? value)
            {
                if (!reader.TryReadObjectHeader(out byte count))
                {
                    value = null;
                    return;
                }

                int[] tags = System.Array.Empty<int>();
                if (count >= 1)
                {
                    reader.ReadUnmanaged(out int length);
                    tags = new int[length];
                    for (int i = 0; i < length; i++)
                    {
                        reader.ReadUnmanaged(out tags[i]);
                    }
                }

                value = new GameplayTagContainer();
                value.SetTagsFromDeserialization(tags);
            }
        }
    }
}
