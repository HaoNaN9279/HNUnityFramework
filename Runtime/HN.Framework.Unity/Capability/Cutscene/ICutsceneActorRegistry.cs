using UnityEngine;

namespace HN.Framework.Unity.Capability.Cutscene
{
    /// <summary>
    /// 过场演员注册表接口。
    /// <see cref="CutsceneActor"/> 在 OnEnable 时自注册、OnDisable 时注销；
    /// <see cref="CutsceneBindingResolver"/> 通过本接口以 O(1) 字典查找解析角色绑定，
    /// 替代 GameObject.Find / FindObjectsByType 的 O(n) 全场景扫描。
    /// </summary>
    /// <remarks>
    /// 注册表以实例形式存在，由 <see cref="CutsceneManager"/> 持有并通过构造函数注入给
    /// <see cref="CutsceneBindingResolver"/>，取代旧的全局静态注册表，便于测试隔离与实现替换。
    /// </remarks>
    public interface ICutsceneActorRegistry
    {
        /// <summary>
        /// 注册一个 CutsceneActor，分别在 Role、Tag、EntityId 三个维度建立索引。
        /// 同名冲突时后者覆盖前者并输出警告。
        /// </summary>
        /// <param name="actor">要注册的演员组件。</param>
        void Register(CutsceneActor actor);

        /// <summary>
        /// 从注册表中注销一个 CutsceneActor。
        /// 仅当字典中当前值等于该 actor 的 GameObject 时才移除（防止覆盖后被误删）。
        /// </summary>
        /// <param name="actor">要注销的演员组件。</param>
        void Unregister(CutsceneActor actor);

        /// <summary>通过 ActorRole 查找 GameObject。O(1)。</summary>
        /// <param name="role">角色名。</param>
        /// <param name="go">命中时输出对应 GameObject。</param>
        /// <returns>命中返回 true，否则 false。</returns>
        bool TryResolveByRole(string role, out GameObject go);

        /// <summary>通过 ActorTag 查找 GameObject。O(1)。</summary>
        /// <param name="tag">演员标签。</param>
        /// <param name="go">命中时输出对应 GameObject。</param>
        /// <returns>命中返回 true，否则 false。</returns>
        bool TryResolveByTag(string tag, out GameObject go);

        /// <summary>通过 EntityId 查找 GameObject。O(1)。</summary>
        /// <param name="entityId">关联的实体 Id。</param>
        /// <param name="go">命中时输出对应 GameObject。</param>
        /// <returns>命中返回 true，否则 false。</returns>
        bool TryResolveByEntity(string entityId, out GameObject go);

        /// <summary>清空所有注册项（用于会话重置或测试环境清理）。</summary>
        void Clear();
    }
}
