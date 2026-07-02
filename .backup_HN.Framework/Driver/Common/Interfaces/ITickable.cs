using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Driver.Common
{
    public interface ITickable
    {
        /// <summary>
        /// 更新方法
        /// </summary>
        public void Tick();

        /// <summary>
        /// Late 更新方法
        /// </summary>
        public void LateTick();
    }
}
