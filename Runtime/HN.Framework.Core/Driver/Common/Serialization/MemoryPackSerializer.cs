using System;
using System.IO;
using System.Threading.Tasks;

namespace HN.Framework.Core.Driver.Common.Serialization
{
    /// <summary>
    /// MemoryPack 序列化器，封装 vendored MemoryPack API，提供高性能二进制序列化与反序列化。
    /// </summary>
    public static class MemoryPackSerializer
    {
        /// <summary>
        /// 将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        public static byte[] Serialize<T>(T value)
        {
            return global::MemoryPack.MemoryPackSerializer.Serialize(value);
        }

        /// <summary>
        /// 将对象序列化为字节数组，使用指定的序列化选项。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">MemoryPack 序列化选项。</param>
        /// <returns>序列化后的字节数组。</returns>
        public static byte[] Serialize<T>(T value, global::MemoryPack.MemoryPackSerializerOptions? options)
        {
            return global::MemoryPack.MemoryPackSerializer.Serialize(value, options);
        }

        /// <summary>
        /// 将对象序列化到指定流。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="stream">目标流。</param>
        /// <param name="value">要序列化的对象。</param>
        public static void Serialize<T>(Stream stream, T value)
        {
            global::MemoryPack.MemoryPackSerializer.SerializeAsync<T>(stream, value)
                .AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 将对象序列化到指定流，使用指定的序列化选项。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="stream">目标流。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">MemoryPack 序列化选项。</param>
        public static void Serialize<T>(Stream stream, T value, global::MemoryPack.MemoryPackSerializerOptions? options)
        {
            global::MemoryPack.MemoryPackSerializer.SerializeAsync<T>(stream, value, options)
                .AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从字节数组反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="data">包含序列化数据的字节数组。</param>
        /// <returns>反序列化后的对象，数据为 null 时返回 default(T)。</returns>
        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default;

            return global::MemoryPack.MemoryPackSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// 从字节数组反序列化为指定类型的对象，使用指定的序列化选项。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="data">包含序列化数据的字节数组。</param>
        /// <param name="options">MemoryPack 序列化选项。</param>
        /// <returns>反序列化后的对象，数据为 null 时返回 default(T)。</returns>
        public static T Deserialize<T>(byte[] data, global::MemoryPack.MemoryPackSerializerOptions? options)
        {
            if (data == null)
                return default;

            return global::MemoryPack.MemoryPackSerializer.Deserialize<T>(data, options);
        }

        /// <summary>
        /// 从流中反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="stream">包含序列化数据的流。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T Deserialize<T>(Stream stream)
        {
            return global::MemoryPack.MemoryPackSerializer.DeserializeAsync<T>(stream)
                .AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从流中反序列化为指定类型的对象，使用指定的序列化选项。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="stream">包含序列化数据的流。</param>
        /// <param name="options">MemoryPack 序列化选项。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T Deserialize<T>(Stream stream, global::MemoryPack.MemoryPackSerializerOptions? options)
        {
            return global::MemoryPack.MemoryPackSerializer.DeserializeAsync<T>(stream, options)
                .AsTask().GetAwaiter().GetResult();
        }
    }
}
