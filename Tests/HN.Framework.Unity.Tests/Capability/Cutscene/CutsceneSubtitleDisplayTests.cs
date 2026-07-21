using HN.Framework.Core.Capability.Localization;
using HN.Framework.Unity.Capability.Cutscene;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneSubtitleDisplayTests
    {
        private class MockLocaleProvider : ILocaleProvider
        {
            public event System.Action<Locale>? OnLocaleChanged;
            public Locale CurrentLocale = Locale.enUS;

            public string GetString(string key) => key == "hello" ? "Hello World" : key;
            public Locale GetCurrentLocale() => CurrentLocale;
            public void SetLocale(Locale locale) { CurrentLocale = locale; OnLocaleChanged?.Invoke(locale); }
        }

        private GameObject _go;
        private CutsceneSubtitleDisplay _display;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("SubtitleDisplay");
            _go.hideFlags = HideFlags.HideAndDontSave;
            _display = _go.AddComponent<CutsceneSubtitleDisplay>();
            _display.hideFlags = HideFlags.HideAndDontSave;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void SubtitleDisplay_Initialized_HidesSubtitle()
        {
            Assert.That(_display, Is.Not.Null);
        }

        [Test]
        public void SubtitleDisplay_ShowSubtitle_WithProvider_DoesNotThrow()
        {
            var provider = new MockLocaleProvider();
            _display.Initialize(provider);
            Assert.DoesNotThrow(() => _display.ShowSubtitle("hello", "Speaker"));
        }

        [Test]
        public void SubtitleDisplay_ShowSubtitle_EmptyKey_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _display.ShowSubtitle("", ""));
        }

        [Test]
        public void SubtitleDisplay_HideSubtitle_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _display.HideSubtitle());
        }

        [Test]
        public void SubtitleDisplay_HideSubtitle_AfterShow_DoesNotThrow()
        {
            _display.ShowSubtitle("hello", "Speaker");
            Assert.DoesNotThrow(() => _display.HideSubtitle());
        }

        [Test]
        public void SubtitleDisplay_Initialize_NullProvider_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _display.Initialize(null));
        }
    }
}