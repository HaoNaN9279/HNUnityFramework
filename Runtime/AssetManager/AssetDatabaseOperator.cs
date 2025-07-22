using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;

#if UNITY_EDITOR
namespace HN.Framework
{
    /// <summary>
    /// AssetDatabase
    /// </summary>
    public class AssetDatabaseOperator : IAssetOperator
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object LoadAsset(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Object>(name);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object
        {
            Debug.LogError("AssetDatabaseOperator does not support asynchronous loading.");
            return null;
        }

        /// <summary>
        /// 释放资源s
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(Object asset)
        {
            asset = null;
        }

        /// <summary>
        /// 实现接口 IReference
        /// </summary>
        public void Clear()
        {

        }
    }
}
#endif