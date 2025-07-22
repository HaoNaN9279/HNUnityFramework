using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

using Object = UnityEngine.Object;

namespace HN.Framework
{
    public interface IAssetCacheItem : IReference
    {
        /// <summary>
        /// 初始化资源缓存项
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="asset"></param>
        public void Initialize(string assetName, Object asset);
    }

    /// <summary>
    /// 资源缓存项
    /// 资源缓存项是一个弱引用，避免内存泄漏
    /// /// 资源缓存项的生命周期由引用池管理
    /// </summary>
    public class AssetCacheItem : IAssetCacheItem
    {
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public AssetCacheItem()
        {
            m_assetName = null;
            m_assetReference = new WeakReference(null);
        }
        #endregion

        #region 对外函数
        /// <summary>
        /// 初始化资源缓存项
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="asset"></param>
        public void Initialize(string assetName, Object asset)
        {
            m_assetName = assetName;
            m_assetReference = new WeakReference(asset);
        }

        public void Clear()
        {
            m_assetName = null;
            m_assetReference.Target = null;
        }
        #endregion

        #region 对外属性
        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName => m_assetName;

        /// <summary>
        /// 资源引用
        /// </summary>
        public WeakReference AssetReference => m_assetReference;

        /// <summary>
        /// 资源是否仍然有效
        /// </summary>
        public bool IsAlive => m_assetReference != null && m_assetReference.IsAlive;

        /// <summary>
        /// 获取资源
        /// </summary>
        public Object Asset => m_assetReference.IsAlive ? m_assetReference.Target as Object : null;
        #endregion

        private string m_assetName;
        private WeakReference m_assetReference;


    }
}
