using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.UI
{
    /// <summary>
    /// UI 系统配置面板，显示在 Project Settings > HN Unity Framework > UI。
    /// </summary>
    public class UISettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        /// <summary>
        /// Unity 自动发现并注册该 Provider。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new UISettingsProvider(
                "Project/HN Unity Framework/UI", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Level.View.UI.UISettings>();
            return provider;
        }

        private UISettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Level.View.UI.UISettings
                .GetSerializedSettings();
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Level.View.UI.UISettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
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
