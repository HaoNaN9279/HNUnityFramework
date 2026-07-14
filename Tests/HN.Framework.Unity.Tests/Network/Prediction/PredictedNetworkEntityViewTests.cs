#nullable enable

using System.Collections.Generic;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Unity.Capability.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Network.Prediction
{
    /// <summary>
    /// PredictedNetworkEntityView 辅助方法的 EditMode 单元测试。
    /// 使用 Mock/PredictedNetworkEntityView 子类测试输入缓冲区、数据创建与释放、边界条件。
    /// 不依赖 FishNet 运行时。
    /// </summary>
    [TestFixture]
    public class PredictedNetworkEntityViewTests
    {
        private GameObject? _gameObject;
        private TestPredictedView? _view;

        /// <summary>
        /// 模拟用户输入结构，用于 CreateReplicateData 测试。
        /// </summary>
        private sealed class TestInput : PredictionInputBase
        {
            public float Horizontal;
            public float Vertical;
            public bool Jump;

            public override void Clear()
            {
                Tick = 0;
                Horizontal = 0f;
                Vertical = 0f;
                Jump = false;
            }
        }

        /// <summary>
        /// PredictedNetworkEntityView 的最小测试用子类。
        /// </summary>
        private sealed class TestPredictedView : PredictedNetworkEntityView
        {
            /// <summary>
            /// 最后模拟的输入列表（用于验证）。
            /// </summary>
            public readonly List<PredictionInputBase> LastSimulatedInputs = new();

            /// <summary>
            /// 公开 StoreReplicateInput 供测试调用。
            /// </summary>
            public void PublicStoreReplicateInput(PredictionInputBase input)
            {
                StoreReplicateInput(input);
            }

            /// <summary>
            /// 公开 GetReconcileInputs 供测试调用。
            /// </summary>
            public List<PredictionInputBase> PublicGetReconcileInputs(uint fromTick, uint toTick)
            {
                return GetReconcileInputs(fromTick, toTick);
            }

            /// <summary>
            /// 公开 CreateReplicateData 供测试调用。
            /// </summary>
            public T PublicCreateReplicateData<T>(uint tick) where T : PredictionInputBase, new()
            {
                return CreateReplicateData<T>(tick);
            }

            /// <summary>
            /// 公开 ReleaseReplicateData 供测试调用。
            /// </summary>
            public void PublicReleaseReplicateData<T>(T data) where T : PredictionInputBase
            {
                ReleaseReplicateData(data);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("PredictedViewTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _view = _gameObject.AddComponent<TestPredictedView>();
        }

        [TearDown]
        public void TearDown()
        {
            _view = null;
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        /// <summary>
        /// CreateReplicateData 应从对象池获取实例并设置 Tick。
        /// </summary>
        [Test]
        public void CreateReplicateData_AllocatesAndSetsTick()
        {
            var data = _view!.PublicCreateReplicateData<TestInput>(42u);
            Assert.That(data, Is.Not.Null, "CreateReplicateData should return non-null.");
            Assert.That(data.Tick, Is.EqualTo(42u), "Tick should be set to 42.");
        }

        /// <summary>
        /// CreateReplicateData → ReleaseReplicateData 可循环而不抛异常。
        /// </summary>
        [Test]
        public void CreateAndRelease_Cycles_DoesNotThrow()
        {
            var data = _view!.PublicCreateReplicateData<TestInput>(1u);
            data.Horizontal = 0.5f;
            Assert.DoesNotThrow(() => _view.PublicReleaseReplicateData(data),
                "ReleaseReplicateData should not throw.");
        }

        /// <summary>
        /// StoreReplicateInput 后 GetReconcileInputs 应能检索到。
        /// </summary>
        [Test]
        public void StoreAndRetrieve_SingleInput_ReturnsCorrect()
        {
            var data = _view!.PublicCreateReplicateData<TestInput>(10u);
            data.Horizontal = 0.3f;
            _view.PublicStoreReplicateInput(data);

            var results = _view.PublicGetReconcileInputs(10u, 10u);
            Assert.That(results, Has.Count.EqualTo(1), "Should retrieve 1 input.");
            Assert.That(results[0].Tick, Is.EqualTo(10u), "Input Tick should match.");
        }

        /// <summary>
        /// GetReconcileInputs 应返回指定 Tick 范围内的所有输入。
        /// </summary>
        [Test]
        public void GetReconcileInputs_RangedQuery_ReturnsCorrectCount()
        {
            for (uint i = 1; i <= 10; i++)
            {
                var data = _view!.PublicCreateReplicateData<TestInput>(i);
                _view.PublicStoreReplicateInput(data);
            }

            var results = _view!.PublicGetReconcileInputs(3u, 7u);
            Assert.That(results, Has.Count.EqualTo(5),
                "Should retrieve 5 inputs for tick range 3-7.");
        }

        /// <summary>
        /// 缓冲区满时 StoreReplicateInput 应自动淘汰最旧条目。
        /// </summary>
        [Test]
        public void StoreReplicateInput_FullBuffer_EvictsOldest()
        {
            // 填满缓冲区（MAX_INPUT_HISTORY = 300）
            for (uint i = 1; i <= 300; i++)
            {
                var data = _view!.PublicCreateReplicateData<TestInput>(i);
                _view.PublicStoreReplicateInput(data);
            }

            // 再添加一个，Tick=1 的应被淘汰
            var newData = _view!.PublicCreateReplicateData<TestInput>(301u);
            _view.PublicStoreReplicateInput(newData);

            var results = _view.PublicGetReconcileInputs(1u, 1u);
            Assert.That(results, Has.Count.EqualTo(0),
                "Oldest input (Tick=1) should be evicted when buffer is full.");
        }

        /// <summary>
        /// GetReconcileInputs 在不存在的 Tick 范围应返回空列表。
        /// </summary>
        [Test]
        public void GetReconcileInputs_NoMatch_ReturnsEmpty()
        {
            var results = _view!.PublicGetReconcileInputs(100u, 200u);
            Assert.That(results, Is.Empty,
                "Empty buffer should return empty list.");
        }

        /// <summary>
        /// StoreReplicateInput 传入 null 应不抛异常。
        /// </summary>
        [Test]
        public void StoreReplicateInput_NullInput_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _view!.PublicStoreReplicateInput(null!),
                "Storing null input should not throw.");
        }
    }
}
