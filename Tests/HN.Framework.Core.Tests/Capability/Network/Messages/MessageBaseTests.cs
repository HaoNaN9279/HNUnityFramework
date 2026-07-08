#nullable enable

using System;
using NUnit.Framework;
using MemoryPack;
using HN.Framework.Core.Capability.Network.Messages;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Capability.Network.Messages
{
    [TestFixture]
    public class MessageBaseTests
    {
        [SetUp]
        public void SetUp()
        {
            MessageFormatters.RegisterAll();
            ReferencePool.RemoveAll<ClientConnectedMessage>();
            ReferencePool.RemoveAll<ClientDisconnectedMessage>();
            ReferencePool.RemoveAll<ServerReadyMessage>();
        }

        [Test]
        public void ClientConnectedMessage_Properties_AreCorrect()
        {
            var msg = new ClientConnectedMessage(100);
            Assert.That(msg.MessageId, Is.EqualTo(1u));
            Assert.That(msg.ClientId, Is.EqualTo(100));
        }

        [Test]
        public void ClientDisconnectedMessage_Properties_AreCorrect()
        {
            var msg = new ClientDisconnectedMessage(200, "timeout");
            Assert.That(msg.MessageId, Is.EqualTo(2u));
            Assert.That(msg.ClientId, Is.EqualTo(200));
            Assert.That(msg.Reason, Is.EqualTo("timeout"));
        }

        [Test]
        public void ServerReadyMessage_MessageId_IsCorrect()
        {
            var msg = new ServerReadyMessage();
            Assert.That(msg.MessageId, Is.EqualTo(3u));
        }

        [Test]
        public void ClientConnectedMessage_Clear_ResetsFields()
        {
            var msg = new ClientConnectedMessage(42);
            msg.Clear();
            Assert.That(msg.ClientId, Is.EqualTo(0));
        }

        [Test]
        public void ClientDisconnectedMessage_Clear_ResetsFields()
        {
            var msg = new ClientDisconnectedMessage(99, "disposed");
            msg.Clear();
            Assert.That(msg.ClientId, Is.EqualTo(0));
            Assert.That(msg.Reason, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MessageIds_AreUnique()
        {
            uint[] ids = new uint[]
            {
                new ClientConnectedMessage(0).MessageId,
                new ClientDisconnectedMessage(0, "").MessageId,
                new ServerReadyMessage().MessageId,
            };
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void ClientConnectedMessage_MemoryPackRoundtrip()
        {
            var original = new ClientConnectedMessage(77);
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var deserialized = MemoryPackSerializer.Deserialize<ClientConnectedMessage>(bytes);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.MessageId, Is.EqualTo(1u));
            Assert.That(deserialized.ClientId, Is.EqualTo(77));
        }

        [Test]
        public void ClientDisconnectedMessage_MemoryPackRoundtrip()
        {
            var original = new ClientDisconnectedMessage(55, "quit");
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var deserialized = MemoryPackSerializer.Deserialize<ClientDisconnectedMessage>(bytes);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.MessageId, Is.EqualTo(2u));
            Assert.That(deserialized.ClientId, Is.EqualTo(55));
            Assert.That(deserialized.Reason, Is.EqualTo("quit"));
        }

        [Test]
        public void ServerReadyMessage_MemoryPackRoundtrip()
        {
            var original = new ServerReadyMessage();
            byte[] bytes = MemoryPackSerializer.Serialize(original);
            var deserialized = MemoryPackSerializer.Deserialize<ServerReadyMessage>(bytes);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.MessageId, Is.EqualTo(3u));
        }

        [Test]
        public void ReferencePool_AcquireAndRelease_ClientConnectedMessage()
        {
            var msg = new ClientConnectedMessage(42);
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg.ClientId, Is.EqualTo(42));

            msg.Clear();
            Assert.That(msg.ClientId, Is.EqualTo(0), "Clear() should reset ClientId");
        }

        [Test]
        public void ReferencePool_AcquireAndRelease_ClientDisconnectedMessage()
        {
            var msg = new ClientDisconnectedMessage(88, "test");
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg.ClientId, Is.EqualTo(88));
            Assert.That(msg.Reason, Is.EqualTo("test"));

            msg.Clear();
            Assert.That(msg.ClientId, Is.EqualTo(0), "Clear() should reset ClientId");
            Assert.That(msg.Reason, Is.EqualTo(string.Empty), "Clear() should reset Reason");
        }
    }
}
