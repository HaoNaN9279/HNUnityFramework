#nullable enable

using System;
using FishNet.Managing;
using HN.Framework.Unity.Capability.Network;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    /// <summary>
    /// FishNetNetworkManager 的 EditMode 测试。验证包装器对 FishNet.NetworkManager 的适配行为。
    /// </summary>
    [TestFixture]
    public class FishNetNetworkManagerTests
    {
        private GameObject? _gameObject;
        private NetworkManager? _fishNetManager;
        private FishNetNetworkManager? _wrapper;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("FishNetNetworkManagerTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _fishNetManager = _gameObject.AddComponent<NetworkManager>();
            _wrapper = new FishNetNetworkManager();
        }

        [TearDown]
        public void TearDown()
        {
            _wrapper = null;

            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }

            _fishNetManager = null;
        }

        #region Initialize

        /// <summary>
        /// 正常调用 Initialize 后，IsServer/IsClient/IsHost 不抛出异常。
        /// </summary>
        [Test]
        public void Initialize_SetsFishNetManager()
        {
            Assert.That(() => _wrapper!.Initialize(_fishNetManager!), Throws.Nothing);

            Assert.That(_wrapper!.IsServer, Is.False,
                "Should not be server after initialization without starting.");
            Assert.That(_wrapper.IsClient, Is.False,
                "Should not be client after initialization without starting.");
            Assert.That(_wrapper.IsHost, Is.False,
                "Should not be host after initialization without starting.");
        }

        /// <summary>
        /// 传入 null 时应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Initialize_Null_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _wrapper!.Initialize(null!),
                Throws.ArgumentNullException);
        }

        #endregion

        #region Properties (Not Initialized)

        /// <summary>
        /// 未初始化时 IsServer 返回 false。
        /// </summary>
        [Test]
        public void IsServer_NotInitialized_ReturnsFalse()
        {
            Assert.That(_wrapper!.IsServer, Is.False);
        }

        /// <summary>
        /// 未初始化时 IsClient 返回 false。
        /// </summary>
        [Test]
        public void IsClient_NotInitialized_ReturnsFalse()
        {
            Assert.That(_wrapper!.IsClient, Is.False);
        }

        /// <summary>
        /// 未初始化时 IsHost 返回 false。
        /// </summary>
        [Test]
        public void IsHost_NotInitialized_ReturnsFalse()
        {
            Assert.That(_wrapper!.IsHost, Is.False);
        }

        #endregion
    }
}
