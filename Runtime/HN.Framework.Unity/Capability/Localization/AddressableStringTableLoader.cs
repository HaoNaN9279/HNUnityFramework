#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HN.Framework.Core.Capability.Localization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 基于 Addressables 的本地化字符串表加载器。
    /// 从 Addressables 加载 JSON 格式的 <see cref="TextAsset"/>，解析为 <see cref="StringTable"/>。
    /// </summary>
    /// <remarks>
    /// <para>Addressables key 规则：<c>{baseKey}_{locale.Code}</c>（如 <c>"locale_zh-CN"</c>）。</para>
    /// <para>JSON 格式要求：</para>
    /// <code>
    /// {"entries": [{"key": "hello", "value": "你好"}, {"key": "bye", "value": "再见"}]}
    /// </code>
    /// <para>使用 <see cref="UnityEngine.JsonUtility"/> 进行反序列化，无需第三方 JSON 库。</para>
    /// </remarks>
    public sealed class AddressableStringTableLoader : ILocaleDataLoader, IDisposable
    {
        private readonly string _baseKey;
        private readonly Dictionary<string, AsyncOperationHandle<TextAsset>> _pendingHandles;
        private readonly HashSet<string> _registeredLocales;
        private readonly object _lock = new();

        /// <summary>
        /// 使用指定的 Addressables key 前缀初始化加载器。
        /// </summary>
        /// <param name="baseKey">
        /// Addressables key 前缀。最终 key 为 <c>{baseKey}_{locale.Code}</c>。
        /// 默认值为 <c>"locale"</c>。
        /// </param>
        public AddressableStringTableLoader(string baseKey = "locale")
        {
            _baseKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));
            _pendingHandles = new Dictionary<string, AsyncOperationHandle<TextAsset>>();
            _registeredLocales = new HashSet<string>();
        }

        /// <inheritdoc/>
        public async Task<StringTable> LoadTableAsync(Locale locale, CancellationToken cancellationToken = default)
        {
            var addressKey = GetAddressKey(locale);
            var handle = Addressables.LoadAssetAsync<TextAsset>(addressKey);

            lock (_lock)
            {
                _pendingHandles[locale.Code] = handle;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var textAsset = await handle.Task;

                cancellationToken.ThrowIfCancellationRequested();

                if (textAsset == null)
                {
                    UnityEngine.Debug.LogWarning($"[AddressableStringTableLoader] 加载的 TextAsset 为 null: {addressKey}");
                    return new StringTable();
                }

                var table = ParseJson(textAsset.text);

                lock (_lock)
                {
                    _registeredLocales.Add(locale.Code);
                }

                return table;
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.Log($"[AddressableStringTableLoader] 加载已取消: {locale.Code}");
                return new StringTable();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[AddressableStringTableLoader] 加载本地化表失败: {locale.Code}, 错误: {ex.Message}");
                return new StringTable();
            }
            finally
            {
                lock (_lock)
                {
                    _pendingHandles.Remove(locale.Code);
                }
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 检查该语言区域是否已通过 <see cref="RegisterLocale"/> 注册，
        /// 或之前已成功加载过。
        /// </remarks>
        public bool IsAvailable(Locale locale)
        {
            lock (_lock)
            {
                return _registeredLocales.Contains(locale.Code);
            }
        }

        /// <summary>
        /// 注册一个已知可用的语言区域。
        /// 注册后 <see cref="IsAvailable"/> 将返回 <c>true</c>。
        /// 也可在成功加载后自动注册。
        /// </summary>
        /// <param name="locale">要注册的语言区域。</param>
        public void RegisterLocale(Locale locale)
        {
            lock (_lock)
            {
                _registeredLocales.Add(locale.Code);
            }
        }

        /// <summary>
        /// 注册多个已知可用的语言区域。
        /// </summary>
        /// <param name="locales">要注册的语言区域集合。</param>
        public void RegisterLocales(IEnumerable<Locale> locales)
        {
            if (locales == null)
            {
                throw new ArgumentNullException(nameof(locales));
            }

            lock (_lock)
            {
                foreach (var locale in locales)
                {
                    _registeredLocales.Add(locale.Code);
                }
            }
        }

        /// <summary>
        /// 释放所有未完成的 Addressables 加载句柄，防止资源泄漏。
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var handle in _pendingHandles.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }

                _pendingHandles.Clear();
                _registeredLocales.Clear();
            }
        }

        /// <summary>
        /// 构建 Addressables key。
        /// </summary>
        private string GetAddressKey(Locale locale)
        {
            return $"{_baseKey}_{locale.Code}";
        }

        /// <summary>
        /// 将 JSON 文本解析为 <see cref="StringTable"/>。
        /// 使用 <see cref="JsonUtility"/> 反序列化包装类，兼容所有 Unity 平台。
        /// </summary>
        private static StringTable ParseJson(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<JsonTableWrapper>(json);

                if (wrapper == null || wrapper.entries == null || wrapper.entries.Count == 0)
                {
                    return new StringTable();
                }

                var entries = new Dictionary<string, string>(wrapper.entries.Count);
                foreach (var entry in wrapper.entries)
                {
                    if (!string.IsNullOrEmpty(entry.key))
                    {
                        entries[entry.key] = entry.value ?? string.Empty;
                    }
                }

                return new StringTable(entries);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableStringTableLoader] JSON 解析失败: {ex.Message}");
                return new StringTable();
            }
        }

        /// <summary>
        /// JSON 表条目：单个键值对。
        /// </summary>
        [Serializable]
        private sealed class JsonTableEntry
        {
            /// <summary>本地化键。</summary>
            public string key = string.Empty;

            /// <summary>本地化文本。</summary>
            public string value = string.Empty;
        }

        /// <summary>
        /// JSON 表包装类，包含条目列表。供 <see cref="JsonUtility"/> 反序列化使用。
        /// </summary>
        [Serializable]
        private sealed class JsonTableWrapper
        {
            /// <summary>本地化条目列表。</summary>
            public List<JsonTableEntry> entries = new();
        }
    }
}
