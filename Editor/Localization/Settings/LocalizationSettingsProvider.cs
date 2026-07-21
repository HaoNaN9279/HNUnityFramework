using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Localization
{
    /// <summary>
    /// 本地化系统配置面板，显示在 Project Settings > HN Unity Framework > Localization。
    /// </summary>
    public class LocalizationSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        /// <summary>
        /// Unity 自动发现并注册该 Provider。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new LocalizationSettingsProvider(
                "Project/HN Unity Framework/Localization", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.Localization.LocalizationSettings>();
            return provider;
        }

        private LocalizationSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.Localization.LocalizationSettings
                .GetSerializedSettings();
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.Localization.LocalizationSettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Localization Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

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
