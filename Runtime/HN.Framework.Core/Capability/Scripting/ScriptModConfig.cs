namespace HN.Framework.Core.Capability.Scripting
{
    /// <summary>
    /// Mod 的配置数据模型，定义 Mod 的元数据和加载参数。
    /// </summary>
    public class ScriptModConfig
    {
        /// <summary>
        /// Mod 的唯一标识符。
        /// </summary>
        public string ModId { get; set; }

        /// <summary>
        /// Mod 的显示名称。
        /// </summary>
        public string ModName { get; set; }

        /// <summary>
        /// Mod 的版本号。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Mod 的入口脚本文件路径列表。
        /// </summary>
        public string[] ScriptPaths { get; set; }

        /// <summary>
        /// 是否启用沙箱隔离（默认 true）。
        /// </summary>
        public bool Sandboxed { get; set; } = true;
    }
}
