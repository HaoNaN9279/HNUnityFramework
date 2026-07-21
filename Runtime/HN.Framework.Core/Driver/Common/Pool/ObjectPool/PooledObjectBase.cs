using System.Collections;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Driver.Common.Pool.ObjectPool
{
    /// <summary>
    /// 池化对象基类
    /// </summary>
    public abstract class PooledObjectBase : IReference
    {
        /// <summary>
        /// 清空对象
        /// </summary>
        public abstract void Clear();
    }
}
