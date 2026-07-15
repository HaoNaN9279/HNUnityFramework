#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Core.Tests.Capability.Network
{
    [TestFixture]
    public class INetworkManagerContractTests
    {
        [Test]
        public void IsServer_Default_ReturnsFalse()
        {
            var mgr = new MockNetworkManager();
            Assert.That(mgr.IsServer, Is.False);
        }

        [Test]
        public void IsClient_Default_ReturnsFalse()
        {
            var mgr = new MockNetworkManager();
            Assert.That(mgr.IsClient, Is.False);
        }

        [Test]
        public void IsHost_Default_ReturnsFalse()
        {
            var mgr = new MockNetworkManager();
            Assert.That(mgr.IsHost, Is.False);
        }

        [Test]
        public void StartServer_UpdatesIsServer()
        {
            var mgr = new MockNetworkManager();
            mgr.StartServer();
            Assert.That(mgr.IsServer, Is.True);
        }

        [Test]
        public void StartClient_UpdatesIsClient()
        {
            var mgr = new MockNetworkManager();
            mgr.StartClient();
            Assert.That(mgr.IsClient, Is.True);
        }

        [Test]
        public void IsHost_WhenBothServerAndClient_ReturnsTrue()
        {
            var mgr = new MockNetworkManager();
            mgr.StartServer();
            mgr.StartClient();
            Assert.That(mgr.IsHost, Is.True);
        }

        [Test]
        public void StopConnection_ResetsBothFlags()
        {
            var mgr = new MockNetworkManager();
            mgr.StartServer();
            mgr.StartClient();
            mgr.StopConnection();
            Assert.That(mgr.IsClient, Is.False);
            Assert.That(mgr.IsServer, Is.False);
        }

        [Test]
        public void OnClientConnected_EventFires()
        {
            var mgr = new MockNetworkManager();
            int receivedId = -1;
            mgr.OnClientConnected += (id) => receivedId = id;
            mgr.TriggerClientConnected(42);
            Assert.That(receivedId, Is.EqualTo(42));
        }

        [Test]
        public void OnClientDisconnected_EventFires()
        {
            var mgr = new MockNetworkManager();
            int receivedId = -1;
            mgr.OnClientDisconnected += (id) => receivedId = id;
            mgr.TriggerClientDisconnected(99);
            Assert.That(receivedId, Is.EqualTo(99));
        }

        [Test]
        public void Connect_UpdatesIsClient()
        {
            var mgr = new MockNetworkManager();
            mgr.Connect();
            Assert.That(mgr.IsClient, Is.True);
        }

        [Test]
        public void Disconnect_UpdatesIsClient()
        {
            var mgr = new MockNetworkManager();
            mgr.StartClient();
            mgr.Disconnect();
            Assert.That(mgr.IsClient, Is.False);
        }

        [Test]
        public void NullEventHandler_DoesNotThrow()
        {
            var mgr = new MockNetworkManager();
            Assert.DoesNotThrow(() => mgr.TriggerClientConnected(1));
            Assert.DoesNotThrow(() => mgr.TriggerClientDisconnected(1));
        }

        [Test]
        public void LocalClientId_Default_ReturnsNegativeOne()
        {
            var mgr = new MockNetworkManager();
            Assert.That(mgr.LocalClientId, Is.EqualTo(-1));
        }

        [Test]
        public void ConnectedClientIds_Default_ReturnsEmpty()
        {
            var mgr = new MockNetworkManager();
            Assert.That(mgr.ConnectedClientIds.Count, Is.EqualTo(0));
        }

        [Test]
        public void LocalClientId_AfterSetting_ReturnsCorrectValue()
        {
            var mgr = new MockNetworkManager();
            mgr.LocalClientId = 42;
            Assert.That(mgr.LocalClientId, Is.EqualTo(42));
        }
    }

    /// <summary>
    /// INetworkManager 的测试桩实现。
    /// </summary>
    internal class MockNetworkManager : INetworkManager
    {
        public bool IsServer { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsHost => IsServer && IsClient;
        public int LocalClientId { get; set; } = -1;
        public System.Collections.Generic.IReadOnlyList<int> ConnectedClientIds { get; set; } = System.Array.Empty<int>();

        public event Action<int>? OnClientConnected;
        public event Action<int>? OnClientDisconnected;

        public void StartServer() => IsServer = true;
        public void StartClient() => IsClient = true;
        public void StopConnection() { IsServer = false; IsClient = false; }
        public void Connect() => IsClient = true;
        public void Disconnect() => IsClient = false;

        // Test helpers
        public void SetServer(bool value) => IsServer = value;
        public void SetClient(bool value) => IsClient = value;
        public void TriggerClientConnected(int clientId) => OnClientConnected?.Invoke(clientId);
        public void TriggerClientDisconnected(int clientId) => OnClientDisconnected?.Invoke(clientId);
    }
}
