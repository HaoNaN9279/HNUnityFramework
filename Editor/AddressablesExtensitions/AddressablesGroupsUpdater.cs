using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System;
using System.IO;
using System.Linq;

namespace HN.Framework.Editor
{
    public static class AddressablesGroupsUpdater
    {
        public static void Update()
        {
            Debug.Log("Update Addressable Groups");
            var settings = AssetDatabase.LoadAssetAtPath(HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_SETTINGS_PATH, typeof(AddressableAssetSettings)) as AddressableAssetSettings;
            if (settings == null)
            {
                Debug.LogError($"Can not find AddressableAssetSettings.asset at {HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_SETTINGS_PATH}");
            }

            var presetsObj = AssetDatabase.LoadAssetAtPath(HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH, typeof(AddressablesAssetsGroupPresets)) as AddressablesAssetsGroupPresets;
            if (presetsObj == null)
            {
                Debug.LogError($"Can not find AddressablesAssetsGroupPresets.asset at {HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH}");
            }

            List<AddressableAssetGroupSchema> schemas = new List<AddressableAssetGroupSchema>();
            var defaultLocalBundledAssetSchema = AssetDatabase.LoadAssetAtPath(HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_DEFAULT_LOCAL_BUNDLED_ASSET_SCHEMA, typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
            if (defaultLocalBundledAssetSchema != null)
                schemas.Add(defaultLocalBundledAssetSchema);

            var defaultLocalContentUpdateSchema = AssetDatabase.LoadAssetAtPath(HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_DEFAULT_LOCAL_CONTENT_UPDATE_SCHEMA, typeof(ContentUpdateGroupSchema)) as ContentUpdateGroupSchema;
            if (defaultLocalContentUpdateSchema != null)
                schemas.Add(defaultLocalContentUpdateSchema);

            Type[] schemasTypes = new Type[] { typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema) };

            //遍历存在的group，创建缺失的group
            Dictionary<string[], AddressableAssetGroup> groupsDict = new Dictionary<string[], AddressableAssetGroup>();
            List<string> groupNames = new List<string>();
            if (presetsObj.GroupPresets.Count > 0)
            {
                foreach (var elm in presetsObj.GroupPresets)
                {
                    AddressableAssetGroup group = settings.FindGroup(elm.GroupName);
                    if (group == null)
                    {
                        group = settings.CreateGroup(elm.GroupName, false, false, false, schemas, schemasTypes);
                    }
                    string pathKeywords = elm.PathKeywords.EndsWith(",") ? elm.PathKeywords.Remove(elm.PathKeywords.Length - 1) : elm.PathKeywords;
                    string[] keywords = pathKeywords.Split(",");
                    groupNames.Add(group.Name);
                    groupsDict.Add(keywords, group);
                }
            }
            AssetDatabase.Refresh();

            //遍历asset，找到所属的group并移动
            var assetGuids = AssetDatabase.FindAssets("", new string[] { HNUnityFrameworkConstants.RUNTIME_ASSETS_FOLDER_PATH });
            foreach (var guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                AddressableAssetEntry entry = settings.FindAssetEntry(guid);
                bool hasGroup = false;
                //对于每一个asset，遍历每一个group，比对asset路径与group的keyword，匹配就将该asset移动到该group
                foreach (var groupKp in groupsDict)
                {
                    string[] folders = assetPath.Split("/");
                    if (AllContained(groupKp.Key, folders))
                    {
                        hasGroup = true;
                        entry = settings.CreateOrMoveEntry(guid, groupKp.Value);
                        entry.parentGroup = groupKp.Value;
                        break;
                    }
                }
                entry.SetAddress(assetPath.Substring(HNUnityFrameworkConstants.RUNTIME_ASSETS_FOLDER_PATH.Length));
                //找不到group就删除现有的entry
                if (!hasGroup)
                {
                    settings.RemoveAssetEntry(guid);
                    entry = null;
                }
                //不属于任何一个group的asset报错提醒
                if (entry == null)
                {
                    Debug.LogError($"Asset: {assetPath} has no matched group.");
                }
            }

            //删除空的group
            var groupGuids = AssetDatabase.FindAssets("t:AddressableAssetGroup", new string[] { HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_GROUPS_FOLDER_PATH });
            foreach (var groupGuid in groupGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(groupGuid);
                string groupName = Path.GetFileNameWithoutExtension(groupPath);
                if (groupName != "Default Local Group" && groupName != "Built In Data" && !groupNames.Contains(groupName))
                {
                    var group = AssetDatabase.LoadAssetAtPath(groupPath, typeof(AddressableAssetGroup)) as AddressableAssetGroup;
                    settings.RemoveGroup(group);
                }
            }

        }
        

        private static bool AllContained(string[] arrayA, string[] arrayB)
        {
            if (arrayA.Length == 0) return true;
            if (arrayB.Length == 0) return false;
            var setB = new HashSet<string>(arrayB);
            return arrayA.All(element => arrayB.Contains(element));
        }
    }
}
