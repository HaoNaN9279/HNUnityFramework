using UnityEngine;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// GameObject 对象池配置参数
    /// </summary>
    public class GameObjectPoolSettings
    {
        /// <summary>
        /// 基础对象池配置
        /// </summary>
        public PoolSettings BaseSettings;

        /// <summary>
        /// 对象池管理器根节点
        /// </summary>
        public GameObject ManagerRoot;

        /// <summary>
        /// 原型对象（Instantiate 的模板）
        /// </summary>
        public GameObject Prototype;
    }
}
