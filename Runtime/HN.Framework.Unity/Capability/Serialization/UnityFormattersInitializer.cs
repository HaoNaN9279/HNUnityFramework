using HN.Framework.Core.Capability.Serialization;

namespace HN.Framework.Unity.Capability.Serialization
{
    /// <summary>
    /// Unity 类型格式化器初始化器，提供 <see cref="RegisterAll"/> 方法
    /// 批量注册所有 Unity 内置类型的 MemoryPack 格式化器。
    /// </summary>
    public static class UnityFormattersInitializer
    {
        /// <summary>
        /// 注册所有 Unity 内置类型的 MemoryPack 格式化器。
        /// 建议在 GameWorld 初始化阶段调用。
        /// </summary>
        public static void RegisterAll()
        {
            // 首先注册 Core 层格式化器（FixedMathSharp 定点数类型）
            FormattersInitializer.RegisterAll();

            MemoryPackFormatterProvider.Register(new Vector2Formatter());
            MemoryPackFormatterProvider.Register(new Vector3Formatter());
            MemoryPackFormatterProvider.Register(new Vector4Formatter());
            MemoryPackFormatterProvider.Register(new Vector2IntFormatter());
            MemoryPackFormatterProvider.Register(new Vector3IntFormatter());
            MemoryPackFormatterProvider.Register(new QuaternionFormatter());
            MemoryPackFormatterProvider.Register(new ColorFormatter());
            MemoryPackFormatterProvider.Register(new Color32Formatter());
            MemoryPackFormatterProvider.Register(new BoundsFormatter());
            MemoryPackFormatterProvider.Register(new BoundsIntFormatter());
            MemoryPackFormatterProvider.Register(new RectFormatter());
            MemoryPackFormatterProvider.Register(new RectIntFormatter());
            MemoryPackFormatterProvider.Register(new Matrix4x4Formatter());
            MemoryPackFormatterProvider.Register(new LayerMaskFormatter());
            MemoryPackFormatterProvider.Register(new AnimationCurveFormatter());
            MemoryPackFormatterProvider.Register(new GradientFormatter());
        }
    }
}
