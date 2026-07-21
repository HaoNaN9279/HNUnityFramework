using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// 框架模块配置 ScriptableObject 的通用工厂工具类。
    /// 封装 GetOrCreateSettings / GetSerializedSettings / EnsureDirectoryExists，
    /// 消除各模块配置的重复样板代码。
    /// </summary>
    public static class HNModuleSettingsUtility
    {
#if UNITY_EDITOR
        /// <summary>
        /// 加载或创建指定路径的配置资源。
        /// 路径目录不存在时自动递归创建。
        /// </summary>
        /// <typeparam name="T">配置类型，继承 ScriptableObject</typeparam>
        /// <param name="assetPath">配置资源路径（如 "Assets/Project/RuntimeAssets/Core/AudioSettings.asset"）</param>
        /// <returns>已存在的或新创建的配置实例</returns>
        public static T GetOrCreateSettings<T>(string assetPath) where T : ScriptableObject
        {
            var settings = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (settings == null)
            {
                var dir = Path.GetDirectoryName(assetPath);
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    EnsureDirectoryExists(dir);
                }

                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        /// <summary>
        /// 获取序列化版本的配置资源，用于 Editor 绘制。
        /// </summary>
        /// <typeparam name="T">配置类型，继承 ScriptableObject</typeparam>
        /// <param name="assetPath">配置资源路径</param>
        /// <returns>序列化对象</returns>
        public static SerializedObject GetSerializedSettings<T>(string assetPath) where T : ScriptableObject
        {
            return new SerializedObject(GetOrCreateSettings<T>(assetPath));
        }

        /// <summary>
        /// 递归确保 Asset 目录存在。
        /// 从 "Assets" 开始逐级检查并创建缺失的文件夹。
        /// </summary>
        /// <param name="assetFolderPath">Asset 目录路径（如 "Assets/Project/RuntimeAssets/Core"）</param>
        private static void EnsureDirectoryExists(string assetFolderPath)
        {
            if (string.IsNullOrEmpty(assetFolderPath))
                return;

            assetFolderPath = assetFolderPath.Replace('\\', '/');
            var segments = assetFolderPath.Split('/');
            string current = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string parent = current;
                current = parent + "/" + segments[i];

                if (!AssetDatabase.IsValidFolder(current))
                {
                    AssetDatabase.CreateFolder(parent, segments[i]);
                }
            }
        }
#endif
    }
}
