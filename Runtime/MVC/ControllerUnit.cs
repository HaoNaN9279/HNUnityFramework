using System.Collections.Generic;

namespace HN.Framework
{
    /// <summary>
    /// Controller单元接口
    /// </summary>
    public interface IControllerUnit : ITickable, IReference
    {
        /// <summary>
        /// 初始化Controller单元
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Controller单元第一帧更新
        /// </summary>
        public void OnFirstFrame();
    }

    /// <summary>
    /// Controller单元
    /// </summary>
    public abstract class ControllerUnit : IControllerUnit
    {
        #region 实现接口 IControllerUnit
        /// <summary>
        /// 初始化Controller单元
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Controller单元第一帧更新
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
        /// 清理Controller单元
        /// </summary>
        public abstract void Clear();
        #endregion
    }
}
