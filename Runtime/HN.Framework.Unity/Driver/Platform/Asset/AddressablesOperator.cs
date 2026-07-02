using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if HAS_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

using HN.Framework.Unity.Capability.Asset;

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// Addressables
    /// </summary>
    public class AddressablesOperator : IAssetOperator
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Object LoadAsset(string name)
        {
            Debug.LogError("AddressablesOperator does not support synchronous loading.");
            return null;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object
        {
#if HAS_ADDRESSABLES
            return new AsyncLoadHandle(Addressables.LoadAssetAsync<T>(name));
#else
            Debug.LogError("Addressables is not installed. Cannot load asset async.");
            return new AsyncLoadHandle(null);
#endif
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(Object asset)
        {
#if HAS_ADDRESSABLES
            Addressables.Release(asset);
#else
            Debug.LogWarning("Addressables is not installed. ReleaseAsset is a no-op.");
#endif
        }
    }
}
