using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public class HNUnityFrameworkGlobalSettingsProvider : SettingsProvider
    {
        /// <summary>
        /// 创建设置提供者实例
        /// </summary>
        /// <returns>设置提供者实例</returns>
        [SettingsProvider]
        public static SettingsProvider CreateHNUnityFrameworkGlobalSettingsProvider()
        {
            var provider = new HNUnityFrameworkGlobalSettingsProvider("Project/HN Unity Framework", SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<HNUnityFrameworkGlobalSettings>();
            return provider;
        }


        public HNUnityFrameworkGlobalSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        /// <summary>
        /// 设置提供者激活时调用，初始化序列化属性
        /// </summary>
        /// <param name="searchContext">搜索上下文</param>
        /// <param name="rootElement">根 VisualElement</param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_GlobalSettings = HNUnityFrameworkGlobalSettings.GetSerializedSettings();
            m_LogicRateModeProperty = m_GlobalSettings.FindProperty("m_LogicRateMode");
            m_LogicRateProperty = m_GlobalSettings.FindProperty("m_LogicRate");
            m_MaxStepsPerFrameProperty = m_GlobalSettings.FindProperty("m_MaxStepsPerFrame");
            m_MaxFrameTimeProperty = m_GlobalSettings.FindProperty("m_MaxFrameTime");
        }

        /// <summary>
        /// 绘制设置界面
        /// </summary>
        /// <param name="searchContext">搜索上下文</param>
        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(m_LogicRateModeProperty, new GUIContent("Logic Rate Mode"));
            if (m_LogicRateModeProperty.enumValueIndex == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_LogicRateProperty, new GUIContent("Logic Rate"));
                EditorGUILayout.PropertyField(m_MaxStepsPerFrameProperty, new GUIContent("Max Steps Per Frame"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(m_MaxFrameTimeProperty, new GUIContent("Max Frame Time"));

            m_GlobalSettings.ApplyModifiedProperties();
        }


        private SerializedObject m_GlobalSettings;
        private SerializedProperty m_LogicRateModeProperty;
        private SerializedProperty m_LogicRateProperty;
        private SerializedProperty m_MaxStepsPerFrameProperty;
        private SerializedProperty m_MaxFrameTimeProperty;
    }


    [InitializeOnLoad]
    public static class HNUnityFrameworkGlobalSettingsInitializer
    {
        static HNUnityFrameworkGlobalSettingsInitializer()
        {
            HNUnityFrameworkGlobalSettings.GetOrCreateSettings();
        }
    }
}
