using System;
using System.Collections.Concurrent;

namespace HN.Framework.Core.Capability.Serialization
{
    /// <summary>
    /// MemoryPack 格式化器注册提供器，提供对 <see cref="global::MemoryPack.MemoryPackFormatterProvider"/>
    /// 的框架友好封装，支持泛型和非泛型的格式化器注册与查找。
    /// </summary>
    /// <remarks>
    /// <para>由于 <c>global::MemoryPack.MemoryPackFormatterProvider</c> 的
    /// <c>GetFormatter</c> 方法为 internal，此类维护独立的格式化器注册表。</para>
    /// <para><c>Register&lt;T&gt;</c> 同时向 vendor 的
    /// <c>global::MemoryPack.MemoryPackFormatterProvider.Register&lt;T&gt;</c>
    /// 委托注册，确保 <c>global::MemoryPack.MemoryPackSerializer</c> 可直接解析已注册的格式化器。</para>
    /// </remarks>
    public static class MemoryPackFormatterProvider
    {
        /// <summary>
        /// 格式化器注册表，键为类型，值为对应的格式化器实例。
        /// </summary>
        private static readonly ConcurrentDictionary<Type, object> _formatters =
            new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 注册指定类型的格式化器，同时委托给 vendor 的 MemoryPackFormatterProvider。
        /// </summary>
        /// <typeparam name="T">格式化器对应的目标类型。</typeparam>
        /// <param name="formatter">
        /// 要注册的格式化器实例，必须是 <see cref="global::MemoryPack.MemoryPackFormatter{T}"/> 的子类。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="formatter"/> 为 null。</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="formatter"/> 不是 <see cref="global::MemoryPack.MemoryPackFormatter{T}"/> 的实例。
        /// </exception>
        public static void Register<T>(global::MemoryPack.IMemoryPackFormatter<T> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (formatter is global::MemoryPack.MemoryPackFormatter<T> typedFormatter)
            {
                global::MemoryPack.MemoryPackFormatterProvider.Register(typedFormatter);
                _formatters[typeof(T)] = formatter;
            }
            else
            {
                throw new ArgumentException(
                    $"Formatter for type {typeof(T).FullName} must inherit from MemoryPack.MemoryPackFormatter<{typeof(T).Name}>.",
                    nameof(formatter));
            }
        }

        /// <summary>
        /// 获取指定类型已注册的格式化器。
        /// </summary>
        /// <typeparam name="T">要查找格式化器的目标类型。</typeparam>
        /// <returns>已注册的格式化器实例。</returns>
        /// <exception cref="InvalidOperationException">指定类型的格式化器未注册。</exception>
        public static global::MemoryPack.IMemoryPackFormatter<T> GetFormatter<T>()
        {
            if (_formatters.TryGetValue(typeof(T), out var formatter)
                && formatter is global::MemoryPack.IMemoryPackFormatter<T> typedFormatter)
            {
                return typedFormatter;
            }

            throw new InvalidOperationException(
                $"No formatter registered for type {typeof(T).FullName}. " +
                $"Call Register<{typeof(T).Name}>() first.");
        }

        /// <summary>
        /// 使用运行时类型信息注册格式化器。
        /// </summary>
        /// <param name="type">格式化器对应的目标类型。</param>
        /// <param name="formatter">
        /// 要注册的格式化器实例，必须实现 <see cref="global::MemoryPack.IMemoryPackFormatter"/>。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> 或 <paramref name="formatter"/> 为 null。</exception>
        /// <exception cref="ArgumentException"><paramref name="formatter"/> 不是有效的 MemoryPack 格式化器。</exception>
        /// <remarks>
        /// 此方法仅将格式化器记录到内部注册表。若需通过
        /// <c>global::MemoryPack.MemoryPackSerializer</c> 使用该格式化器，
        /// 请优先使用泛型 <see cref="Register{T}"/> 方法。
        /// </remarks>
        public static void Register(Type type, object formatter)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!(formatter is global::MemoryPack.IMemoryPackFormatter))
            {
                throw new ArgumentException(
                    $"Formatter for type {type.FullName} must implement MemoryPack.IMemoryPackFormatter.",
                    nameof(formatter));
            }

            _formatters[type] = formatter;
        }

        /// <summary>
        /// 使用运行时类型信息获取已注册的格式化器。
        /// </summary>
        /// <param name="type">要查找格式化器的目标类型。</param>
        /// <returns>已注册的格式化器实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException">指定类型的格式化器未注册。</exception>
        public static object GetFormatter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_formatters.TryGetValue(type, out var formatter))
            {
                return formatter;
            }

            throw new InvalidOperationException(
                $"No formatter registered for type {type.FullName}. " +
                $"Call Register() first.");
        }
    }
}
