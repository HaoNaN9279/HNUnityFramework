using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Audio
{
    /// <summary>
    /// 音频系统配置面板，显示在 Project Settings > HN Unity Framework > Audio。
    /// </summary>
    public class AudioSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        /// <summary>
        /// Unity 自动发现并注册该 Provider。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new AudioSettingsProvider(
                "Project/HN Unity Framework/Audio", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.Audio.AudioSettings>();
            return provider;
        }

        private AudioSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.Audio.AudioSettings
                .GetSerializedSettings();
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.Audio.AudioSettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 自动遍历绘制所有序列化字段
            var prop = m_Settings.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name == "m_Script") continue;
                    EditorGUILayout.PropertyField(prop, true);
                }
                while (prop.NextVisible(false));
            }

            m_Settings.ApplyModifiedProperties();
        }
    }
}
