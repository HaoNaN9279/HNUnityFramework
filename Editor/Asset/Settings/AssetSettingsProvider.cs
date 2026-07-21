using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Asset
{
    /// <summary>
    /// 资源系统配置面板，显示在 Project Settings > HN Unity Framework > Asset。
    /// </summary>
    public class AssetSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new AssetSettingsProvider(
                "Project/HN Unity Framework/Asset", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.Asset.AssetSettings>();
            return provider;
        }

        private AssetSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.Asset.AssetSettings
                .GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.Asset.AssetSettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Asset Settings", EditorStyles.boldLabel);
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
