#nullable enable

using System;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Unity.Driver.Platform;
using TMPro;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 文本本地化组件。挂载到含 <see cref="TMP_Text"/> 的 GameObject 上，
    /// 自动在语言切换时更新文本内容。
    /// </summary>
    public class TextLocalizer : MonoBehaviour
    {
        [Tooltip("本地化键，用于从 ILocaleProvider 中查询对应的文本")]
        [SerializeField] private string _key;

        [Tooltip("目标文本组件，未指定时自动查找")]
        [SerializeField] private TMP_Text _text;

        private ILocaleProvider? _provider;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            var driver = FindObjectOfType<GameWorldDriver>();
            if (driver != null)
            {
                _provider = driver.World.LocaleProvider;
            }

            UpdateText();
        }

        private void OnEnable()
        {
            if (_provider != null)
                _provider.OnLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            if (_provider != null)
                _provider.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale)
        {
            UpdateText();
        }

        /// <summary>
        /// 手动更新本地化文本。从 ILocaleProvider 获取当前语言的字符串并赋值给 TMP_Text。
        /// </summary>
        public void UpdateText()
        {
            if (_text != null && _provider != null && !string.IsNullOrEmpty(_key))
            {
                _text.text = _provider.GetString(_key);
            }
        }

        /// <summary>
        /// 设置新的本地化键并立即刷新文本。
        /// </summary>
        /// <param name="newKey">新的本地化键。</param>
        public void SetKey(string newKey)
        {
            _key = newKey;
            UpdateText();
        }
    }
}
