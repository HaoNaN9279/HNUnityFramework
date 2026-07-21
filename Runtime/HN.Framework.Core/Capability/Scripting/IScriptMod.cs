namespace HN.Framework.Core.Capability.Scripting
{
    /// <summary>
    /// Mod 生命周期接口，定义 Mod 从加载到卸载的完整生命周期。
    /// </summary>
    public interface IScriptMod
    {
        /// <summary>
        /// Mod 的唯一标识符。
        /// </summary>
        string ModId { get; }

        /// <summary>
        /// Mod 的显示名称。
        /// </summary>
        string ModName { get; }

        /// <summary>
        /// Mod 当前状态。
        /// </summary>
        ModState State { get; }

        /// <summary>
        /// Mod 被加载时调用（初始化资源）。
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Mod 被启用时调用（开始运行逻辑）。
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Mod 被禁用时调用（暂停运行逻辑，不释放资源）。
        /// </summary>
        void OnDisable();

        /// <summary>
        /// Mod 被卸载时调用（释放所有资源）。
        /// </summary>
        void OnUnload();
    }
}
