namespace HN.Framework.Core.Capability.Scripting
{
    /// <summary>
    /// Mod 的状态枚举。
    /// </summary>
    public enum ModState
    {
        /// <summary>
        /// 未加载。
        /// </summary>
        NotLoaded,

        /// <summary>
        /// 已加载但未启用。
        /// </summary>
        Loaded,

        /// <summary>
        /// 已启用，正在运行。
        /// </summary>
        Enabled,

        /// <summary>
        /// 已禁用（暂停运行）。
        /// </summary>
        Disabled,

        /// <summary>
        /// 加载或执行时出错。
        /// </summary>
        Error
    }
}
