using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR 构建配置 ScriptableObject，定义 AOT 元数据生成和热更新构建的相关参数。
    /// 通过 <see cref="HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility"/> 统一管理创建/加载。
    /// </summary>
    public class HybridCLRSettings : ScriptableObject
    {
        [Header("AOT Metadata")]
        [Tooltip("AOT 元数据输出根目录")]
        public string aotAssembliesRoot = "Assets/StreamingAssets/AOTMetadata";

        [Tooltip("需要生成 AOT 元数据的程序集名称")]
        public string[] aotAssemblyNames = new[] { "Assembly-CSharp", "Assembly-CSharp-firstpass" };

        [Header("Hot Update Assemblies")]
        [Tooltip("热更新程序集输出根目录")]
        public string hotUpdateAssembliesRoot = "Assets/StreamingAssets/HotUpdates";

        [Tooltip("热更新程序集名称")]
        public string[] hotUpdateAssemblyNames = new[] { "HotFix" };

        [Header("Runtime Labels")]
        [Tooltip("AOT 元数据在 Addressables 中的标签")]
        public string aotMetadataLabel = "AOTMetadata";

        [Tooltip("热更新 DLL 在 Addressables 中的资源键")]
        public string hotUpdateKey = "HotUpdateDLL";

        [Header("Build Options")]
        [Tooltip("是否自动生成 AOT 补充元数据")]
        public bool autoGenerateMetadata = true;

        [Tooltip("是否自动拷贝 HybridCLR 原生库")]
        public bool autoCopyNativeLibs = true;

        // ── 工厂方法 ──

        /// <summary>
        /// 获取或创建 HybridCLR 构建配置资源。
        /// </summary>
        public static HybridCLRSettings GetOrCreateSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<HybridCLRSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<HybridCLRSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/EditorAssets/HybridCLRSettings.asset";
    }
}
