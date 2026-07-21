using FixedMathSharp;
using FixedMathSharp.Bounds;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability.Network.Messages;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Core.Level;
using HN.Framework.Core.Level.Logic.Entity;
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Core.Level.Logic.Inventory.Serialization;

namespace HN.Framework.Core.Capability.Serialization
{
    /// <summary>
    /// Core 层格式化器初始化器，提供 <see cref="RegisterAll"/> 方法
    /// 批量注册所有 Core 层 MemoryPack 格式化器。
    /// 建议在 GameWorld 初始化阶段调用。
    /// </summary>
    public static class FormattersInitializer
    {
        private static volatile bool _isRegistered;

        /// <summary>
        /// 注册所有 Core 层 MemoryPack 格式化器。幂等操作。
        /// 建议在 GameWorld 初始化阶段调用。
        /// </summary>
        public static void RegisterAll()
        {
            if (_isRegistered) return;
            _isRegistered = true;

            // FixedMathSharp 定点数格式化器
            MemoryPackFormatterProvider.Register(new Fixed64Formatter());
            MemoryPackFormatterProvider.Register(new Vector2dFormatter());
            MemoryPackFormatterProvider.Register(new Vector3dFormatter());
            MemoryPackFormatterProvider.Register(new Vector4dFormatter());
            MemoryPackFormatterProvider.Register(new FixedQuaternionFormatter());
            MemoryPackFormatterProvider.Register(new Fixed4x4Formatter());
            MemoryPackFormatterProvider.Register(new FixedBoundBoxFormatter());
            MemoryPackFormatterProvider.Register(new FixedBoundSphereFormatter());

            // 模块格式化器
            EntityFormatters.RegisterAll();
            AssetRefFormatters.RegisterAll();
            FrameInputFormatters.RegisterAll();
            MessageFormatters.RegisterAll();
            GameplayTagFormatters.RegisterAll();
            PredictionInputFormatter.Register();
        }
    }
}
