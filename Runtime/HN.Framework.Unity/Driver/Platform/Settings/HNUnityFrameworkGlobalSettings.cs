using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Driver.Platform
{
    public class HNUnityFrameworkGlobalSettings : ScriptableObject
    {
#if UNITY_EDITOR
        public static HNUnityFrameworkGlobalSettings GetOrCreateSettings()
        {
            return HNModuleSettingsUtility.GetOrCreateSettings<HNUnityFrameworkGlobalSettings>(GlobalSettingsPath);
        }

        public static SerializedObject GetSerializedSettings()
        {
            return HNModuleSettingsUtility.GetSerializedSettings<HNUnityFrameworkGlobalSettings>(GlobalSettingsPath);
        }
#endif

        public static string GetGlobalSettingsLoadPath()
        {
            return "Core/HNUnityFrameworkGlobalSettings.asset";
        }


        public LogicRateMode LogicRateMode => m_LogicRateMode;

        public int LogicRate => m_LogicRate;

        public int MaxStepsPerFrame => m_MaxStepsPerFrame;

        public float MaxFrameTime => m_MaxFrameTime;


        [SerializeField]
        private LogicRateMode m_LogicRateMode = LogicRateMode.Custom;

        [SerializeField]
        private int m_LogicRate = 60;

        [SerializeField]
        private int m_MaxStepsPerFrame = 5;

        [SerializeField]
        private float m_MaxFrameTime = 0.1f;


        public static readonly string GlobalSettingsPath = "Assets/Project/RuntimeAssets/Core/HNUnityFrameworkGlobalSettings.asset";

    }


    [Serializable]
    public enum LogicRateMode
    {
        Custom,
        NoLimit,
    }
}
