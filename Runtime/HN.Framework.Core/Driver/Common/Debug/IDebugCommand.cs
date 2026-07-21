#nullable enable

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 调试命令接口
    /// </summary>
    public interface IDebugCommand
    {
        /// <summary>
        /// 命令名称，如 "pool.show"
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 命令描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args">命令参数</param>
        void Execute(string[] args);
    }
}
