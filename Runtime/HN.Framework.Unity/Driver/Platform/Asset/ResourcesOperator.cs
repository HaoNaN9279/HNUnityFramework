using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common;

using Object = UnityEngine.Object;

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// Resources
    /// </summary>
    public class ResourcesOperator : IAssetOperator
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object LoadAsset(string name)
        {
            return Resources.Load(name);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object
        {
            return new AsyncLoadHandle(Resources.LoadAsync<T>(name));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(Object asset)
        {
            Resources.UnloadAsset(asset);
        }

        /// <summary>
        /// 实现接口 IReference
        /// </summary>
        public void Clear()
        {

        }
    }
}
