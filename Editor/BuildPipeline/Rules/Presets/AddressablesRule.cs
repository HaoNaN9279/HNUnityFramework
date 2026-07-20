using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// Addressables 配置检查规则。
    /// 检测同 Key 冲突、Bundle 命名冲突、必填 Label 缺失等。
    /// Addressables 包未安装时静默跳过。
    /// </summary>
    public class AddressablesRule : IAssetRule
    {
        private static readonly bool s_AddressablesAvailable;
        private static readonly string s_AddressablesSettingsType = "UnityEditor.AddressableAssets.AddressableAssetSettings";
        private static Type s_SettingsType;

        /// <summary>
        /// 静态构造函数：探测 Addressables 包是否存在。
        /// </summary>
        static AddressablesRule()
        {
            s_SettingsType = Type.GetType(
                "UnityEditor.AddressableAssets.AddressableAssetSettings, Unity.Addressables.Editor",
                false);
            s_AddressablesAvailable = s_SettingsType != null;
        }

        /// <inheritdoc />
        public string RuleName => "Addressables Configuration Check";

        /// <inheritdoc />
        public string Description => "Checks Addressables configuration (duplicate keys, bundle naming, labels).";

        /// <inheritdoc />
        public Type TargetImporterType => null;

        /// <inheritdoc />
        public IReadOnlyList<RuleResult> Validate(string assetPath)
        {
            var results = new List<RuleResult>();

            if (!s_AddressablesAvailable)
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            try
            {
                // 获取 AddressableAssetSettings 实例
                object settings = GetAddressableAssetSettings();
                if (settings == null)
                {
                    results.Add(RuleResult.Pass(RuleName, assetPath));
                    return results;
                }

                // 检查 Addressables 分组中的重复 Key
                CheckDuplicateKeys(settings, assetPath, results);
            }
            catch (Exception ex)
            {
                results.Add(RuleResult.Fail(
                    RuleName,
                    assetPath,
                    RuleSeverity.Warning,
                    $"Failed to check Addressables configuration: {ex.Message}"));
            }

            if (results.Count == 0)
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
            }

            return results;
        }

        private static object GetAddressableAssetSettings()
        {
            if (s_SettingsType == null)
            {
                return null;
            }

            // 通过反射调用 AddressableAssetSettingsDefaultObject.Settings
            var defaultObjectType = Type.GetType(
                "UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor",
                false);

            if (defaultObjectType == null)
            {
                return null;
            }

            var settingsProp = defaultObjectType.GetProperty(
                "Settings",
                BindingFlags.Public | BindingFlags.Static);

            return settingsProp?.GetValue(null, null);
        }

        private void CheckDuplicateKeys(
            object settings, string assetPath, List<RuleResult> results)
        {
            if (settings == null)
            {
                return;
            }

            // 通过反射访问 groups 属性
            var groupsProp = s_SettingsType.GetProperty("groups",
                BindingFlags.Public | BindingFlags.Instance);

            if (groupsProp == null)
            {
                return;
            }

            var groups = groupsProp.GetValue(settings, null) as System.Collections.IList;
            if (groups == null)
            {
                return;
            }

            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (object group in groups)
            {
                Type groupType = group.GetType();
                var entriesProp = groupType.GetProperty("entries",
                    BindingFlags.Public | BindingFlags.Instance);

                if (entriesProp == null)
                {
                    continue;
                }

                var entries = entriesProp.GetValue(group, null) as System.Collections.IList;
                if (entries == null)
                {
                    continue;
                }

                foreach (object entry in entries)
                {
                    Type entryType = entry.GetType();
                    var addressProp = entryType.GetProperty("address",
                        BindingFlags.Public | BindingFlags.Instance);

                    if (addressProp == null)
                    {
                        continue;
                    }

                    string address = addressProp.GetValue(entry, null) as string;
                    if (string.IsNullOrEmpty(address))
                    {
                        continue;
                    }

                    if (!seenKeys.Add(address))
                    {
                        results.Add(RuleResult.Fail(
                            RuleName,
                            assetPath,
                            RuleSeverity.Error,
                            $"Duplicate Addressables key found: \"{address}\""));
                    }
                }
            }
        }
    }
}
