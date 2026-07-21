using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Cutscene
{
    /// <summary>
    /// 过场动画系统配置面板，显示在 Project Settings > HN Unity Framework > Cutscene。
    /// </summary>
    public class CutsceneSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new CutsceneSettingsProvider(
                "Project/HN Unity Framework/Cutscene", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.Cutscene.CutsceneSettings>();
            return provider;
        }

        private CutsceneSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.Cutscene.CutsceneSettings
                .GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.Cutscene.CutsceneSettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Cutscene Settings", EditorStyles.boldLabel);
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
