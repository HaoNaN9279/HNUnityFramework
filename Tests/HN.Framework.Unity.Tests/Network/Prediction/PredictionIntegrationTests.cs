#nullable enable

using System;
using FishNet.Component.ColliderRollback;
using FishNet.Managing.Predicting;
using FishNet.Managing.Timing;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Unity.Capability.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Network.Prediction
{
    /// <summary>
    /// 客户端预测集成测试（架构验证）。
    /// 验证预测/校调/回滚流程的核心类型创建、null 安全、序列化与默认值行为。
    /// 不启动真实 FishNet 网络——仅做代码级验证。
    /// </summary>
    [TestFixture]
    public class PredictionIntegrationTests
    {
        private GameObject? _gameObject;
        private PredictionManager? _predictionManager;
        private TimeManager? _timeManager;
        private RollbackManager? _rollbackManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("PredictionIntegrationTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        [TearDown]
        public void TearDown()
        {
            _predictionManager = null;
            _timeManager = null;
            _rollbackManager = null;

            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        #region Null 安全检查

        /// <summary>
        /// PredictionManagerAdapter 构造函数：传入 null PredictionManager 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void PredictionManagerAdapter_NullPredictionManager_ThrowsArgumentNullException()
        {
            _timeManager = _gameObject!.AddComponent<TimeManager>();

            Assert.That(
                () => new PredictionManagerAdapter(null!, _timeManager),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("predictionManager"));
        }

        /// <summary>
        /// PredictionManagerAdapter 构造函数：传入 null TimeManager 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void PredictionManagerAdapter_NullTimeManager_ThrowsArgumentNullException()
        {
            _predictionManager = _gameObject!.AddComponent<PredictionManager>();

            Assert.That(
                () => new PredictionManagerAdapter(_predictionManager, null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("timeManager"));
        }

        /// <summary>
        /// LagCompensationAdapter 构造函数：传入 null RollbackManager 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void LagCompensationAdapter_NullRollbackManager_ThrowsArgumentNullException()
        {
            _timeManager = _gameObject!.AddComponent<TimeManager>();

            Assert.That(
                () => new LagCompensationAdapter(null!, _timeManager),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("rollbackManager"));
        }

        /// <summary>
        /// LagCompensationAdapter 构造函数：传入 null TimeManager 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void LagCompensationAdapter_NullTimeManager_ThrowsArgumentNullException()
        {
            _rollbackManager = _gameObject!.AddComponent<RollbackManager>();

            Assert.That(
                () => new LagCompensationAdapter(_rollbackManager, null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("timeManager"));
        }

        #endregion

        #region PredictionReconcileData

        /// <summary>
        /// PredictionReconcileData&lt;T&gt; 结构体：字段赋值后能正确读取。
        /// 使用 int 作为 T，验证 unmanaged 约束下的字段行为。
        /// </summary>
        [Test]
        public void PredictionReconcileData_FieldAssignment_Correct()
        {
            var data = new PredictionReconcileData<int>
            {
                ClientTick = 1u,
                ServerTick = 5u,
                AuthoritativeState = 42
            };

            Assert.That(data.ClientTick, Is.EqualTo(1u),
                "ClientTick should match assigned value.");
            Assert.That(data.ServerTick, Is.EqualTo(5u),
                "ServerTick should match assigned value.");
            Assert.That(data.AuthoritativeState, Is.EqualTo(42),
                "AuthoritativeState should match assigned value.");
        }

        /// <summary>
        /// PredictionReconcileData&lt;T&gt; MemoryPack 序列化：序列化→反序列化往返测试。
        /// 使用 float 作为 T 验证泛型 struct 的正确往返。
        /// </summary>
        [Test]
        public void PredictionReconcileData_SerializeDeserialize_Roundtrip()
        {
            var original = new PredictionReconcileData<float>
            {
                ClientTick = 42u,
                ServerTick = 100u,
                AuthoritativeState = 3.14f
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<float>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(42u),
                "ClientTick should survive roundtrip.");
            Assert.That(result.ServerTick, Is.EqualTo(100u),
                "ServerTick should survive roundtrip.");
            Assert.That(result.AuthoritativeState, Is.EqualTo(3.14f).Within(0.0001f),
                "AuthoritativeState should survive roundtrip.");
        }

        /// <summary>
        /// PredictionReconcileData&lt;T&gt; 默认值：创建后所有字段为默认零值。
        /// 用于验证未赋值状态下的结构体行为。
        /// </summary>
        [Test]
        public void PredictionReconcileData_Default_AllFieldsAreZero()
        {
            var data = default(PredictionReconcileData<double>);

            Assert.That(data.ClientTick, Is.EqualTo(0u),
                "Default ClientTick should be 0.");
            Assert.That(data.ServerTick, Is.EqualTo(0u),
                "Default ServerTick should be 0.");
            Assert.That(data.AuthoritativeState, Is.EqualTo(0.0),
                "Default AuthoritativeState should be 0.");
        }

        #endregion

        #region PredictionInputBase

        /// <summary>
        /// PredictionInputBase 派生类：可正常实例化并读写 Tick 属性。
        /// 验证抽象基类接口（IReference）实现完整。
        /// </summary>
        [Test]
        public void PredictionInputBase_TickProperty_ReadWrite()
        {
            var input = new TestPredictionInput { Tick = 123u };

            Assert.That(input.Tick, Is.EqualTo(123u),
                "Tick property should be readable and writable.");
        }

        /// <summary>
        /// PredictionInputBase 派生类：Clear() 方法应正确重置所有字段。
        /// 验证 IReference 接口实现的正确性。
        /// </summary>
        [Test]
        public void PredictionInputBase_Clear_ResetsAllFields()
        {
            var input = new TestPredictionInput
            {
                Tick = 999u,
                AxisX = 1.5f,
                AxisY = -0.3f,
                Jump = true
            };

            input.Clear();

            Assert.That(input.Tick, Is.EqualTo(0u),
                "Tick should be 0 after Clear.");
            Assert.That(input.AxisX, Is.EqualTo(0f),
                "AxisX should be 0 after Clear.");
            Assert.That(input.AxisY, Is.EqualTo(0f),
                "AxisY should be 0 after Clear.");
            Assert.That(input.Jump, Is.False,
                "Jump should be false after Clear.");
        }

        /// <summary>
        /// PredictionInputBase 派生类：字段复制后值保持一致。
        /// 验证数据结构的字段读写完整性（MemoryPack 源码生成器在测试程序集中不可用）。
        /// </summary>
        [Test]
        public void PredictionInputBase_FieldCopy_PreservesValues()
        {
            var original = new TestPredictionInput
            {
                Tick = 256u,
                AxisX = 0.75f,
                AxisY = -0.5f,
                Jump = true
            };

            var copy = new TestPredictionInput
            {
                Tick = original.Tick,
                AxisX = original.AxisX,
                AxisY = original.AxisY,
                Jump = original.Jump
            };

            Assert.That(copy.Tick, Is.EqualTo(256u),
                "Tick should be preserved in copy.");
            Assert.That(copy.AxisX, Is.EqualTo(0.75f).Within(0.0001f),
                "AxisX should be preserved in copy.");
            Assert.That(copy.AxisY, Is.EqualTo(-0.5f).Within(0.0001f),
                "AxisY should be preserved in copy.");
            Assert.That(copy.Jump, Is.True,
                "Jump should be preserved in copy.");
        }

        #endregion

        #region GetNetworkTickInfo

        /// <summary>
        /// PredictionManagerAdapter.GetNetworkTickInfo() 在网络未启动时返回 (0, 0, 0)。
        /// 验证未初始化状态下返回合理的默认值。
        /// </summary>
        [Test]
        public void GetNetworkTickInfo_NotRunning_ReturnsDefaults()
        {
            _predictionManager = _gameObject!.AddComponent<PredictionManager>();
            _timeManager = _gameObject!.AddComponent<TimeManager>();

            using var adapter = new PredictionManagerAdapter(_predictionManager, _timeManager);
            var (tick, rtt, tickRate) = adapter.GetNetworkTickInfo();

            Assert.That(tick, Is.EqualTo(0u),
                "Tick should be 0 when network is not running.");
            Assert.That(rtt, Is.EqualTo(0L),
                "RoundTripTime should be 0 when network is not running.");
            Assert.That(tickRate, Is.EqualTo((ushort)0),
                "TickRate should be 0 when network is not running.");
        }

        #endregion

        #region 测试用派生类

        /// <summary>
        /// PredictionInputBase 的测试用具体子类。
        /// 用于验证 IReference 接口实现及字段读写行为。
        /// MemoryPack 源码生成器在测试程序集中不可用，
        /// 因此仅测试数据结构行为，不测试序列化。
        /// </summary>
        private sealed class TestPredictionInput : PredictionInputBase
        {
            /// <summary>水平轴输入（-1 ~ 1）。</summary>
            public float AxisX;

            /// <summary>垂直轴输入（-1 ~ 1）。</summary>
            public float AxisY;

            /// <summary>跳跃按钮。</summary>
            public bool Jump;

            /// <inheritdoc/>
            public override void Clear()
            {
                Tick = 0;
                AxisX = 0f;
                AxisY = 0f;
                Jump = false;
            }
        }

        #endregion
    }
}
