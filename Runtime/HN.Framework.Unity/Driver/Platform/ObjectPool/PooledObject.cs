using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// 池化对象接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPooledObject<T> where T : Object
    {
        /// <summary>
        /// 目标Object
        /// </summary>
        T Target { get; }

        /// <summary>
        /// 使用Object初始化池化对象
        /// </summary>
        /// <param name="obj"></param>
        public void Initialize(T obj);
    }

    /// <summary>
    /// 池化对象基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PooledObject<T> : PooledObjectBase, IPooledObject<T> where T : Object
    {
        /// <summary>
        /// 目标Object
        /// </summary>
        public T Target { get; set; }

        /// <summary>
        /// 使用Object初始化池化对象
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Initialize(T obj);

        /// <summary>
        /// 清空对象
        /// </summary>
        public override void Clear()
        {
            Target = null;
        }
    }
}
