using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Driver.Common
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
