using System.Collections;
using HN.Framework.Core.Capability.Localization;
using TMPro;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutsceneSubtitleDisplay : MonoBehaviour
    {
        [Tooltip("字幕 TextMeshPro 组件")]
        public TextMeshProUGUI SubtitleText;

        [Tooltip("说话者名称 TextMeshPro 组件")]
        public TextMeshProUGUI SpeakerText;

        [Tooltip("字幕容器 CanvasGroup（用于淡入淡出）")]
        public CanvasGroup SubtitleCanvasGroup;

        [Tooltip("淡入淡出时间（秒）")]
        public float FadeDuration = 0.3f;

        [Tooltip("默认字体大小")]
        public float DefaultFontSize = 36f;

        private ILocaleProvider _localeProvider;
        private Coroutine _fadeCoroutine;
        private bool _isVisible;

        private void Awake()
        {
            if (SubtitleCanvasGroup == null)
                SubtitleCanvasGroup = GetComponent<CanvasGroup>();

            if (SubtitleCanvasGroup != null)
                SubtitleCanvasGroup.alpha = 0f;

            _isVisible = false;
        }

        public void Initialize(ILocaleProvider localeProvider)
        {
            _localeProvider = localeProvider;
            if (_localeProvider != null)
            {
                _localeProvider.OnLocaleChanged += OnLocaleChanged;
            }
        }

        public void ShowSubtitle(string localizationKey, string speakerRole)
        {
            if (string.IsNullOrEmpty(localizationKey)) return;

            var text = _localeProvider != null
                ? _localeProvider.GetString(localizationKey)
                : localizationKey;

            if (SubtitleText != null)
                SubtitleText.text = text;

            if (SpeakerText != null && !string.IsNullOrEmpty(speakerRole))
                SpeakerText.text = speakerRole;

            if (!_isVisible)
            {
                _isVisible = true;
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeTo(1f));
            }
        }

        public void HideSubtitle()
        {
            if (_isVisible)
            {
                _isVisible = false;
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeTo(0f));
            }
        }

        private void OnLocaleChanged(Locale newLocale)
        {
            if (_isVisible && SubtitleText != null && !string.IsNullOrEmpty(SubtitleText.text))
            {
                var key = SubtitleText.text;
                if (_localeProvider != null)
                    SubtitleText.text = _localeProvider.GetString(key);
            }
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (SubtitleCanvasGroup == null) yield break;

            float startAlpha = SubtitleCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / FadeDuration;
                SubtitleCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            SubtitleCanvasGroup.alpha = targetAlpha;
        }

        private void OnDestroy()
        {
            if (_localeProvider != null)
            {
                _localeProvider.OnLocaleChanged -= OnLocaleChanged;
            }
        }
    }
}