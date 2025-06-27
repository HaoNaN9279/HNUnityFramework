using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace HN.Framework
{
    [System.Serializable]
    public class AssetManager
    {
        public static AssetManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetManager();
                    instance.Initialize();
                }

                return instance;
            }
        }

        private static AssetManager instance;


        // public static OperationHandle<T> Load<T>(string name) where T : Object => instance.LoadAsset<T>(name);
        // public static OperationHandle<T> Load<T>(AssetReference assetRef) where T : Object => instance.LoadAsset<T>(assetRef);
        // public static OperationHandle<SceneInstance> Load(string name, LoadSceneMode loadSceneMode, bool activateOnLoad) => instance.LoadScene(name, loadSceneMode, activateOnLoad);
        // public static OperationHandle<SceneInstance> Unload(SceneInstance sceneInstance, UnloadSceneOptions unloadSceneOptions) => instance.UnloadScene(sceneInstance, unloadSceneOptions);


        public OperationHandle<T> LoadAsset<T>(string name) where T : Object
        {
            return new OperationHandle<T>(Addressables.LoadAssetAsync<T>(name));
        }

        public OperationHandle<T> LoadAsset<T>(AssetReference assetRef) where T : Object
        {
            return new OperationHandle<T>(assetRef.LoadAssetAsync<T>());
        }

        public void ReleaseAsset<T>(T asset)
        {
            Addressables.Release(asset);
        }

        public OperationHandle<SceneInstance> LoadScene(string name, LoadSceneMode loadSceneMode, bool activateOnLoad)
        {
            return new OperationHandle<SceneInstance>(Addressables.LoadSceneAsync(name, loadSceneMode, activateOnLoad));
        }

        public OperationHandle<SceneInstance> UnloadScene(SceneInstance sceneInstance, UnloadSceneOptions unloadSceneOptions)
        {
            return new OperationHandle<SceneInstance>(Addressables.UnloadSceneAsync(sceneInstance, unloadSceneOptions));
        }



        public void Initialize()
        {
            Debug.Log("Asset Manager Initialize.");


        }
    }
}
