namespace HN.Framework.Core.Capability.Scripting
{
    /// <summary>
    /// 脚本引擎抽象接口，定义脚本执行、全局注册和函数调用的基本契约。
    /// 由具体的脚本引擎实现（如 xLua、HybridCLR 运行时加载等）。
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// 执行一段脚本代码。
        /// </summary>
        /// <param name="code">要执行的脚本代码。</param>
        /// <param name="chunkName">代码块名称，用于调试和错误追踪。</param>
        void Execute(string code, string chunkName = "inline");

        /// <summary>
        /// 在脚本环境中注册一个全局对象。
        /// </summary>
        /// <param name="name">全局变量名称。</param>
        /// <param name="obj">要注册的对象实例。</param>
        void RegisterGlobal(string name, object obj);

        /// <summary>
        /// 调用脚本中定义的函数。
        /// </summary>
        /// <param name="moduleName">模块名称。</param>
        /// <param name="funcName">函数名称。</param>
        /// <param name="args">传递给函数的参数。</param>
        /// <returns>函数返回值。</returns>
        object CallFunction(string moduleName, string funcName, params object[] args);

        /// <summary>
        /// 释放脚本引擎占用的资源。
        /// </summary>
        void Dispose();
    }
}
