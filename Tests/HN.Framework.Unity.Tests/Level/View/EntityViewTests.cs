using System;
using HN.Framework.Core.Level.Logic;
using HN.Framework.Unity.Level.View;
using HN.Framework.Unity.Level.View.Binding;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.View
{
    [TestFixture]
    public class EntityViewTests
    {
        private GameObject _go;
        private EntityView _view;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestEntityView");
            _go.hideFlags = HideFlags.HideAndDontSave;
            _view = _go.AddComponent<EntityView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_view != null)
            {
                _view.Deinitialize();
            }
            if (_go != null)
            {
                UnityEngine.Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void Initialize_SetsEntityIdAndDefId()
        {
            _view.Initialize(42, 100);

            Assert.That(_view.EntityId, Is.EqualTo(42u));
            Assert.That(_view.EntityDefId, Is.EqualTo(100));
        }

        [Test]
        public void Initialize_CreatesDefaultPropertyBinder()
        {
            _view.Initialize(1, 10);

            // binder should be automatically created as DefaultPropertyBinder via reflection
            var binderProp = typeof(EntityView).GetProperty("Binder",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.That(binderProp, Is.Not.Null);
            var binder = binderProp!.GetValue(_view);
            Assert.That(binder, Is.InstanceOf<DefaultPropertyBinder>());
        }

        [Test]
        public void Initialize_CallsOnSpawned()
        {
            bool wasCalled = false;
            var testView = _go.AddComponent<TestEntityView>();
            testView.OnSpawnedAction = () => wasCalled = true;

            testView.Initialize(1, 10);

            Assert.That(wasCalled, Is.True);
        }

        [Test]
        public void Deinitialize_CallsOnDespawned()
        {
            bool wasCalled = false;
            var testView = _go.AddComponent<TestEntityView>();
            testView.OnDespawnedAction = () => wasCalled = true;
            testView.Initialize(1, 10);

            testView.Deinitialize();

            Assert.That(wasCalled, Is.True);
        }

        [Test]
        public void Deinitialize_ResetsEntityIdToZero()
        {
            _view.Initialize(42, 100);
            Assert.That(_view.EntityId, Is.EqualTo(42u));

            _view.Deinitialize();

            Assert.That(_view.EntityId, Is.EqualTo(0u));
        }

        [Test]
        public void Deinitialize_CallsUnbindAll_OnBinder()
        {
            var mockBinder = new MockPropertyBinder();
            var testView = _go.AddComponent<TestEntityView>();
            testView.BindData(mockBinder);
            testView.Initialize(1, 10);

            Assert.That(testView.BinderWasBound, Is.True);

            testView.Deinitialize();

            Assert.That(mockBinder.UnbindAllCalled, Is.True);
        }

        /// <summary>
        /// Mock that tracks whether UnbindAll was called.
        /// </summary>
        private sealed class MockPropertyBinder : PropertyBinder
        {
            public bool UnbindAllCalled { get; private set; }

            public override void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged)
            {
                // No-op for test
            }

            public override void UnbindAll()
            {
                UnbindAllCalled = true;
            }
        }

        /// <summary>
        /// Testable EntityView subclass for monitoring lifecycle callbacks.
        /// </summary>
        private class TestEntityView : EntityView
        {
            public System.Action? OnSpawnedAction { get; set; }
            public System.Action? OnDespawnedAction { get; set; }
            public bool BinderWasBound { get; private set; }
            public bool UnbindWasCalled { get; private set; }

            public override void BindData(PropertyBinder binderInstance)
            {
                BinderWasBound = true;
                base.BindData(binderInstance);
            }

            protected override void OnSpawned()
            {
                OnSpawnedAction?.Invoke();
            }

            protected override void OnDespawned()
            {
                OnDespawnedAction?.Invoke();
                UnbindWasCalled = true;
            }
        }
    }
}
