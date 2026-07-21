#nullable enable

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 模块级日志通道接口
    /// </summary>
    public interface ILogChannel
    {
        /// <summary>
        /// 通道名称，如 "pool", "network", "audio" 等
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 运行时开关，控制该通道日志是否输出
        /// </summary>
        bool Enabled { get; set; }
    }
}
