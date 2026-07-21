using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Capability
{
    public interface ILogProvider
    {
        void Log(string message);

        /// <summary>
        /// 记录结构化日志。
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="channel">日志通道名称</param>
        /// <param name="message">日志消息</param>
        /// <param name="context">可选的上下文对象</param>
        void Log(LogLevel level, string channel, string message, object context = null);
    }
}
