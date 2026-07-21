#nullable enable

using System.Threading.Tasks;
using HN.Framework.Core.Level.Logic.Sheet;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HN.Framework.Unity.Capability.Sheet
{
    /// <summary>
    /// <see cref="AssetRef{T}"/> 的 Unity 扩展方法。
    /// 提供基于 Addressables 的异步资源加载与释放。
    /// </summary>
    public static class AssetRefExtensions
    {
        /// <summary>
        /// 通过 <see cref="AssetRef{T}.Label"/> 异步加载资源。
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object 的子类。</typeparam>
        /// <param name="assetRef">配置表资产引用。</param>
        /// <returns>加载完成的资源实例。</returns>
        /// <exception cref="System.InvalidOperationException">
        /// <paramref name="assetRef"/> 无效（Label 为 null 或空）时抛出。
        /// </exception>
        public static async Task<T> LoadAssetAsync<T>(this AssetRef<T> assetRef) where T : UnityEngine.Object
        {
            if (!assetRef.IsValid)
                throw new System.InvalidOperationException("AssetRef is not valid (Label is null or empty).");

            var handle = Addressables.LoadAssetAsync<T>(assetRef.Label);
            return await handle.Task;
        }

        /// <summary>
        /// 释放通过 <see cref="LoadAssetAsync{T}"/> 加载的资源。
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object 的子类。</typeparam>
        /// <param name="assetRef">配置表资产引用。</param>
        /// <param name="asset">要释放的资源实例。允许为 null（不执行任何操作）。</param>
        public static void ReleaseAsset<T>(this AssetRef<T> assetRef, T? asset) where T : UnityEngine.Object
        {
            if (asset != null)
                Addressables.Release(asset);
        }
    }
}
