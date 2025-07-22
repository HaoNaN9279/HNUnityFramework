using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace HN.Framework
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
            return new AsyncLoadHandle(Addressables.LoadAssetAsync<T>(name));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(Object asset)
        {
            Addressables.Release(asset);
        }

        /// <summary>
        /// 实现接口 IReference
        /// </summary>
        public void Clear()
        {

        }
    }
}
