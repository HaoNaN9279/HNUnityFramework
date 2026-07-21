#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Unity.Capability.Localization;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Localization
{
    /// <summary>
    /// 用于测试 <see cref="TextLocalizer"/> 的模拟 <see cref="ILocaleProvider"/> 实现。
    /// 支持多语言区域的字符串表存储和语言区域切换。
    /// </summary>
    internal class MockLocaleProvider : ILocaleProvider
    {
        public event Action<Locale>? OnLocaleChanged;

        private readonly Dictionary<string, StringTable> _tables = new();
        private Locale _currentLocale = Locale.zhCN;

        public void AddTable(Locale locale, StringTable table)
        {
            _tables[locale.Code] = table;
        }

        public string GetString(string key)
        {
            if (_tables.TryGetValue(_currentLocale.Code, out var table))
                return table.GetString(key);
            return key;
        }

        public Locale GetCurrentLocale()
        {
            return _currentLocale;
        }

        public void SetLocale(Locale locale)
        {
            _currentLocale = locale;
            OnLocaleChanged?.Invoke(locale);
        }
    }

    /// <summary>
    /// <see cref="TextLocalizer"/> 的 EditMode 单元测试。
    /// 覆盖文本更新、语言切换响应、键变更、边界条件。
    /// 因为 EditMode 不自动调用生命周期方法，通过反射注入依赖。
    /// </summary>
    [TestFixture]
    public class TextLocalizerTests
    {
        private GameObject _gameObject = null!;
        private TextLocalizer _localizer = null!;
        private TMP_Text _text = null!;
        private MockLocaleProvider _mockProvider = null!;

        private static readonly FieldInfo s_providerField =
            typeof(TextLocalizer).GetField("_provider", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly FieldInfo s_textField =
            typeof(TextLocalizer).GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TextLocalizerTest");
            _gameObject.hideFlags = HideFlags.HideAndDontSave;
            _text = _gameObject.AddComponent<TextMeshProUGUI>();
            _localizer = _gameObject.AddComponent<TextLocalizer>();

            // 注入 _text（绕过 Awake 的 GetComponent）
            s_textField.SetValue(_localizer, _text);

            _mockProvider = new MockLocaleProvider();
            _mockProvider.AddTable(Locale.zhCN, new StringTable(new Dictionary<string, string>
            {
                { "hello", "你好" },
                { "bye", "再见" }
            }));
            _mockProvider.AddTable(Locale.enUS, new StringTable(new Dictionary<string, string>
            {
                { "hello", "Hello" },
                { "bye", "Goodbye" }
            }));

            // 注入 _provider（绕过 Awake 的 FindObjectOfType）
            s_providerField.SetValue(_localizer, _mockProvider);

            _localizer.SetKey("hello");
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void SetKey_UpdatesTextImmediately()
        {
            // SetKey 在 SetUp 中已调用，验证 _text 已被更新
            Assert.AreEqual("你好", _text.text);
        }

        [Test]
        public void UpdateText_ReflectsCurrentLocale()
        {
            _localizer.UpdateText();
            Assert.AreEqual("你好", _text.text);
        }

        [Test]
        public void UpdateText_SwitchesLanguage_UpdatesText()
        {
            // 模拟 OnEnable 的事件订阅
            _mockProvider.OnLocaleChanged += (_) => _localizer.UpdateText();

            Assert.AreEqual("你好", _text.text);

            _mockProvider.SetLocale(Locale.enUS);
            Assert.AreEqual("Hello", _text.text);
        }

        [Test]
        public void SetKey_ChangesKeyAndRefreshes()
        {
            _localizer.SetKey("bye");
            Assert.AreEqual("再见", _text.text);
        }

        [Test]
        public void SetKey_ThenSwitchLocale_ReturnsCorrectText()
        {
            _mockProvider.OnLocaleChanged += (_) => _localizer.UpdateText();

            _localizer.SetKey("bye");
            Assert.AreEqual("再见", _text.text);

            _mockProvider.SetLocale(Locale.enUS);
            Assert.AreEqual("Goodbye", _text.text);
        }

        [Test]
        public void UpdateText_NullProvider_DoesNotThrow()
        {
            s_providerField.SetValue(_localizer, null);

            Assert.DoesNotThrow(() => _localizer.UpdateText());
        }

        [Test]
        public void UpdateText_EmptyKey_DoesNotChangeText()
        {
            _localizer.SetKey(string.Empty);
            _text.text = "Original";

            _localizer.UpdateText();
            Assert.AreEqual("Original", _text.text);
        }

        [Test]
        public void UpdateText_NullText_DoesNotThrow()
        {
            // 销毁 TMP_Text 组件并使字段为 null
            UnityEngine.Object.DestroyImmediate(_text);
            s_textField.SetValue(_localizer, null);

            Assert.DoesNotThrow(() => _localizer.UpdateText());
        }

        [Test]
        public void UpdateText_MissingKey_ReturnsKeyItself()
        {
            _localizer.SetKey("nonexistent");
            Assert.AreEqual("nonexistent", _text.text);
        }
    }
}
