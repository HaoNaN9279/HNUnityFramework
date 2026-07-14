#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 语言选择器，管理可用语言列表和 <see cref="PlayerPrefs"/> 持久化语言偏好。
    /// </summary>
    public class LocaleSelector
    {
        private const string PlayerPrefsKey = "HN_Framework_Locale";

        private readonly IReadOnlyList<Core.Capability.Localization.Locale> _availableLocales;

        /// <summary>
        /// 使用指定的可用语言列表初始化选择器。
        /// </summary>
        /// <param name="availableLocales">用户可选的可用语言列表。</param>
        /// <exception cref="ArgumentNullException"><paramref name="availableLocales"/> 为 <c>null</c>。</exception>
        public LocaleSelector(IReadOnlyList<Core.Capability.Localization.Locale> availableLocales)
        {
            _availableLocales = availableLocales ?? throw new ArgumentNullException(nameof(availableLocales));
        }

        /// <summary>
        /// 获取可用语言列表。
        /// </summary>
        public IReadOnlyList<Core.Capability.Localization.Locale> AvailableLocales => _availableLocales;

        /// <summary>
        /// 获取 <see cref="PlayerPrefs"/> 中保存的语言偏好。
        /// 如果尚未保存或保存的语言不在可用列表中，则返回 <paramref name="fallback"/>。
        /// </summary>
        /// <param name="fallback">当无已保存偏好或偏好无效时使用的回退语言。</param>
        /// <returns>已保存的语言，或 <paramref name="fallback"/>。</returns>
        public Core.Capability.Localization.Locale GetSavedLocale(Core.Capability.Localization.Locale fallback)
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                return fallback;
            }

            var savedCode = PlayerPrefs.GetString(PlayerPrefsKey);

            for (var i = 0; i < _availableLocales.Count; i++)
            {
                if (_availableLocales[i].Code == savedCode)
                {
                    return _availableLocales[i];
                }
            }

            return fallback;
        }

        /// <summary>
        /// 将语言偏好保存到 <see cref="PlayerPrefs"/> 并立即写入磁盘。
        /// </summary>
        /// <param name="locale">要保存的语言。</param>
        public void SaveLocale(Core.Capability.Localization.Locale locale)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, locale.Code);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 验证指定的语言是否在可用语言列表中。
        /// </summary>
        /// <param name="locale">要检查的语言。</param>
        /// <returns>如果语言在可用列表中则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool IsLocaleAvailable(Core.Capability.Localization.Locale locale)
        {
            for (var i = 0; i < _availableLocales.Count; i++)
            {
                if (_availableLocales[i] == locale)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
