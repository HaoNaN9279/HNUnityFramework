using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建管线设置提供者，在 Project Settings 中显示。
    /// </summary>
    public class BuildPipelineSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        /// <summary>
        /// 创建设置提供者实例。
        /// </summary>
        /// <returns>设置提供者实例</returns>
        [SettingsProvider]
        public static SettingsProvider CreateBuildPipelineSettingsProvider()
        {
            var provider = new BuildPipelineSettingsProvider(
                "Project/HN Unity Framework/Build Pipeline", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<BuildPipelineSettings>();
            return provider;
        }

        private BuildPipelineSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
        }

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_Settings = BuildPipelineSettings.GetSerializedSettings();
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            if (m_Settings == null)
            {
                m_Settings = BuildPipelineSettings.GetSerializedSettings();
            }

            m_Settings.Update();

            EditorGUILayout.LabelField("Build Pipeline Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                m_Settings.FindProperty("m_BuildOutputRoot"),
                new GUIContent("Build Output Root"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pre-Build Validation", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                m_Settings.FindProperty("m_EnablePreBuildValidation"),
                new GUIContent("Enable Validation"));

            using (new EditorGUI.DisabledGroupScope(
                !m_Settings.FindProperty("m_EnablePreBuildValidation").boolValue))
            {
                EditorGUILayout.PropertyField(
                    m_Settings.FindProperty("m_BlockBuildOnValidationFailure"),
                    new GUIContent("Block Build on Failure"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Reporting", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                m_Settings.FindProperty("m_AutoGenerateBuildManifest"),
                new GUIContent("Auto Generate Manifest"));

            EditorGUILayout.PropertyField(
                m_Settings.FindProperty("m_VersionConfigPath"),
                new GUIContent("Version Config Path"));

            m_Settings.ApplyModifiedProperties();
        }
    }
}
