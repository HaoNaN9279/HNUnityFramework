namespace HN.Framework.Core.Driver.Common
{
    /// <summary>
    /// 可被引用池管理的对象接口
    /// 提供一个清理方法，用于重置对象状态
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 清理对象状态
        /// </summary>
        public void Clear();
    }
}
