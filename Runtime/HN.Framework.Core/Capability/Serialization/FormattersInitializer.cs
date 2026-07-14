using FixedMathSharp;
using FixedMathSharp.Bounds;

namespace HN.Framework.Core.Capability.Serialization
{
    /// <summary>
    /// Core 层格式化器初始化器，提供 <see cref="RegisterAll"/> 方法
    /// 批量注册 FixedMathSharp 定点数类型的 MemoryPack 格式化器。
    /// </summary>
    public static class FormattersInitializer
    {
        /// <summary>
        /// 注册所有 FixedMathSharp 定点数类型的 MemoryPack 格式化器。
        /// 建议在 GameWorld 初始化阶段调用。
        /// </summary>
        public static void RegisterAll()
        {
            MemoryPackFormatterProvider.Register(new Fixed64Formatter());
            MemoryPackFormatterProvider.Register(new Vector2dFormatter());
            MemoryPackFormatterProvider.Register(new Vector3dFormatter());
            MemoryPackFormatterProvider.Register(new Vector4dFormatter());
            MemoryPackFormatterProvider.Register(new FixedQuaternionFormatter());
            MemoryPackFormatterProvider.Register(new Fixed4x4Formatter());
            MemoryPackFormatterProvider.Register(new FixedBoundBoxFormatter());
            MemoryPackFormatterProvider.Register(new FixedBoundSphereFormatter());
        }
    }
}
