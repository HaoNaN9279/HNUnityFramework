using HN.Framework.Core.Driver;

namespace HN.Framework.Core.Capability.Scripting
{
    /// <summary>
    /// 热更新 DLL 入口接口。热更新程序集通过实现此接口，
    /// 在框架加载后注册游戏模块到 GameWorld。
    /// </summary>
    public interface IHotUpdateEntry
    {
        /// <summary>
        /// 注册热更新 DLL 中的游戏模块到 GameWorld。
        /// 此方法在热更新 DLL 被加载后由 HybridCLRAdapter 调用。
        /// </summary>
        /// <param name="world">框架的 GameWorld 实例。</param>
        void OnRegisterGameModules(GameWorld world);
    }
}
