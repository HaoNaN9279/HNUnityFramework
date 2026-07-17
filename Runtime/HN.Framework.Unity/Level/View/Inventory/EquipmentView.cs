#nullable enable

using UnityEngine;
using HN.Framework.Core.Level.Logic.Inventory;

namespace HN.Framework.Unity.Level.View.Inventory
{
    /// <summary>
    /// 装备视图组件。挂载到 Entity 的 GameObject 上，桥接 Core 层 <see cref="IEquipment"/> 数据到场景。
    /// 外部系统（如装备面板 UI）可通过 <see cref="GetEquipment"/> 获取装备数据。
    /// </summary>
    public class EquipmentView : MonoBehaviour
    {
        [SerializeField] private uint _entityId;
        private IEquipment? _equipment;

        /// <summary>绑定的 Entity ID。</summary>
        public uint EntityId => _entityId;

        /// <summary>
        /// 初始化装备视图。
        /// </summary>
        public void Initialize(uint entityId)
        {
            _entityId = entityId;
        }

        /// <summary>
        /// 获取 Core 层装备系统实例。可能为 null。
        /// </summary>
        public IEquipment? GetEquipment() => _equipment;

        /// <summary>
        /// 设置 Core 层装备系统引用。
        /// </summary>
        public void SetEquipment(IEquipment equipment) => _equipment = equipment;

        /// <summary>
        /// 获取指定槽位的已装备物品。
        /// </summary>
        /// <param name="slotMask">目标装备槽位掩码，由项目 Luban 配置表定义。</param>
        public ItemInstance? GetEquippedItem(int slotMask)
        {
            return _equipment?.GetEquippedItem(slotMask);
        }

        private void OnDestroy()
        {
            _equipment = null;
        }
    }
}