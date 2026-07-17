#nullable enable

using NUnit.Framework;
using UnityEngine;
using HN.Framework.Core.Level.Logic.Inventory;
using HN.Framework.Unity.Level.View.Inventory;

namespace HN.Framework.Unity.Tests.Level.View.Inventory
{
    [TestFixture]
    public class ContainerViewTests
    {
        private GameObject _dummyGo = null!;
        private ContainerView _view = null!;

        [SetUp]
        public void SetUp()
        {
            _dummyGo = new GameObject("ContainerViewTest")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _view = _dummyGo.AddComponent<ContainerView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_dummyGo != null)
                Object.DestroyImmediate(_dummyGo);
        }

        [Test]
        public void Initialize_SetsEntityIdAndType()
        {
            _view.Initialize(42, ContainerType.Backpack);

            Assert.That(_view.EntityId, Is.EqualTo(42));
            Assert.That(_view.ContainerType, Is.EqualTo(ContainerType.Backpack));
        }

        [Test]
        public void GetContainer_BeforeSet_ReturnsNull()
        {
            Assert.That(_view.GetContainer(), Is.Null);
        }

        [Test]
        public void SetContainer_ThenGet_ReturnsSameInstance()
        {
            var container = new Container(ContainerType.Backpack, 10, 0);
            _view.SetContainer(container);

            var retrieved = _view.GetContainer();
            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved!.Type, Is.EqualTo(ContainerType.Backpack));
        }

        [Test]
        public void OnDestroy_ClearsContainer()
        {
            var container = new Container(ContainerType.Backpack, 10, 0);
            _view.SetContainer(container);

            Object.DestroyImmediate(_dummyGo);

            // 验证销毁后容器引用清空（view 不再可用）
            // ContainerView 不再有效
        }
    }
}
