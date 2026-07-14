#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HN.Framework.Core.Capability.Localization;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 本地化管理器，实现 <see cref="ILocaleProvider"/> 接口。
    /// 管理多语言字符串表、语言切换和本地化事件分发。
    /// 由 <c>GameWorldDriver</c> 初始化时构造并注入 <c>GameWorld</c>。
    /// 不实现 <c>ITickable</c>，语言切换为事件驱动。
    /// </summary>
    public sealed class LocaleManager : ILocaleProvider, IDisposable
    {
        private readonly Dictionary<string, StringTable> _tables;
        private readonly ILocaleDataLoader _loader;
        private readonly LocaleSelector _selector;
        private readonly Locale _fallbackLocale;

        private Locale _currentLocale;

        /// <summary>
        /// 初始化本地化管理器。
        /// </summary>
        /// <param name="loader">本地化数据加载器，负责从数据源加载字符串表。</param>
        /// <param name="availableLocales">用户可选的可用语言列表。</param>
        /// <param name="fallbackLocale">默认回退语言，当已保存偏好无效时使用。</param>
        /// <exception cref="ArgumentNullException"><paramref name="loader"/> 或 <paramref name="availableLocales"/> 为 <c>null</c>。</exception>
        public LocaleManager(
            ILocaleDataLoader loader,
            IReadOnlyList<Locale> availableLocales,
            Locale fallbackLocale)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _fallbackLocale = fallbackLocale;
            _selector = new LocaleSelector(availableLocales);
            _tables = new Dictionary<string, StringTable>();
            _currentLocale = _selector.GetSavedLocale(fallbackLocale);
        }

        // --- ILocaleProvider 实现 ---

        /// <inheritdoc/>
        public event Action<Locale>? OnLocaleChanged;

        /// <inheritdoc/>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key ?? string.Empty;
            }

            // 优先查找当前语言
            if (_tables.TryGetValue(_currentLocale.Code, out var table))
            {
                if (table.TryGetString(key, out var value))
                {
                    return value;
                }
            }

            // 当前语言未命中，尝试回退语言
            if (_currentLocale.Code != _fallbackLocale.Code
                && _tables.TryGetValue(_fallbackLocale.Code, out var fallbackTable))
            {
                if (fallbackTable.TryGetString(key, out var fallbackValue))
                {
                    return fallbackValue;
                }
            }

            // 所有表都未命中，返回键本身（优雅降级）
            return key;
        }

        /// <inheritdoc/>
        public Locale GetCurrentLocale()
        {
            return _currentLocale;
        }

        /// <inheritdoc/>
        public async void SetLocale(Locale locale)
        {
            if (_currentLocale == locale)
            {
                return;
            }

            // 如果该语言的 StringTable 尚未加载，先异步加载
            if (!_tables.ContainsKey(locale.Code))
            {
                var table = await _loader.LoadTableAsync(locale);
                _tables[locale.Code] = table;
            }

            _currentLocale = locale;
            _selector.SaveLocale(locale);
            OnLocaleChanged?.Invoke(locale);
        }

        // --- 预加载 ---

        /// <summary>
        /// 预加载指定语言区域的字符串表。
        /// 如果该表已经加载，则立即返回。
        /// </summary>
        /// <param name="locale">要预加载的语言区域。</param>
        /// <returns>表示异步预加载操作的任务。</returns>
        public async Task PreloadTableAsync(Locale locale)
        {
            if (!_tables.ContainsKey(locale.Code))
            {
                var table = await _loader.LoadTableAsync(locale);
                _tables[locale.Code] = table;
            }
        }

        /// <summary>
        /// 预加载所有可用语言的字符串表。
        /// 多个语言的加载任务将并行执行。
        /// </summary>
        /// <returns>表示所有预加载操作完成的任务。</returns>
        public async Task PreloadAllTablesAsync()
        {
            var locales = _selector.AvailableLocales;
            var tasks = new Task[locales.Count];
            for (var i = 0; i < locales.Count; i++)
            {
                tasks[i] = PreloadTableAsync(locales[i]);
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 获取当前语言选择器，提供可用语言列表和持久化偏好查询能力。
        /// </summary>
        public LocaleSelector Selector => _selector;

        /// <summary>
        /// 释放本地化管理器持有的资源。
        /// 清理所有已加载的字符串表，并释放数据加载器（如果它实现了 <see cref="IDisposable"/>）。
        /// </summary>
        public void Dispose()
        {
            _tables.Clear();
            (_loader as IDisposable)?.Dispose();
        }
    }
}
