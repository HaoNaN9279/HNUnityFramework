using UnityEngine;

namespace HN.Framework.Unity.Level.View
{
    /// <summary>
    /// View 工厂基类，负责创建和管理 View 实例。
    /// </summary>
    public abstract class ViewFactory
    {
        /// <summary>
        /// 根据给定的地址创建 View 实例。
        /// </summary>
        /// <param name="prefabAddress">预制体地址（如 Addressables 地址或 Resources 路径）。</param>
        /// <returns>创建的 <see cref="EntityView"/> 实例，创建失败时返回 null。</returns>
        public virtual EntityView CreateView(string prefabAddress)
        {
            return null;
        }
    }
}
