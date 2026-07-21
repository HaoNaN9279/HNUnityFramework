#nullable enable

using FishNet.Object;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    /// <summary>
    /// NetworkEntityView 的 EditMode 测试。验证基类的属性、RequireComponent 行为及生命周期方法。
    /// </summary>
    [TestFixture]
    public class NetworkEntityViewTests
    {
        private GameObject? _gameObject;
        private TestNetworkEntityView? _entity;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("NetworkEntityViewTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        [TearDown]
        public void TearDown()
        {
            _entity = null;

            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        #region Component Requirements

        /// <summary>
        /// 添加 NetworkEntityView 时自动添加 NetworkObject 组件（RequireComponent）。
        /// </summary>
        [Test]
        public void Create_GameObject_With_NetworkEntityView_AutoAdds_NetworkObject()
        {
            _entity = _gameObject!.AddComponent<TestNetworkEntityView>();

            var networkObject = _gameObject!.GetComponent<NetworkObject>();
            Assert.That(networkObject, Is.Not.Null,
                "NetworkObject should be auto-added via RequireComponent.");
        }

        #endregion

        #region NetId

        /// <summary>
        /// 未生成（Unspawned）状态下 NetId 应为 0。
        /// </summary>
        [Test]
        public void NetId_OnUnspawned_ReturnsZero()
        {
            _entity = _gameObject!.AddComponent<TestNetworkEntityView>();

            Assert.That(_entity.NetId, Is.EqualTo(0),
                "NetId should be 0 when the entity is not spawned on the network.");
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// 调用 OnSpawned 和 OnDespawned 方法验证生命周期方法可被调用。
        /// </summary>
        [Test]
        public void OnSpawned_OnDespawned_Called()
        {
            _entity = _gameObject!.AddComponent<TestNetworkEntityView>();

            Assert.That(_entity.SpawnedCalled, Is.False,
                "SpawnedCalled should be false before OnSpawned.");
            Assert.That(_entity.DespawnedCalled, Is.False,
                "DespawnedCalled should be false before OnDespawned.");

            _entity.OnSpawned();
            Assert.That(_entity.SpawnedCalled, Is.True,
                "SpawnedCalled should be true after OnSpawned.");

            _entity.OnDespawned();
            Assert.That(_entity.DespawnedCalled, Is.True,
                "DespawnedCalled should be true after OnDespawned.");
        }

        #endregion

        /// <summary>
        /// NetworkEntityView 的测试用具体子类，暴露生命周期标志位。
        /// </summary>
        private sealed class TestNetworkEntityView : HN.Framework.Unity.Capability.Network.NetworkEntityView
        {
            public bool SpawnedCalled { get; private set; }
            public bool DespawnedCalled { get; private set; }

            public override void OnSpawned()
            {
                base.OnSpawned();
                SpawnedCalled = true;
            }

            public override void OnDespawned()
            {
                base.OnDespawned();
                DespawnedCalled = true;
            }
        }
    }
}
