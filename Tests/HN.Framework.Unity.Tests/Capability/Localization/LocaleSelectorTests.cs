#nullable enable

using System.Collections.Generic;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Unity.Capability.Localization;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Localization
{
    /// <summary>
    /// <see cref="LocaleSelector"/> 的 EditMode 单元测试。
    /// 覆盖可用语言列表查询、PlayerPrefs 持久化、回退逻辑。
    /// </summary>
    [TestFixture]
    public class LocaleSelectorTests
    {
        private IReadOnlyList<Locale> _availableLocales = null!;

        [SetUp]
        public void SetUp()
        {
            _availableLocales = new List<Locale> { Locale.zhCN, Locale.enUS, Locale.jaJP };
            PlayerPrefs.DeleteKey("HN_Framework_Locale");
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey("HN_Framework_Locale");
        }

        [Test]
        public void GetSavedLocale_NoSavedPreference_ReturnsFallback()
        {
            var selector = new LocaleSelector(_availableLocales);
            var result = selector.GetSavedLocale(Locale.zhCN);
            Assert.AreEqual(Locale.zhCN, result);
        }

        [Test]
        public void SaveLocale_Then_GetSavedLocale_ReturnsSaved()
        {
            var selector = new LocaleSelector(_availableLocales);
            selector.SaveLocale(Locale.enUS);

            var result = selector.GetSavedLocale(Locale.zhCN);
            Assert.AreEqual(Locale.enUS, result);
        }

        [Test]
        public void IsLocaleAvailable_ExistingLocale_ReturnsTrue()
        {
            var selector = new LocaleSelector(_availableLocales);
            Assert.IsTrue(selector.IsLocaleAvailable(Locale.zhCN));
        }

        [Test]
        public void IsLocaleAvailable_NonExistingLocale_ReturnsFalse()
        {
            var selector = new LocaleSelector(_availableLocales);
            // zhTW 不在 _availableLocales 中
            Assert.IsFalse(selector.IsLocaleAvailable(Locale.zhTW));
        }

        [Test]
        public void AvailableLocales_ReturnsProvidedList()
        {
            var selector = new LocaleSelector(_availableLocales);
            Assert.AreEqual(3, selector.AvailableLocales.Count);
        }

        [Test]
        public void GetSavedLocale_SavedCodeNotInAvailable_ReturnsFallback()
        {
            // 手动写入 PlayerPrefs 一个不在可用列表中的语言代码
            PlayerPrefs.SetString("HN_Framework_Locale", "fr-FR");
            PlayerPrefs.Save();

            var selector = new LocaleSelector(_availableLocales);
            var result = selector.GetSavedLocale(Locale.zhCN);
            Assert.AreEqual(Locale.zhCN, result);
        }
    }
}
