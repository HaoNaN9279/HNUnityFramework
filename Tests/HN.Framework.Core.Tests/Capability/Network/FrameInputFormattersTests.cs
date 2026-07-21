using System.Collections.Generic;
using FixedMathSharp;
using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability.Serialization;

namespace HN.Framework.Core.Tests.Capability.Network
{
    [TestFixture]
    public class FrameInputFormattersTests
    {
        [SetUp]
        public void SetUp()
        {
            FormattersInitializer.RegisterAll();
        }

        [Test]
        public void Serialize_Deserialize_Roundtrip()
        {
            var original = new FrameInput(100);
            original.AddAction("Horizontal", Fixed64.FromDouble(1.5));
            original.AddAction("Vertical", Fixed64.FromDouble(-0.5));
            original.AddAction("Jump", new Fixed64(1));

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FrameInput>(data);

            Assert.That(result.FrameNumber, Is.EqualTo(100ul));
            Assert.That(result.Actions, Is.Not.Null);
            Assert.That(result.Actions.Count, Is.EqualTo(3));
            Assert.That(result.Actions["Horizontal"], Is.EqualTo(Fixed64.FromDouble(1.5)));
            Assert.That(result.Actions["Vertical"], Is.EqualTo(Fixed64.FromDouble(-0.5)));
            Assert.That(result.Actions["Jump"], Is.EqualTo(new Fixed64(1)));
        }

        [Test]
        public void Serialize_Deserialize_EmptyActions()
        {
            var original = new FrameInput(42);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FrameInput>(data);

            Assert.That(result.FrameNumber, Is.EqualTo(42ul));
            Assert.That(result.Actions, Is.Not.Null);
            Assert.That(result.Actions.Count, Is.EqualTo(0));
        }

        [Test]
        public void Serialize_Deserialize_SingleAction()
        {
            var original = new FrameInput(1);
            original.AddAction("Fire", new Fixed64(1));

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FrameInput>(data);

            Assert.That(result.FrameNumber, Is.EqualTo(1ul));
            Assert.That(result.Actions.Count, Is.EqualTo(1));
            Assert.That(result.Actions["Fire"], Is.EqualTo(new Fixed64(1)));
        }

        [Test]
        public void Serialize_Deserialize_LargeFrameNumber()
        {
            var original = new FrameInput(ulong.MaxValue);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FrameInput>(data);

            Assert.That(result.FrameNumber, Is.EqualTo(ulong.MaxValue));
        }

        [Test]
        public void Serialize_Deserialize_LargeDict()
        {
            var original = new FrameInput(0);
            for (int i = 0; i < 100; i++)
            {
                original.AddAction($"key{i}", new Fixed64(i));
            }

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<FrameInput>(data);

            Assert.That(result.Actions.Count, Is.EqualTo(100));
            Assert.That(result.Actions["key99"], Is.EqualTo(new Fixed64(99)));
        }
    }
}
