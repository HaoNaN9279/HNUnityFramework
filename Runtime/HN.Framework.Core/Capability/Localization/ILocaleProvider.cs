#nullable enable

using System;

namespace HN.Framework.Core.Capability.Localization
{
    /// <summary>
    /// 本地化提供程序接口，提供多语言字符串查询和语言区域切换能力。
    /// </summary>
    public interface ILocaleProvider
    {
        /// <summary>
        /// 当语言区域切换完成时触发。订阅者收到新的 <see cref="Locale"/> 值。
        /// </summary>
        event Action<Locale>? OnLocaleChanged;

        /// <summary>
        /// 根据键获取当前语言区域的本地化字符串。
        /// 如果键不存在，应返回键本身作为优雅降级。
        /// </summary>
        /// <param name="key">本地化键。</param>
        /// <returns>本地化文本，或键本身（降级）。</returns>
        string GetString(string key);

        /// <summary>
        /// 获取当前激活的语言区域。
        /// </summary>
        /// <returns>当前 <see cref="Locale"/>。</returns>
        Locale GetCurrentLocale();

        /// <summary>
        /// 切换到指定的语言区域，并触发 <see cref="OnLocaleChanged"/> 事件。
        /// </summary>
        /// <param name="locale">要切换到的目标语言区域。</param>
        void SetLocale(Locale locale);
    }
}
