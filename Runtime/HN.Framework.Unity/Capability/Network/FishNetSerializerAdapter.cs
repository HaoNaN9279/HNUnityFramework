using System;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// FishNet 序列化适配器骨架，用于将 MemoryPack 注入 FishNet 的序列化管道。
    /// 完整实现由 C6 Network 模块负责。
    /// </summary>
    public static class FishNetSerializerAdapter
    {
        /// <summary>
        /// 注册 MemoryPack 为 FishNet 的自定义序列化器。
        /// 在 GameWorld 初始化时调用，后续由 C6 模块补充完整逻辑。
        /// </summary>
        public static void RegisterMemoryPackSerializer()
        {
            // C6 Network 模块负责在此处将 MemoryPack 注册为 FishNet 的全局序列化器：
            //   Writer.Write<T>(value) → global::MemoryPack.MemoryPackSerializer.Serialize
            //   Reader.Read<T>() → global::MemoryPack.MemoryPackSerializer.Deserialize
            throw new NotImplementedException("C6 Network 模块需实现此方法");
        }
    }
}
