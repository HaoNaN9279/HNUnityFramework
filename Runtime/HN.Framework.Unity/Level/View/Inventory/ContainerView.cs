#nullable enable

using UnityEngine;
using HN.Framework.Core.Level.Logic.Inventory;

namespace HN.Framework.Unity.Level.View.Inventory
{
    /// <summary>
    /// 容器视图组件。挂载到 Entity 的 GameObject 上，用于桥接 Core 层 <see cref="IContainer"/> 数据到场景。
    /// 外部系统（如 UI 面板）可通过 <see cref="GetContainer"/> 获取只读容器数据。
    /// </summary>
    public class ContainerView : MonoBehaviour
    {
        [SerializeField] private uint _entityId;
        [SerializeField] private ContainerType _containerType;
        private IContainer? _container;

        /// <summary>绑定的 Entity ID。</summary>
        public uint EntityId => _entityId;

        /// <summary>容器类型（背包、仓库、装备栏等）。</summary>
        public ContainerType ContainerType => _containerType;

        /// <summary>
        /// 初始化容器视图。
        /// </summary>
        /// <param name="entityId">绑定的实体 ID。</param>
        /// <param name="containerType">容器类型。</param>
        public void Initialize(uint entityId, ContainerType containerType)
        {
            _entityId = entityId;
            _containerType = containerType;
        }

        /// <summary>
        /// 获取 Core 层容器实例。可能为 null（尚未通过 <see cref="SetContainer"/> 注入）。
        /// </summary>
        public IContainer? GetContainer() => _container;

        /// <summary>
        /// 设置 Core 层容器引用。由 GameWorld 或管理器在初始化时调用。
        /// </summary>
        public void SetContainer(IContainer container) => _container = container;

        private void OnDestroy()
        {
            _container = null;
        }
    }
}