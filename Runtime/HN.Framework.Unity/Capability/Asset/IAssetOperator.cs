using System;
using HN.Framework.Unity.Driver.Platform;

namespace HN.Framework.Unity.Capability.Asset
{
    /// <summary>
    /// 资源操作器接口，定义资源的同步加载、异步加载和释放操作
    /// </summary>
    public interface IAssetOperator
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(string name);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncLoadHandle LoadAssetAsync<T>(string name) where T : UnityEngine.Object;

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public void ReleaseAsset(UnityEngine.Object asset);
    }
}
