using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutsceneCanvas : MonoBehaviour
    {
        [Tooltip("黑边 Image 组件")]
        public Image LetterboxImage;

        [Tooltip("字幕显示组件")]
        public CutsceneSubtitleDisplay SubtitleDisplay;

        [Tooltip("黑边高度比例（0~1，0.5=上下各25%）")]
        public float LetterboxHeight = 0.15f;

        [Tooltip("黑边渐入渐出时间（秒）")]
        public float LetterboxFadeDuration = 0.5f;

        private Coroutine _letterboxCoroutine;
        private static CutsceneCanvas _instance;

        public static CutsceneCanvas Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }

        public void ShowLetterbox()
        {
            gameObject.SetActive(true);
            if (_letterboxCoroutine != null) StopCoroutine(_letterboxCoroutine);
            _letterboxCoroutine = StartCoroutine(FadeLetterbox(1f));
        }

        public void HideLetterbox()
        {
            if (_letterboxCoroutine != null) StopCoroutine(_letterboxCoroutine);
            _letterboxCoroutine = StartCoroutine(FadeLetterbox(0f, () =>
            {
                gameObject.SetActive(false);
            }));
        }

        private IEnumerator FadeLetterbox(float targetAlpha, System.Action onComplete = null)
        {
            if (LetterboxImage == null) yield break;

            float startAlpha = LetterboxImage.color.a;
            float elapsed = 0f;

            while (elapsed < LetterboxFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / LetterboxFadeDuration;
                var color = LetterboxImage.color;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                LetterboxImage.color = color;
                yield return null;
            }

            var finalColor = LetterboxImage.color;
            finalColor.a = targetAlpha;
            LetterboxImage.color = finalColor;

            onComplete?.Invoke();
        }

        public void ShowSubtitle(string localizationKey, string speakerRole)
        {
            if (SubtitleDisplay != null)
                SubtitleDisplay.ShowSubtitle(localizationKey, speakerRole);
        }

        public void HideSubtitle()
        {
            if (SubtitleDisplay != null)
                SubtitleDisplay.HideSubtitle();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}