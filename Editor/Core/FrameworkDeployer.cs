using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HN.Framework.Editor
{
    public static class FrameworkDeployer
    {
        public static void Deploy()
        {
            Debug.Log("Deploy");
            HNUnityFrameworkGlobalSettings.GetOrCreateSettings();
            CreateProjectFolders();
            CreateGameEntryScene();
            CreateAddressablesAssetsGroupPresets();

            AssetDatabase.SaveAssets();

        }


        private static void CreateProjectFolders()
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

        private static void CreateGameEntryScene()
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

        private static void CreateAddressablesAssetsGroupPresets()
        {
            var obj = AssetDatabase.LoadAssetAtPath(HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH, typeof(AddressablesAssetsGroupPresets));
            if (obj == null)
            {
                var presetsObj = ScriptableObject.CreateInstance<AddressablesAssetsGroupPresets>();
                AssetDatabase.CreateAsset(presetsObj, HNUnityFrameworkConstants.ADDRESSABLES_ASSETS_GROUP_PRESETS_PATH);
                AssetDatabase.Refresh();
            }
        }

        private static void TryCreateFolder(string parentFolder, string newFolder)
        {
            if (!AssetDatabase.IsValidFolder(parentFolder + "/" + newFolder))
            {
                AssetDatabase.CreateFolder(parentFolder, newFolder);
            }
        }
    }
}
