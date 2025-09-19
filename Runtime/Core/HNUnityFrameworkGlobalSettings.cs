using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework
{
    public class HNUnityFrameworkGlobalSettings : ScriptableObject
    {
#if UNITY_EDITOR
        public static HNUnityFrameworkGlobalSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<HNUnityFrameworkGlobalSettings>(GlobalSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<HNUnityFrameworkGlobalSettings>();
                AssetDatabase.CreateAsset(settings, GlobalSettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif


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


        public static readonly string GlobalSettingsPath = "Assets/HNUnityFramework/HNUnityFrameworkGlobalSettings.asset";

    }


    [Serializable]
    public enum LogicRateMode
    {
        Custom,
        NoLimit,
    }
}
