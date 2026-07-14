using MemoryPack;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Level.Logic.Entity
{
    /// <summary>
    /// 实体基类，纯数据模型，支持 MemoryPack 序列化用于网络同步。
    /// 实现 <see cref="IReference"/> 接口，支持通过 ReferencePool 进行对象池复用。
    /// </summary>
    [MemoryPackable]
    public partial class Entity : IReference
    {
        [MemoryPackOrder(0)]
        public uint EntityId { get; private set; }

        [MemoryPackOrder(1)]
        public int EntityDefId { get; private set; }

        /// <summary>
        /// 初始化实体数据。仅供 EntityManager 调用。
        /// </summary>
        internal void Initialize(uint id, int defId)
        {
            EntityId = id;
            EntityDefId = defId;
        }

        /// <summary>
        /// 重置实体数据，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            EntityId = 0;
            EntityDefId = 0;
        }
    }
}
