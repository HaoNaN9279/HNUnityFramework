using global::MemoryPack;
using global::MemoryPack.Internal;

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// AssetRef 模块的 MemoryPack 格式化器注册入口。
    /// 使用 <see cref="MemoryPackFormatterProvider.RegisterGenericType"/> 为开放泛型
    /// <see cref="AssetRef{T}"/> 注册格式化器，一次性覆盖所有封闭类型。
    /// 调用 <see cref="RegisterAll"/> 注册所有格式化器以支持序列化。
    /// </summary>
    public static class AssetRefFormatters
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有 AssetRef 格式化器。幂等操作。
        /// 应在程序启动时（MemoryPack 初始化阶段）调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            MemoryPackFormatterProvider.RegisterGenericType(
                typeof(AssetRef<>),
                typeof(AssetRefFormatter<>));
        }

        /// <summary>
        /// AssetRef 格式化器（开放泛型，序列化 Label 字符串）。
        /// </summary>
        [Preserve]
        internal sealed class AssetRefFormatter<T> : MemoryPackFormatter<AssetRef<T>>
        {
            public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AssetRef<T> value)
            {
                writer.WriteString(value.Label);
            }

            public override void Deserialize(ref MemoryPackReader reader, ref AssetRef<T> value)
            {
                value.Label = reader.ReadString() ?? string.Empty;
            }
        }
    }
}
