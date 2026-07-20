using System;
using HN.Framework.Editor.EditorUI.Descriptors;
using NUnit.Framework;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Unity.Tests.EditorUI
{
    [TestFixture]
    public class DescriptorRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            // 每次测试前清空注册表
            DescriptorRegistry.Clear();
        }

        // ─── 注册 PropertyDescriptor ───────────────────────────────

        [Test]
        public void RegisterPropertyDescriptor_CanRegister()
        {
            var descriptor = new MockPropertyDescriptor();
            DescriptorRegistry.RegisterPropertyDescriptor(descriptor);

            var results = DescriptorRegistry.GetPropertyDescriptors(typeof(ReadOnlyAttribute));
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(descriptor, results[0]);
        }

        [Test]
        public void RegisterPropertyDescriptor_MultipleRegistrations()
        {
            var d1 = new MockPropertyDescriptor();
            var d2 = new MockPropertyDescriptor();
            DescriptorRegistry.RegisterPropertyDescriptor(d1);
            DescriptorRegistry.RegisterPropertyDescriptor(d2);

            var results = DescriptorRegistry.GetPropertyDescriptors(typeof(ReadOnlyAttribute));
            Assert.AreEqual(2, results.Length);
        }

        [Test]
        public void RegisterPropertyDescriptor_TypeFilter()
        {
            var d1 = new MockPropertyDescriptor();
            DescriptorRegistry.RegisterPropertyDescriptor(d1);

            // 查询不匹配的类型应该返回空
            var results = DescriptorRegistry.GetPropertyDescriptors(typeof(ButtonAttribute));
            Assert.AreEqual(0, results.Length);
        }

        [Test]
        public void RegisterPropertyDescriptor_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DescriptorRegistry.RegisterPropertyDescriptor(null));
        }

        // ─── 注册 ClassDescriptor ──────────────────────────────────

        [Test]
        public void RegisterClassDescriptor_CanRegister()
        {
            var descriptor = new MockClassDescriptor();
            DescriptorRegistry.RegisterClassDescriptor(descriptor);

            var results = DescriptorRegistry.GetClassDescriptors(typeof(FoldoutGroupAttribute));
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(descriptor, results[0]);
        }

        [Test]
        public void RegisterClassDescriptor_MultipleRegistrations()
        {
            var d1 = new MockClassDescriptor();
            var d2 = new MockClassDescriptor();
            DescriptorRegistry.RegisterClassDescriptor(d1);
            DescriptorRegistry.RegisterClassDescriptor(d2);

            var results = DescriptorRegistry.GetClassDescriptors(typeof(FoldoutGroupAttribute));
            Assert.AreEqual(2, results.Length);
        }

        [Test]
        public void RegisterClassDescriptor_OrderSorting()
        {
            var d1 = new MockClassDescriptor { OrderValue = 10 };
            var d2 = new MockClassDescriptor { OrderValue = 5 };
            var d3 = new MockClassDescriptor { OrderValue = 20 };

            DescriptorRegistry.RegisterClassDescriptor(d1);
            DescriptorRegistry.RegisterClassDescriptor(d2);
            DescriptorRegistry.RegisterClassDescriptor(d3);

            var results = DescriptorRegistry.GetClassDescriptors(typeof(FoldoutGroupAttribute));
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(5, results[0].Order);
            Assert.AreEqual(10, results[1].Order);
            Assert.AreEqual(20, results[2].Order);
        }

        [Test]
        public void RegisterClassDescriptor_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DescriptorRegistry.RegisterClassDescriptor(null));
        }

        // ─── GetAll ────────────────────────────────────────────────

        [Test]
        public void GetAllPropertyDescriptors_Empty()
        {
            var results = DescriptorRegistry.GetAllPropertyDescriptors();
            Assert.AreEqual(0, results.Length);
        }

        [Test]
        public void GetAllClassDescriptors_Empty()
        {
            var results = DescriptorRegistry.GetAllClassDescriptors();
            Assert.AreEqual(0, results.Length);
        }

        [Test]
        public void GetAllPropertyDescriptors_AfterRegistration()
        {
            DescriptorRegistry.RegisterPropertyDescriptor(new MockPropertyDescriptor());
            var results = DescriptorRegistry.GetAllPropertyDescriptors();
            Assert.AreEqual(1, results.Length);
        }

        // ─── Clear ─────────────────────────────────────────────────

        [Test]
        public void Clear_RemovesAllRegistrations()
        {
            DescriptorRegistry.RegisterPropertyDescriptor(new MockPropertyDescriptor());
            DescriptorRegistry.RegisterClassDescriptor(new MockClassDescriptor());

            DescriptorRegistry.Clear();

            Assert.AreEqual(0, DescriptorRegistry.GetAllPropertyDescriptors().Length);
            Assert.AreEqual(0, DescriptorRegistry.GetAllClassDescriptors().Length);
        }

        // ─── 缓存重建 ──────────────────────────────────────────────

        [Test]
        public void CacheRebuild_AfterNewRegistration()
        {
            DescriptorRegistry.RegisterPropertyDescriptor(new MockPropertyDescriptor());
            var before = DescriptorRegistry.GetPropertyDescriptors(typeof(ReadOnlyAttribute));
            Assert.AreEqual(1, before.Length);

            DescriptorRegistry.RegisterPropertyDescriptor(new MockPropertyDescriptor());
            var after = DescriptorRegistry.GetPropertyDescriptors(typeof(ReadOnlyAttribute));
            Assert.AreEqual(2, after.Length);
        }

        [Test]
        public void QueryUnregisteredType_ReturnsEmpty()
        {
            var results = DescriptorRegistry.GetPropertyDescriptors(typeof(TitleAttribute));
            Assert.AreEqual(0, results.Length);
        }

        // ─── Mock Implementations ──────────────────────────────────

        private class MockPropertyDescriptor : IPropertyDescriptor
        {
            public Type[] SupportedAttributeTypes => new[] { typeof(ReadOnlyAttribute) };

            public void BeforeField(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
                Attribute attribute, UnityEngine.GUIContent label) { }

            public void AfterField(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
                Attribute attribute, UnityEngine.GUIContent label) { }
        }

        private class MockClassDescriptor : IClassDescriptor
        {
            public Type[] SupportedAttributeTypes => new[] { typeof(FoldoutGroupAttribute) };
            public int OrderValue { get; set; } = 0;
            public int Order => OrderValue;

            public void OnGroupBegin(UnityEditor.SerializedProperty property, Attribute[] attributes) { }
            public void BeforeClass() { }
            public void AfterClass() { }
            public void OnGroupEnd(UnityEditor.SerializedProperty property, Attribute[] attributes) { }
        }
    }
}
