#nullable enable

using System.Threading;
using System.Threading.Tasks;
using HN.Framework.Core.Capability.Localization;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 本地化数据加载器接口。负责从数据源加载指定语言区域的 <see cref="StringTable"/>。
    /// </summary>
    public interface ILocaleDataLoader
    {
        /// <summary>
        /// 异步加载指定语言区域的字符串表。
        /// </summary>
        /// <param name="locale">目标语言区域。</param>
        /// <param name="cancellationToken">取消令牌，用于在加载过程中取消操作。</param>
        /// <returns>加载完成的 <see cref="StringTable"/>；加载失败时返回空的 StringTable。</returns>
        Task<StringTable> LoadTableAsync(Locale locale, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查指定语言区域的数据是否可用。
        /// </summary>
        /// <param name="locale">目标语言区域。</param>
        /// <returns>如果当前加载器可以加载该语言区域的数据，则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool IsAvailable(Locale locale);
    }
}
