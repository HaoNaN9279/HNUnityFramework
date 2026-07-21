using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR 构建配置面板，显示在 Project Settings > HN Unity Framework > HybridCLR。
    /// </summary>
    public class HybridCLRSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new HybridCLRSettingsProvider(
                "Project/HN Unity Framework/HybridCLR", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Editor.Scripting.HybridCLRSettings>();
            return provider;
        }

        private HybridCLRSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HybridCLRSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HybridCLRSettings.GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("HybridCLR Build Settings", EditorStyles.boldLabel);
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
