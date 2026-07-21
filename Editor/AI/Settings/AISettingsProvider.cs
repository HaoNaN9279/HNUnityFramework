using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.AI
{
    /// <summary>
    /// AI 系统配置面板，显示在 Project Settings > HN Unity Framework > AI。
    /// </summary>
    public class AISettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new AISettingsProvider(
                "Project/HN Unity Framework/AI", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.AI.AISettings>();
            return provider;
        }

        private AISettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.AI.AISettings
                .GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.AI.AISettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("AI Settings", EditorStyles.boldLabel);
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
