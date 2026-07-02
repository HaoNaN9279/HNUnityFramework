using System.Collections.Generic;

namespace HN.Framework.Level.Logic
{
    #region Model单元接口
    /// <summary>
    /// Model单元接口
    /// </summary>
    public interface IModelUnit : ITickable, IReference
    {
        /// <summary>
        /// 初始化Model单元
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Model单元第一帧更新
        /// </summary>
        public void OnFirstFrame();
    }
    #endregion

    /// <summary>
    /// Model单元
    /// </summary>
    public abstract class ModelUnit : IModelUnit
    {
        #region 实现接口 IModelUnit
        /// <summary>
        /// 初始化Model单元
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Model单元第一帧更新
        /// </summary>
        public abstract void OnFirstFrame();

        /// <summary>
        /// 每帧更新
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public abstract void LateTick();

        /// <summary>
        /// 销毁Model单元
        /// </summary>
        public abstract void Clear();
        #endregion
    }
}
