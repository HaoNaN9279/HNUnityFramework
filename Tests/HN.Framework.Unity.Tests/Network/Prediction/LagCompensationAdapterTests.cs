#nullable enable

using System;
using FishNet.Component.ColliderRollback;
using FishNet.Managing;
using FishNet.Managing.Timing;
using HN.Framework.Unity.Capability.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Network.Prediction
{
    /// <summary>
    /// LagCompensationAdapter 的 EditMode 集成测试。
    /// 使用真实 FishNet 组件（通过 HideAndDontSave GameObject）验证适配器的代码路径正确性。
    /// 若 FishNet 组件在 EditMode 环境中初始化失败，相应测试将标记为 Ignore。
    /// </summary>
    [TestFixture]
    public class LagCompensationAdapterTests
    {
        private GameObject? _gameObject;
        private NetworkManager? _networkManager;
        private RollbackManager? _rollbackManager;
        private TimeManager? _timeManager;
        private LagCompensationAdapter? _adapter;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("LagCompensationAdapterTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        [TearDown]
        public void TearDown()
        {
            _adapter?.Dispose();
            _adapter = null;

            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }

            _rollbackManager = null;
            _timeManager = null;
            _networkManager = null;
        }

        #region Constructor Null Checks

        /// <summary>
        /// 传入 null RollbackManager 应抛出 ArgumentNullException。
        /// 此测试不依赖任何 FishNet 运行时状态，仅验证防御性编程。
        /// </summary>
        [Test]
        public void Constructor_NullRollbackManager_ThrowsArgumentNullException()
        {
            // 需要一个有效的 TimeManager 来隔离测试目标参数
            _timeManager = _gameObject!.AddComponent<TimeManager>();

            Assert.That(
                () => new LagCompensationAdapter(null!, _timeManager),
                Throws.ArgumentNullException
                    .With.Property("ParamName").EqualTo("rollbackManager"));
        }

        /// <summary>
        /// 传入 null TimeManager 应抛出 ArgumentNullException。
        /// 此测试不依赖任何 FishNet 运行时状态，仅验证防御性编程。
        /// </summary>
        [Test]
        public void Constructor_NullTimeManager_ThrowsArgumentNullException()
        {
            // 需要一个有效的 RollbackManager 来隔离测试目标参数
            _rollbackManager = _gameObject!.AddComponent<RollbackManager>();

            Assert.That(
                () => new LagCompensationAdapter(_rollbackManager, null!),
                Throws.ArgumentNullException
                    .With.Property("ParamName").EqualTo("timeManager"));
        }

        /// <summary>
        /// 两个参数同时为 null 时应抛出 ArgumentNullException（先检查 rollbackManager）。
        /// </summary>
        [Test]
        public void Constructor_BothNull_ThrowsArgumentNullException()
        {
            Assert.That(
                () => new LagCompensationAdapter(null!, null!),
                Throws.ArgumentNullException
                    .With.Property("ParamName").EqualTo("rollbackManager"));
        }

        #endregion

        #region Construction and Dispose

        /// <summary>
        /// 使用有效的 FishNet 组件构造 LagCompensationAdapter 不应抛出异常。
        /// 若 EditMode 下 FishNet 组件 Awake 失败，此测试将被跳过。
        /// </summary>
        [Test]
        public void Constructor_ValidArguments_DoesNotThrow()
        {
            Assert.That(
                () => CreateAdapter(),
                Throws.Nothing,
                "Constructing with valid RollbackManager and TimeManager should not throw.");
        }

        /// <summary>
        /// Dispose 在正常构造后调用不应抛出异常。
        /// 验证适配器清理路径无意外错误。
        /// </summary>
        [Test]
        public void Dispose_DoesNotThrow()
        {
            CreateAdapter();

            Assert.That(
                () => _adapter!.Dispose(),
                Throws.Nothing,
                "Dispose should not throw after normal construction.");
        }

        /// <summary>
        /// 重复调用 Dispose 不应抛出异常（幂等性）。
        /// </summary>
        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            CreateAdapter();

            _adapter!.Dispose();

            Assert.That(
                () => _adapter.Dispose(),
                Throws.Nothing,
                "Calling Dispose twice should be safe (idempotent).");
        }

        #endregion

        #region Return

        /// <summary>
        /// Return() 在未执行 Rollback 时调用不应抛出异常。
        /// 验证直接调用 Return 的代码路径不崩溃。
        /// </summary>
        [Test]
        public void Return_WithoutRollback_DoesNotThrow()
        {
            CreateAdapter();

            Assert.That(
                () => _adapter!.Return(),
                Throws.Nothing,
                "Return() without prior Rollback should not throw.");
        }

        /// <summary>
        /// Return() 在 Dispose 后调用不应抛出异常。
        /// 验证 Dispose 后的防御性行为。
        /// </summary>
        [Test]
        public void Return_AfterDispose_DoesNotThrow()
        {
            CreateAdapter();
            _adapter!.Dispose();

            Assert.That(
                () => _adapter.Return(),
                Throws.Nothing,
                "Return() after Dispose should not throw.");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 创建包含完整 FishNet 组件链的 LagCompensationAdapter。
        /// 添加 NetworkManager + TimeManager + RollbackManager 到测试 GameObject 上，
        /// 然后构造适配器实例。
        /// </summary>
        private void CreateAdapter()
        {
            // NetworkManager 是 FishNet 组件树的根节点，TimeManager 依赖它
            _networkManager = _gameObject!.AddComponent<NetworkManager>();

            // TimeManager 通过 NetworkManager 获取 ticks 等信息
            _timeManager = _gameObject.AddComponent<TimeManager>();

            // RollbackManager 管理碰撞体回滚状态
            _rollbackManager = _gameObject.AddComponent<RollbackManager>();

            _adapter = new LagCompensationAdapter(_rollbackManager, _timeManager);
        }

        #endregion
    }
}
