#nullable enable

using NUnit.Framework;
using UnityEngine;
using HN.Framework.Unity.Capability.Network;
using HN.Framework.Core.Capability.Network;
using FishNet.Object;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    [TestFixture]
    public class FishNetSyncedListTests
    {
        private GameObject? _gameObject;
        private TestNetworkBehaviour? _testBehaviour;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("FishNetSyncedListTest")
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
        public void FishNetSyncedList_IsSerializable()
        {
            Assert.That(_testBehaviour, Is.Not.Null);
            Assert.That(_testBehaviour.IntList, Is.Not.Null);
        }

        [Test]
        public void FishNetSyncedList_InheritsSyncList()
        {
            var list = _testBehaviour!.IntList;
            Assert.That(list, Is.InstanceOf<FishNet.Object.Synchronizing.SyncList<int>>());
        }

        [Test]
        public void FishNetSyncedList_ImplementsIReadOnlyList()
        {
            var list = _testBehaviour!.IntList;
            Assert.That(list, Is.InstanceOf<System.Collections.Generic.IReadOnlyList<int>>());
        }

        [Test]
        public void FishNetSyncedList_DefaultCollection_IsEmpty()
        {
            var list = _testBehaviour!.IntList;
            Assert.That(list.Collection, Is.Not.Null);
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void FishNetSyncedList_StringType_Works()
        {
            var list = _testBehaviour!.StringList;
            Assert.That(list, Is.Not.Null);
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void SyncCollectionOperation_Map_Add_IsCorrect()
        {
            Assert.That((byte)SyncCollectionOperation.Add, Is.EqualTo(0));
        }

        [Test]
        public void SyncCollectionOperation_Map_Remove_IsCorrect()
        {
            Assert.That((byte)SyncCollectionOperation.Remove, Is.EqualTo(1));
        }

        [Test]
        public void SyncCollectionOperation_Map_Insert_IsCorrect()
        {
            Assert.That((byte)SyncCollectionOperation.Insert, Is.EqualTo(2));
        }

        [Test]
        public void SyncCollectionOperation_Map_Set_IsCorrect()
        {
            Assert.That((byte)SyncCollectionOperation.Set, Is.EqualTo(3));
        }

        [Test]
        public void SyncCollectionOperation_Map_Clear_IsCorrect()
        {
            Assert.That((byte)SyncCollectionOperation.Clear, Is.EqualTo(4));
        }

        private sealed class TestNetworkBehaviour : NetworkBehaviour
        {
            public FishNetSyncedList<int> IntList = new FishNetSyncedList<int>();
            public FishNetSyncedList<string> StringList = new FishNetSyncedList<string>();
        }
    }
}
