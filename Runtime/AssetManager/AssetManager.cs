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
    public class AssetManager
    {
        public static OperationHandle<T> LoadAsset<T>(string name) where T : Object
        {
            return new OperationHandle<T>(Addressables.LoadAssetAsync<T>(name));
        }

        public static OperationHandle<T> LoadAsset<T>(AssetReference assetRef) where T : Object
        {
            return new OperationHandle<T>(assetRef.LoadAssetAsync<T>());
        }

        public static void ReleaseAsset<T>(T asset)
        {
            Addressables.Release(asset);
        }

        public static OperationHandle<SceneInstance> LoadScene(string name, LoadSceneMode loadSceneMode, bool activateOnLoad)
        {
            return new OperationHandle<SceneInstance>(Addressables.LoadSceneAsync(name, loadSceneMode, activateOnLoad));
        }

        public static OperationHandle<SceneInstance> UnloadScene(SceneInstance sceneInstance, UnloadSceneOptions unloadSceneOptions)
        {
            return new OperationHandle<SceneInstance>(Addressables.UnloadSceneAsync(sceneInstance, unloadSceneOptions));
        }


    }
}
