using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System;
using System.IO;
using System.Linq;

namespace HN.Framework.Editor
{
    public class HNUnityFrameworkEditorMenus
    {
        [MenuItem(FRAMEWORK_NAME + "/Deploy Framework", false, 0)]
        public static void DeployFramework()
        {
            Debug.Log("Deploy");
            HNUnityFrameworkGlobalSettings.GetOrCreateSettings();
            CreateProjectFolders();
            CreateGameEntryScene();
            CreateAddressablesAssetsGroupPresets();

            AssetDatabase.SaveAssets();

            void CreateProjectFolders()
            {
                TryCreateFolder("Assets", "StreamingAssets");
                TryCreateFolder("Assets", "Resources");
                TryCreateFolder("Assets", "Plugins");
                TryCreateFolder("Assets", "Editor");
                AssetDatabase.Refresh();
                TryCreateFolder("Assets", "Project");
                AssetDatabase.Refresh();
                TryCreateFolder("Assets/Project", "EditorAssets");
                TryCreateFolder("Assets/Project", "RuntimeAssets");
                TryCreateFolder("Assets/Project", "Scripts");
                AssetDatabase.Refresh();
                TryCreateFolder("Assets/Project/RuntimeAssets", "Core");
                TryCreateFolder("Assets/Project/RuntimeAssets", "Scenes");
                TryCreateFolder("Assets/Project/RuntimeAssets", "Shaders");
                TryCreateFolder("Assets/Project/RuntimeAssets", "ConfigData");
                TryCreateFolder("Assets/Project/RuntimeAssets", "UI");
                AssetDatabase.Refresh();
                TryCreateFolder("Assets/Project/Scripts", "Editor");
                TryCreateFolder("Assets/Project/Scripts", "Runtime");
            }

            void CreateGameEntryScene()
            {
                string gameEntryScenePath = "Assets/Project/RuntimeAssets/Core/GameEntry.unity";
                var gameEntry = AssetDatabase.LoadAssetAtPath(gameEntryScenePath, typeof(Scene));
                if (gameEntry == null)
                {
                    Scene gameEntryScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(gameEntryScene, gameEntryScenePath);
                    AssetDatabase.Refresh();
                    EditorSceneManager.OpenScene(gameEntryScenePath, OpenSceneMode.Single);
                }
            }

            void CreateAddressablesAssetsGroupPresets()
            {
                var obj = AssetDatabase.LoadAssetAtPath(ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH, typeof(AddressablesAssetsGroupPresets));
                if (obj == null)
                {
                    var presetsObj = ScriptableObject.CreateInstance<AddressablesAssetsGroupPresets>();
                    AssetDatabase.CreateAsset(presetsObj, ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH);
                    AssetDatabase.Refresh();
                }
            }

            void TryCreateFolder(string parentFolder, string newFolder)
            {
                if (!AssetDatabase.IsValidFolder(parentFolder + "/" + newFolder))
                {
                    AssetDatabase.CreateFolder(parentFolder, newFolder);
                }
            }
        }


        [MenuItem(FRAMEWORK_NAME + "/Update Addressable Groups", false, 1)]
        public static void UpdateAddressableGroups()
        {
            Debug.Log("Update Addressable Groups");
            var settings = AssetDatabase.LoadAssetAtPath(ADDRESSABLES_ASSETS_SETTINGS_PATH, typeof(AddressableAssetSettings)) as AddressableAssetSettings;
            if (settings == null)
            {
                Debug.LogError($"Can not find AddressableAssetSettings.asset at {ADDRESSABLES_ASSETS_SETTINGS_PATH}");
            }

            var presetsObj = AssetDatabase.LoadAssetAtPath(ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH, typeof(AddressablesAssetsGroupPresets)) as AddressablesAssetsGroupPresets;
            if (presetsObj == null)
            {
                Debug.LogError($"Can not find AddressablesAssetsGroupPresets.asset at {ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH}");
            }

            List<AddressableAssetGroupSchema> schemas = new List<AddressableAssetGroupSchema>();
            var defaultLocalBundledAssetSchema = AssetDatabase.LoadAssetAtPath(ADDRESSABLES_ASSETS_DEFAULT_LOCAL_BUNDLED_ASSET_SCHEMA, typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
            if (defaultLocalBundledAssetSchema != null)
                schemas.Add(defaultLocalBundledAssetSchema);

            var defaultLocalContentUpdateSchema = AssetDatabase.LoadAssetAtPath(ADDRESSABLES_ASSETS_DEFAULT_LOCAL_CONTENT_UPDATE_SCHEMA, typeof(ContentUpdateGroupSchema)) as ContentUpdateGroupSchema;
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
            var assetGuids = AssetDatabase.FindAssets("", new string[] { RUNTIME_ASSETS_FOLDER_PATH });
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
                entry.SetAddress(assetPath.Substring(RUNTIME_ASSETS_FOLDER_PATH.Length));
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
            var groupGuids = AssetDatabase.FindAssets("t:AddressableAssetGroup", new string[] { ADDRESSABLES_ASSETS_GROUPS_FOLDER_PATH });
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


            bool AllContained(string[] arrayA, string[] arrayB)
            {
                if (arrayA.Length == 0) return true;
                if (arrayB.Length == 0) return false;
                var setB = new HashSet<string>(arrayB);
                return arrayA.All(element => arrayB.Contains(element));
            }
        }


        public const string FRAMEWORK_NAME = "HN Unity Framework";
        public const string RUNTIME_ASSETS_FOLDER_PATH = "Assets/Project/RuntimeAssets";
        public const string ADDRESSABLES_ASSETS_SETTINGS_PATH = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public const string ADDRESSABLES_ASSETS_GROUPS_FOLDER_PATH = "Assets/AddressableAssetsData/AssetGroups";
        public const string ADDRESSABLES_ASSETS_DEFAULT_LOCAL_BUNDLED_ASSET_SCHEMA = "Assets/AddressableAssetData/AssetGroups/Schemas/Default Local Group_BundledAssetGroupSchema.asset";
        public const string ADDRESSABLES_ASSETS_DEFAULT_LOCAL_CONTENT_UPDATE_SCHEMA = "Assets/AddressableAssetData/AssetGroups/Schemas/Default Local Group_ContentUpdateGroupSchema.asset";
        public const string ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH = "Assets/AddressableAssetsData/AddressablesAssetsGroupPresets.asset";
    }
}
