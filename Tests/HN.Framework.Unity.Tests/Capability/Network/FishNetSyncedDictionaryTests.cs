#nullable enable

using NUnit.Framework;
using UnityEngine;
using HN.Framework.Unity.Capability.Network;
using HN.Framework.Core.Capability.Network;
using FishNet.Object;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    [TestFixture]
    public class FishNetSyncedDictionaryTests
    {
        private GameObject? _gameObject;
        private TestNetworkBehaviour? _testBehaviour;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("FishNetSyncedDictionaryTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _gameObject.AddComponent<NetworkObject>();
            _testBehaviour = _gameObject.AddComponent<TestNetworkBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            _testBehaviour = null;
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        [Test]
        public void FishNetSyncedDictionary_IsSerializable()
        {
            Assert.That(_testBehaviour, Is.Not.Null);
            Assert.That(_testBehaviour.IntDict, Is.Not.Null);
        }

        [Test]
        public void FishNetSyncedDictionary_InheritsSyncDictionary()
        {
            var dict = _testBehaviour!.IntDict;
            Assert.That(dict, Is.InstanceOf<FishNet.Object.Synchronizing.SyncDictionary<string, int>>());
        }

        [Test]
        public void FishNetSyncedDictionary_ImplementsIReadOnlyDictionary()
        {
            var dict = _testBehaviour!.IntDict;
            Assert.That(dict, Is.InstanceOf<System.Collections.Generic.IReadOnlyDictionary<string, int>>());
        }

        [Test]
        public void FishNetSyncedDictionary_DefaultCollection_IsEmpty()
        {
            var dict = _testBehaviour!.IntDict;
            Assert.That(dict.Collection, Is.Not.Null);
            Assert.That(dict.Count, Is.EqualTo(0));
        }

        [Test]
        public void FishNetSyncedDictionary_StringStringType_Works()
        {
            var dict = _testBehaviour!.StringDict;
            Assert.That(dict, Is.Not.Null);
            Assert.That(dict.Count, Is.EqualTo(0));
        }

        [Test]
        public void SyncDictChange_StructFields_Correct()
        {
            var change = new SyncDictChange<string, int>(SyncCollectionOperation.Add, "score", 100);
            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(change.Key, Is.EqualTo("score"));
            Assert.That(change.Value, Is.EqualTo(100));
        }

        private sealed class TestNetworkBehaviour : NetworkBehaviour
        {
            public FishNetSyncedDictionary<string, int> IntDict = new FishNetSyncedDictionary<string, int>();
            public FishNetSyncedDictionary<string, string> StringDict = new FishNetSyncedDictionary<string, string>();
        }
    }
}
