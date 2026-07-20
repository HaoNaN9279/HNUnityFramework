using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework.Editor
{
    public static class HNUnityFrameworkConstants
    {
        /// <summary>
        /// 框架名称，用于菜单和日志标识。
        /// </summary>
        public const string FRAMEWORK_NAME = "HNFramework";
        /// <summary>
        /// 项目文件夹根路径（由 FrameworkDeployer 自动创建）。
        /// </summary>
        public const string PROJECT_PATH = "Assets/Project";
        /// <summary>
        /// 运行时资源文件夹路径。
        /// </summary>
        public const string RUNTIME_ASSETS_FOLDER_PATH = PROJECT_PATH + "/RuntimeAssets";
        /// <summary>
        /// Addressable Asset Settings 配置文件路径。
        /// </summary>
        public const string ADDRESSABLES_ASSETS_SETTINGS_PATH = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        /// <summary>
        /// Addressable 资源组文件夹路径。
        /// </summary>
        public const string ADDRESSABLES_ASSETS_GROUPS_FOLDER_PATH = "Assets/AddressableAssetsData/AssetGroups";
        /// <summary>
        /// 默认本地资源组的 BundledAssetGroupSchema 文件路径。
        /// </summary>
        public const string ADDRESSABLES_ASSETS_DEFAULT_LOCAL_BUNDLED_ASSET_SCHEMA = "Assets/AddressableAssetData/AssetGroups/Schemas/Default Local Group_BundledAssetGroupSchema.asset";
        /// <summary>
        /// 默认本地资源组的 ContentUpdateGroupSchema 文件路径。
        /// </summary>
        public const string ADDRESSABLES_ASSETS_DEFAULT_LOCAL_CONTENT_UPDATE_SCHEMA = "Assets/AddressableAssetData/AssetGroups/Schemas/Default Local Group_ContentUpdateGroupSchema.asset";
        /// <summary>
        /// Addressable 资源组预设配置文件路径。
        /// </summary>
        public const string ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH = "Assets/AddressableAssetsData/AddressablesAssetsGroupPresets.asset";
    }
}
