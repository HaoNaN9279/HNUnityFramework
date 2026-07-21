using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Network
{
    /// <summary>
    /// 网络系统配置面板，显示在 Project Settings > HN Unity Framework > Network。
    /// </summary>
    public class NetworkSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var provider = new NetworkSettingsProvider(
                "Project/HN Unity Framework/Network", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<
                HN.Framework.Unity.Capability.Network.NetworkSettings>();
            return provider;
        }

        private NetworkSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = HN.Framework.Unity.Capability.Network.NetworkSettings
                .GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = HN.Framework.Unity.Capability.Network.NetworkSettings
                    .GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Network Settings", EditorStyles.boldLabel);
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
