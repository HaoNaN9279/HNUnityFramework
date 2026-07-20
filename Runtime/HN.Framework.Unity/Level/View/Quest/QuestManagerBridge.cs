#nullable enable

using UnityEngine;
using HN.Framework.Core.Level.Logic.Quest;

namespace HN.Framework.Unity.Level.View.Quest
{
    /// <summary>
    /// QuestManager 视图桥接组件。挂载到 GameObject 上，用于桥接 Core 层 <see cref="IQuestManager{TId}"/> 数据到场景。
    /// 遵循 View-Bridge 模式，纯桥接不包含业务逻辑（与 <c>ContainerView</c>/<c>EquipmentView</c> 一致）。
    /// 外部系统（如 UI 面板）可通过 <see cref="GetQuestManager"/> 获取管理器引用。
    /// </summary>
    public class QuestManagerBridge : MonoBehaviour
    {
        [SerializeField] private uint _entityId;
        private IQuestManager<uint>? _questManager;

        /// <summary>绑定的 Entity ID。</summary>
        public uint EntityId => _entityId;

        /// <summary>
        /// 初始化 QuestManager 桥接。
        /// </summary>
        /// <param name="entityId">绑定的实体 ID。</param>
        public void Initialize(uint entityId)
        {
            _entityId = entityId;
        }

        /// <summary>
        /// 获取 Core 层 QuestManager 实例。可能为 null（尚未通过 <see cref="SetQuestManager"/> 注入）。
        /// </summary>
        public IQuestManager<uint>? GetQuestManager() => _questManager;

        /// <summary>
        /// 设置 Core 层 QuestManager 引用。由 GameWorld 或管理器在初始化时调用。
        /// </summary>
        /// <param name="questManager">QuestManager 实例。</param>
        public void SetQuestManager(IQuestManager<uint> questManager) => _questManager = questManager;

        private void OnDestroy()
        {
            _questManager = null;
        }
    }
}
