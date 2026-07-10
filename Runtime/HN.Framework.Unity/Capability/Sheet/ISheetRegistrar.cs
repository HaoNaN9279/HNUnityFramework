#nullable enable

using HN.Framework.Core.Level.Logic.Sheet;

namespace HN.Framework.Unity.Capability.Sheet
{
    /// <summary>
    /// 由配置代码生成的 Tables 类实现此接口，
    /// 在其中将每个表注册到 <see cref="ISheetManager"/>。
    /// </summary>
    /// <remarks>
    /// Tables 类会在 <see cref="RegisterTo"/> 方法中遍历所有表字段，
    /// 调用 <see cref="ISheetManager.RegisterTable{TKey,TRow}"/> 逐一注册。
    /// </remarks>
    public interface ISheetRegistrar
    {
        /// <summary>
        /// 将所有表注册到指定的配置表管理器。
        /// </summary>
        /// <param name="manager">配置表管理器实例。</param>
        void RegisterTo(ISheetManager manager);
    }
}
