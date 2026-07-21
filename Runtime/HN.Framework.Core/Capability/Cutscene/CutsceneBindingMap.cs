using System.Collections.Generic;

namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 绑定解析方式
    /// </summary>
    public enum BindingResolveMode
    {
        /// <summary>通过场景路径查找（GameObject.Find）</summary>
        ScenePath = 0,
        /// <summary>通过 Tag 查找（GameObject.FindWithTag）</summary>
        Tag,
        /// <summary>通过 EntityId 间接查找（集成 L3 Entity 系统）</summary>
        EntityId,
        /// <summary>通过 CutsceneActor 组件查找</summary>
        ActorComponent
    }

    /// <summary>
    /// 过场角色绑定映射，用于 CutsceneBindingResolver 将角色名解析为场景中的 GameObject。
    /// 支持四种解析方式：ScenePath / Tag / EntityId / ActorComponent。
    /// </summary>
    [System.Serializable]
    public struct CutsceneBindingMap
    {
        /// <summary>解析方式</summary>
        public BindingResolveMode ResolveMode { get; set; }
        
        /// <summary>绑定映射表：角色名 → 绑定标识（场景路径/Tag/EntityId/组件标签）</summary>
        public Dictionary<string, string> Bindings { get; set; }

        public CutsceneBindingMap(BindingResolveMode resolveMode, Dictionary<string, string>? bindings = null)
        {
            ResolveMode = resolveMode;
            Bindings = bindings ?? new Dictionary<string, string>();
        }

        /// <summary>添加绑定项</summary>
        public void AddBinding(string roleName, string targetIdentifier)
        {
            Bindings[roleName] = targetIdentifier;
        }

        /// <summary>尝试获取绑定目标标识</summary>
        public bool TryGetBinding(string roleName, out string targetIdentifier)
        {
            return Bindings.TryGetValue(roleName, out targetIdentifier);
        }
    }
}
