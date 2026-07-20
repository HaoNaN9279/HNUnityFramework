using System;
using System.Reflection;
using HN.Framework.Editor.EditorUI.Utility;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests.EditorUI
{
    [TestFixture]
    public class ReflectionCacheTests
    {
        private class TestClass
        {
            public int FieldA;
            private string FieldB;
            protected float FieldC;
            internal bool FieldD;
            public void TestMethod() { }
        }

        [Test]
        public void GetFields_ReturnsAllFields()
        {
            var fields = ReflectionCache.GetFields(typeof(TestClass));
            Assert.AreEqual(4, fields.Length, "Should find all 4 fields (public/private/protected/internal)");
        }

        [Test]
        public void GetFields_SameType_ReturnsCachedResult()
        {
            var fields1 = ReflectionCache.GetFields(typeof(TestClass));
            var fields2 = ReflectionCache.GetFields(typeof(TestClass));
            Assert.AreSame(fields1, fields2, "Should return cached array for same type");
        }

        [Test]
        public void GetFields_NoFields_DoesNotThrow()
        {
            var fields = ReflectionCache.GetFields(typeof(int));
            Assert.IsNotNull(fields);
        }

        [Test]
        public void GetMethods_ReturnsAllMethods()
        {
            var methods = ReflectionCache.GetMethods(typeof(TestClass));
            Assert.IsNotNull(methods);
            Assert.IsTrue(methods.Length > 0);
        }

        [Test]
        public void GetMethods_SameType_ReturnsCachedResult()
        {
            var m1 = ReflectionCache.GetMethods(typeof(TestClass));
            var m2 = ReflectionCache.GetMethods(typeof(TestClass));
            Assert.AreSame(m1, m2, "Should return cached array for same type");
        }

        [Test]
        public void Clear_InvalidatesCache()
        {
            var fields1 = ReflectionCache.GetFields(typeof(TestClass));
            ReflectionCache.Clear();
            var fields2 = ReflectionCache.GetFields(typeof(TestClass));
            Assert.AreNotSame(fields1, fields2, "After Clear, GetFields should return new array");
        }

        [Test]
        public void Clear_AllCachesCleared_DoesNotThrow()
        {
            ReflectionCache.GetFields(typeof(TestClass));
            ReflectionCache.GetMethods(typeof(TestClass));
            ReflectionCache.Clear();
            var fields = ReflectionCache.GetFields(typeof(TestClass));
            var methods = ReflectionCache.GetMethods(typeof(TestClass));
            Assert.IsNotNull(fields);
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetAttributes_TypeWithNoAttributes_ReturnsEmpty()
        {
            var attrs = ReflectionCache.GetAttributes<ObsoleteAttribute>(typeof(TestClass));
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length);
        }

        private class AttributedClass
        {
            [Obsolete]
            public void ObsoleteMethod() { }
        }

        [Test]
        public void GetAttributes_WithAttribute()
        {
            var attrs = ReflectionCache.GetAttributes<ObsoleteAttribute>(typeof(AttributedClass));
            Assert.IsNotNull(attrs);
        }
    }
}
