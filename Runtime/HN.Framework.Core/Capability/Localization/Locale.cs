#nullable enable

using System;

namespace HN.Framework.Core.Capability.Localization
{
    /// <summary>
    /// 表示一个语言区域标识，包含语言代码和显示名称。
    /// 支持值相等性比较，可作为字典键使用。
    /// </summary>
    [Serializable]
    public readonly struct Locale : IEquatable<Locale>
    {
        /// <summary>
        /// 语言区域代码，遵循 BCP 47 标签格式（如 "zh-CN"、"en-US"、"ja-JP"）。
        /// </summary>
        public readonly string Code;

        /// <summary>
        /// 语言区域的本地化显示名称（如 "中文"、"English"、"日本語"）。
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 使用指定的代码和名称初始化 <see cref="Locale"/> 的新实例。
        /// </summary>
        /// <param name="code">语言区域代码（BCP 47 标签）。</param>
        /// <param name="name">本地化显示名称。</param>
        public Locale(string code, string name)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>中文（简体）。</summary>
        public static readonly Locale zhCN = new("zh-CN", "中文（简体）");

        /// <summary>中文（繁体）。</summary>
        public static readonly Locale zhTW = new("zh-TW", "中文（繁體）");

        /// <summary>英语。</summary>
        public static readonly Locale enUS = new("en-US", "English");

        /// <summary>日语。</summary>
        public static readonly Locale jaJP = new("ja-JP", "日本語");

        /// <summary>韩语。</summary>
        public static readonly Locale koKR = new("ko-KR", "한국어");

        /// <summary>
        /// 基于语言区域代码比较两个 <see cref="Locale"/> 是否相等。
        /// </summary>
        public bool Equals(Locale other) => Code == other.Code;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Locale other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Code.GetHashCode();

        /// <summary>
        /// 基于语言区域代码比较两个 <see cref="Locale"/> 是否相等。
        /// </summary>
        public static bool operator ==(Locale left, Locale right) => left.Equals(right);

        /// <summary>
        /// 基于语言区域代码比较两个 <see cref="Locale"/> 是否不等。
        /// </summary>
        public static bool operator !=(Locale left, Locale right) => !left.Equals(right);

        /// <inheritdoc/>
        public override string ToString() => $"{Name} ({Code})";
    }
}
