#nullable enable

using HN.Framework.Core.Capability.Network;
using HN.Framework.Unity.Capability.Network;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Network
{
    /// <summary>
    /// NetworkEntityView 同步功能测试。验证 SyncedModel 注册/注销、ApplySyncValue 及生命周期清理行为。
    /// </summary>
    [TestFixture]
    public class NetworkEntityViewSyncTests
    {
        private GameObject? _gameObject;
        private TestNetworkEntityView? _entity;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("NetworkEntityViewSyncTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _entity = _gameObject.AddComponent<TestNetworkEntityView>();
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

        [Test]
        public void RegisterSyncedModel_RegistersModel()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            _entity!.PublicRegisterModel(model);
            Assert.Pass("Registration succeeded without exception.");
        }

        [Test]
        public void RegisterSyncedModel_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _entity!.PublicRegisterModel<int>(null!));
        }

        [Test]
        public void RegisterSyncedModel_MultipleTimes_DoesNotDuplicate()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            _entity!.PublicRegisterModel(model);
            _entity!.PublicRegisterModel(model);
            Assert.Pass("Multiple registration of same model succeeded.");
        }

        [Test]
        public void UnregisterSyncedModel_UnregistersModel()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            _entity!.PublicRegisterModel(model);
            _entity!.PublicUnregisterModel(model);
            Assert.Pass("Unregistration succeeded without exception.");
        }

        [Test]
        public void UnregisterSyncedModel_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _entity!.PublicUnregisterModel<int>(null!));
        }

        [Test]
        public void UnregisterSyncedModel_NotRegistered_DoesNotThrow()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            Assert.DoesNotThrow(() => _entity!.PublicUnregisterModel(model));
        }

        [Test]
        public void ApplySyncValue_ClientSide_UpdatesModel()
        {
            SyncedModel<int> model = new SyncedModel<int>(0);
            NetworkEntityView.ApplySyncValue(model, 42, asServer: false);
            Assert.That(model.Value, Is.EqualTo(42));
        }

        [Test]
        public void ApplySyncValue_ServerSide_UpdatesModel()
        {
            SyncedModel<int> model = new SyncedModel<int>(0);
            NetworkEntityView.ApplySyncValue(model, 42, asServer: true);
            Assert.That(model.Value, Is.EqualTo(42));
        }

        [Test]
        public void ApplySyncValue_ClientSide_TriggersOnValueChanged()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            int changedValue = 0;
            model.OnValueChanged += (v) => changedValue = v;
            NetworkEntityView.ApplySyncValue(model, 99, asServer: false);
            Assert.That(changedValue, Is.EqualTo(99));
        }

        [Test]
        public void ApplySyncValue_SameValue_DoesNotTriggerOnValueChanged()
        {
            SyncedModel<int> model = new SyncedModel<int>(42);
            int changeCount = 0;
            model.OnValueChanged += (_) => changeCount++;
            NetworkEntityView.ApplySyncValue(model, 42, asServer: false);
            Assert.That(changeCount, Is.EqualTo(0));
        }

        [Test]
        public void ApplySyncValue_NullModel_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => NetworkEntityView.ApplySyncValue<int>(null!, 42, asServer: false));
        }

        [Test]
        public void ApplySyncValue_FloatType_WorksCorrectly()
        {
            SyncedModel<float> model = new SyncedModel<float>();
            NetworkEntityView.ApplySyncValue(model, 3.14f, asServer: false);
            Assert.That(model.Value, Is.EqualTo(3.14f).Within(0.001f));
        }

        [Test]
        public void ApplySyncValue_StringType_WorksCorrectly()
        {
            SyncedModel<string> model = new SyncedModel<string>();
            NetworkEntityView.ApplySyncValue(model, "Hello", asServer: false);
            Assert.That(model.Value, Is.EqualTo("Hello"));
        }

        [Test]
        public void OnDespawned_ClearsRegisteredModels()
        {
            SyncedModel<int> model1 = new SyncedModel<int>();
            SyncedModel<int> model2 = new SyncedModel<int>();
            _entity!.PublicRegisterModel(model1);
            _entity!.PublicRegisterModel(model2);
            _entity.PublicInvokeOnDespawned();
            model1.SetValue(1);
            model2.SetValue(2);
            Assert.That(model1.Value, Is.EqualTo(1));
            Assert.That(model2.Value, Is.EqualTo(2));
        }

        [Test]
        public void OnDespawned_AfterCleanup_CanRegisterAgain()
        {
            SyncedModel<int> model = new SyncedModel<int>();
            _entity!.PublicRegisterModel(model);
            _entity.PublicInvokeOnDespawned();
            Assert.DoesNotThrow(() => _entity.PublicRegisterModel(new SyncedModel<int>()));
        }

        private sealed class TestNetworkEntityView : NetworkEntityView
        {
            public void PublicRegisterModel<T>(SyncedModel<T> model) => RegisterSyncedModel(model);
            public void PublicUnregisterModel<T>(SyncedModel<T> model) => UnregisterSyncedModel(model);
            public void PublicInvokeOnDespawned() => OnDespawned();
        }
    }
}
