#nullable enable

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 调试中枢接口，管理日志通道和调试命令的注册与日志输出
    /// </summary>
    public interface IDebugHub
    {
        /// <summary>
        /// 注册日志通道
        /// </summary>
        /// <param name="channel">日志通道实例</param>
        void RegisterChannel(ILogChannel channel);

        /// <summary>
        /// 注册调试命令
        /// </summary>
        /// <param name="command">调试命令实例</param>
        void RegisterCommand(IDebugCommand command);

        /// <summary>
        /// 输出一条日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="channel">日志通道名称</param>
        /// <param name="message">日志消息</param>
        /// <param name="context">可选的日志上下文</param>
        void Log(LogLevel level, string channel, string message, object? context = null);
    }
}
