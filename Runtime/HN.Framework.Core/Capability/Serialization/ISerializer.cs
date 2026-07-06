using System;

namespace HN.Framework.Core.Capability.Serialization
{
    /// <summary>
    /// 统一序列化接口，提供泛型和非泛型序列化/反序列化方法。
    /// </summary>
    /// <remarks>
    /// 默认实现应委托给 <c>global::MemoryPack.MemoryPackSerializer</c>：
    /// <list type="bullet">
    /// <item><description><c>Serialize&lt;T&gt;</c> → <c>global::MemoryPack.MemoryPackSerializer.Serialize(obj)</c></description></item>
    /// <item><description><c>Deserialize&lt;T&gt;</c> → <c>global::MemoryPack.MemoryPackSerializer.Deserialize&lt;T&gt;(data)</c></description></item>
    /// <item><description>非泛型 <c>Serialize</c> → <c>global::MemoryPack.MemoryPackSerializer.Serialize(type, obj)</c></description></item>
    /// <item><description>非泛型 <c>Deserialize</c> → <c>global::MemoryPack.MemoryPackSerializer.Deserialize(type, data)</c></description></item>
    /// </list>
    /// </remarks>
    public interface ISerializer
    {
        /// <summary>
        /// 将指定对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        byte[] Serialize<T>(T obj);

        /// <summary>
        /// 将字节数组反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="data">要反序列化的字节数组。</param>
        /// <returns>反序列化后的对象。</returns>
        T Deserialize<T>(byte[] data);

        /// <summary>
        /// 使用运行时类型信息将对象序列化为字节数组。
        /// </summary>
        /// <param name="type">要序列化的对象类型。</param>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        byte[] Serialize(Type type, object obj);

        /// <summary>
        /// 使用运行时类型信息将字节数组反序列化为对象。
        /// </summary>
        /// <param name="type">目标对象类型。</param>
        /// <param name="data">要反序列化的字节数组。</param>
        /// <returns>反序列化后的对象。</returns>
        object Deserialize(Type type, byte[] data);
    }
}
