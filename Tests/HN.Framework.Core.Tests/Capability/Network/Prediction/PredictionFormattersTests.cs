using NUnit.Framework;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Capability.Network.Prediction;
using HN.Framework.Core.Capability.Serialization;

namespace HN.Framework.Core.Tests.Capability.Network.Prediction
{
    [TestFixture]
    public class PredictionFormattersTests
    {
        [SetUp]
        public void SetUp()
        {
            FormattersInitializer.RegisterAll();
        }

        [Test]
        public void PredictionReconcileData_Byte_Roundtrip()
        {
            var original = new PredictionReconcileData<byte>
            {
                ClientTick = 1,
                ServerTick = 2,
                AuthoritativeState = 128
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<byte>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(1u));
            Assert.That(result.ServerTick, Is.EqualTo(2u));
            Assert.That(result.AuthoritativeState, Is.EqualTo(128));
        }

        [Test]
        public void PredictionReconcileData_Int_Roundtrip()
        {
            var original = new PredictionReconcileData<int>
            {
                ClientTick = 100,
                ServerTick = 200,
                AuthoritativeState = -42
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<int>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(100u));
            Assert.That(result.ServerTick, Is.EqualTo(200u));
            Assert.That(result.AuthoritativeState, Is.EqualTo(-42));
        }

        [Test]
        public void PredictionReconcileData_Float_Roundtrip()
        {
            var original = new PredictionReconcileData<float>
            {
                ClientTick = 0,
                ServerTick = 1,
                AuthoritativeState = 3.14159f
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<float>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(0u));
            Assert.That(result.ServerTick, Is.EqualTo(1u));
            Assert.That(result.AuthoritativeState, Is.EqualTo(3.14159f));
        }

        [Test]
        public void PredictionReconcileData_BoundaryValues()
        {
            var original = new PredictionReconcileData<uint>
            {
                ClientTick = uint.MaxValue,
                ServerTick = uint.MaxValue,
                AuthoritativeState = uint.MinValue
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<uint>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(uint.MaxValue));
            Assert.That(result.ServerTick, Is.EqualTo(uint.MaxValue));
            Assert.That(result.AuthoritativeState, Is.EqualTo(uint.MinValue));
        }

        [Test]
        public void PredictionReconcileData_Long_Roundtrip()
        {
            var original = new PredictionReconcileData<long>
            {
                ClientTick = 1,
                ServerTick = 2,
                AuthoritativeState = long.MaxValue
            };

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<PredictionReconcileData<long>>(data);

            Assert.That(result.ClientTick, Is.EqualTo(1u));
            Assert.That(result.ServerTick, Is.EqualTo(2u));
            Assert.That(result.AuthoritativeState, Is.EqualTo(long.MaxValue));
        }
    }
}
