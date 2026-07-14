#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Unity.Capability.Localization;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Localization
{
    /// <summary>
    /// 用于测试 <see cref="LocaleManager"/> 的模拟数据加载器。
    /// 返回预先设置的字符串表，不依赖外部数据源。
    /// </summary>
    internal class MockLocaleDataLoader : ILocaleDataLoader
    {
        private readonly Dictionary<string, StringTable> _tables = new();

        public void AddTable(Locale locale, StringTable table)
        {
            _tables[locale.Code] = table;
        }

        public Task<StringTable> LoadTableAsync(Locale locale, CancellationToken cancellationToken = default)
        {
            if (_tables.TryGetValue(locale.Code, out var table))
                return Task.FromResult(table);
            return Task.FromResult(new StringTable());
        }

        public bool IsAvailable(Locale locale)
        {
            return _tables.ContainsKey(locale.Code);
        }
    }

    /// <summary>
    /// <see cref="LocaleManager"/> 的 EditMode 单元测试。
    /// 覆盖字符串查询、语言切换、事件分发、预加载、资源释放。
    /// </summary>
    [TestFixture]
    public class LocaleManagerTests
    {
        private MockLocaleDataLoader _loader = null!;
        private IReadOnlyList<Locale> _availableLocales = null!;
        private LocaleManager _manager = null!;

        [SetUp]
        public void SetUp()
        {
            _loader = new MockLocaleDataLoader();
            _availableLocales = new List<Locale> { Locale.zhCN, Locale.enUS };
            PlayerPrefs.DeleteKey("HN_Framework_Locale");

            // 预设测试数据
            var zhTable = new StringTable(new Dictionary<string, string>
            {
                { "hello", "你好" },
                { "bye", "再见" },
                { "only_zh", "仅中文" }
            });
            var enTable = new StringTable(new Dictionary<string, string>
            {
                { "hello", "Hello" },
                { "bye", "Goodbye" },
                { "only_en", "English Only" }
            });
            _loader.AddTable(Locale.zhCN, zhTable);
            _loader.AddTable(Locale.enUS, enTable);

            _manager = new LocaleManager(_loader, _availableLocales, Locale.zhCN);
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Dispose();
            PlayerPrefs.DeleteKey("HN_Framework_Locale");
        }

        [Test]
        public void GetCurrentLocale_Initialized_ReturnsFallback()
        {
            Assert.AreEqual(Locale.zhCN, _manager.GetCurrentLocale());
        }

        [Test]
        public void GetString_ExistingKey_ReturnsLocalizedText()
        {
            // 先加载默认语言的表
            var loadTask = _manager.PreloadTableAsync(Locale.zhCN);
            loadTask.Wait();

            var result = _manager.GetString("hello");
            Assert.AreEqual("你好", result);
        }

        [Test]
        public void GetString_MissingKey_ReturnsKeyItself()
        {
            var result = _manager.GetString("nonexistent");
            Assert.AreEqual("nonexistent", result);
        }

        [Test]
        public void GetString_NullKey_ReturnsEmptyString()
        {
            var result = _manager.GetString(null!);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void GetString_EmptyKey_ReturnsEmptyString()
        {
            var result = _manager.GetString(string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void SetLocale_TriggersOnLocaleChanged()
        {
            // 预加载目标语言表，使 SetLocale 同步完成
            var preloadTask = _manager.PreloadTableAsync(Locale.enUS);
            preloadTask.Wait();

            Locale? changedLocale = null;
            _manager.OnLocaleChanged += (locale) => changedLocale = locale;

            _manager.SetLocale(Locale.enUS);

            Assert.AreEqual(Locale.enUS, changedLocale);
            Assert.AreEqual(Locale.enUS, _manager.GetCurrentLocale());
        }

        [Test]
        public void SetLocale_SameLocale_DoesNotTriggerEvent()
        {
            var callCount = 0;
            _manager.OnLocaleChanged += (_) => callCount++;

            _manager.SetLocale(Locale.zhCN);

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void GetString_FallsBackToFallbackLocale_WhenCurrentLocaleMissing()
        {
            // 创建仅 zhCN 有特定键的场景
            var customLoader = new MockLocaleDataLoader();
            customLoader.AddTable(Locale.zhCN, new StringTable(new Dictionary<string, string>
            {
                { "fallback_key", "回退值" }
            }));
            customLoader.AddTable(Locale.enUS, new StringTable(new Dictionary<string, string>
            {
                { "en_only", "English Only" }
            }));

            using var mgr = new LocaleManager(customLoader,
                new List<Locale> { Locale.zhCN, Locale.enUS }, Locale.zhCN);

            // 预加载所有表
            mgr.PreloadTableAsync(Locale.zhCN).Wait();
            mgr.PreloadTableAsync(Locale.enUS).Wait();

            // 切换到 enUS
            mgr.SetLocale(Locale.enUS);

            // "fallback_key" 不在 enUS 中，应回退到 zhCN
            Assert.AreEqual("回退值", mgr.GetString("fallback_key"));
            // "en_only" 在 enUS 中，直接返回
            Assert.AreEqual("English Only", mgr.GetString("en_only"));
        }

        [Test]
        public void SetLocale_UpdatesPersistedPreference()
        {
            var preloadTask = _manager.PreloadTableAsync(Locale.enUS);
            preloadTask.Wait();

            _manager.SetLocale(Locale.enUS);

            // 验证 PlayerPrefs 已持久化
            Assert.AreEqual("en-US", PlayerPrefs.GetString("HN_Framework_Locale"));
        }

        [Test]
        public void PreloadTableAsync_LoadsTable()
        {
            var task = _manager.PreloadTableAsync(Locale.enUS);
            task.Wait();

            // 切换后应立即返回已加载的表
            _manager.SetLocale(Locale.enUS);
            Assert.AreEqual("Hello", _manager.GetString("hello"));
        }

        [Test]
        public void PreloadAllTablesAsync_LoadsAllTables()
        {
            var task = _manager.PreloadAllTablesAsync();
            task.Wait();

            // 两表都加载后，切换不需再异步等待
            _manager.SetLocale(Locale.enUS);
            Assert.AreEqual("Hello", _manager.GetString("hello"));
        }

        [Test]
        public void Dispose_ClearsTables()
        {
            _manager.Dispose();

            // Dispose 后 GetString 优雅降级，返回键本身
            var result = _manager.GetString("hello");
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void Selector_ReturnsConfiguredSelector()
        {
            var selector = _manager.Selector;
            Assert.IsNotNull(selector);
            Assert.AreEqual(2, selector.AvailableLocales.Count);
            Assert.IsTrue(selector.IsLocaleAvailable(Locale.zhCN));
            Assert.IsTrue(selector.IsLocaleAvailable(Locale.enUS));
        }
    }
}
