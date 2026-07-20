#nullable enable

using MemoryPack;


namespace HN.Framework.Core.Level.Logic.Quest.Serialization
{
    /// <summary>
    /// Quest 模块的 MemoryPack 格式化器注册入口。
    /// 调用 RegisterAll() 在启动时注册所有自定义格式化器。
    /// 幂等操作，可重复调用。
    /// </summary>
    public static class QuestFormatters
    {
        private static bool _registered;

        /// <summary>
        /// 注册所有 Quest 模块的 MemoryPack 格式化器。
        /// 幂等安全。
        /// </summary>
        public static void RegisterAll()
        {
            if (_registered) return;
            _registered = true;

            // QuestInstance 和 AchievementInstance 使用 [MemoryPackable] 源生成器，
            // 无需手动注册格式化器。
            // 如果将来需要版本兼容控制，在此添加手动格式化器注册。

            // 示例（预留）：
            // MemoryPackFormatterProvider.Register(new QuestInstanceFormatter());
            // MemoryPackFormatterProvider.Register(new AchievementInstanceFormatter());
        }

        /// <summary>
        /// 重置注册状态（仅测试使用）。
        /// </summary>
        internal static void ResetRegistration()
        {
            _registered = false;
        }
    }
}
