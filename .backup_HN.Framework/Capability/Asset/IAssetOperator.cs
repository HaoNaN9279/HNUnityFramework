using System;
using System.Collections;
using System.Collections.Generic;

using Object = System.Object;

namespace HN.Framework.Capability
{
    /// <summary>
    /// 资源操作器接口，定义资源的同步加载、异步加载和释放操作
    /// </summary>
    public interface IAssetOperator : IReference
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object LoadAsset(string name);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object;

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(Object asset);
    }
}
