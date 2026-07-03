using System;
using System.Reflection;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Unity.Driver.Platform.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Serialization
{
    /// <summary>
    /// EditMode 单元测试，覆盖 JsonData 的行为：
    /// Obj 缓存、序列化/反序列化、ISerializationCallbackReceiver 回调、构造。
    /// </summary>
    [TestFixture]
    public class JsonDataTests
    {
        #region Helper Types

        /// <summary>
        /// 用于测试的 JsonObject 子类，包含基础类型字段。
        /// </summary>
        [Serializable]
        public class TestData : JsonObject
        {
            /// <summary>整数值。</summary>
            public int Value;
            /// <summary>字符串名称。</summary>
            public string Name;
        }

        /// <summary>
        /// 另一个测试类型，用于验证多对象不冲突。
        /// </summary>
        [Serializable]
        public class OtherData : JsonObject
        {
            /// <summary>浮点值。</summary>
            public float Score;
        }

        #endregion

        #region Reflection Helpers

        /// <summary>通过反射设置 JsonData.jsonText 字段。</summary>
        private static void SetJsonText(JsonData data, string text)
        {
            var field = typeof(JsonData).GetField("jsonText",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "jsonText field should exist");
            field.SetValue(data, text);
        }

        /// <summary>通过反射获取 JsonData.jsonText 字段。</summary>
        private static string GetJsonText(JsonData data)
        {
            var field = typeof(JsonData).GetField("jsonText",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "jsonText field should exist");
            return field.GetValue(data) as string;
        }

        /// <summary>通过反射获取 JsonData.obj 字段。</summary>
        private static JsonObject GetRawObj(JsonData data)
        {
            var field = typeof(JsonData).GetField("obj",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "obj field should exist");
            return field.GetValue(data) as JsonObject;
        }

        #endregion

        #region AC-C2.1.1 — First Obj Access Deserializes jsonText

        /// <summary>
        /// AC-C2.1.1：创建 JsonData，设置 jsonText，首次访问 Obj 反序列化数据。
        /// </summary>
        [Test]
        public void Obj_FirstAccess_DeserializesJsonText()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            Assert.That(data, Is.Not.Null);

            SetJsonText(data, "{\"Value\":42,\"Name\":\"Alice\"}");

            // Act
            var result = data.Obj as TestData;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(42));
            Assert.That(result.Name, Is.EqualTo("Alice"));
        }

        /// <summary>
        /// AC-C2.1.1：空 jsonText 时 Obj 不应为 null，字段保持默认值。
        /// </summary>
        [Test]
        public void Obj_FirstAccess_EmptyJsonText_ReturnsDefaults()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            Assert.That(data, Is.Not.Null);

            SetJsonText(data, "");

            // Act
            var result = data.Obj as TestData;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(0));
            Assert.That(result.Name, Is.Null);
        }

        #endregion

        #region AC-C2.1.2 — Obj Cache (No Re-parse When jsonText Unchanged)

        /// <summary>
        /// AC-C2.1.2：首次访问 Obj 解析 jsonText，再次访问返回同一缓存对象。
        /// </summary>
        [Test]
        public void Obj_SecondAccess_ReturnsCachedSameReference()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            SetJsonText(data, "{\"Value\":10,\"Name\":\"Bob\"}");

            // Act — 首次访问
            var first = data.Obj as TestData;
            Assert.That(first, Is.Not.Null);

            // Act — 再次访问，应返回同一对象引用（缓存命中）
            var second = data.Obj as TestData;

            // Assert
            Assert.That(second, Is.SameAs(first), "Second access should return cached reference");
            Assert.That(second.Value, Is.EqualTo(10));
            Assert.That(second.Name, Is.EqualTo("Bob"));
        }

        /// <summary>
        /// AC-C2.1.2：修改 Obj 后再次访问（未调用 Serialize），应反映内存修改，
        /// 因为缓存返回同一对象引用。
        /// </summary>
        [Test]
        public void Obj_ModifyWithoutSerialize_ReflectedOnNextAccess()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            SetJsonText(data, "{\"Value\":1,\"Name\":\"Old\"}");

            var obj = data.Obj as TestData;
            Assert.That(obj, Is.Not.Null);

            // 直接修改 Obj 字段（不调用 Serialize）
            obj.Value = 999;
            obj.Name = "ModifiedInMemory";

            // Act — 再次访问 Obj，应返回同一对象，修改可见
            var objAgain = data.Obj as TestData;

            // Assert
            Assert.That(objAgain, Is.SameAs(obj));
            Assert.That(objAgain.Value, Is.EqualTo(999),
                "In-memory modifications should be visible because cache returns same reference");
            Assert.That(objAgain.Name, Is.EqualTo("ModifiedInMemory"));
        }

        #endregion

        #region AC-C2.1.3 — Cache Invalidation When jsonText Changes

        /// <summary>
        /// AC-C2.1.3：手动修改 jsonText 后，访问 Obj 应重新解析。
        /// </summary>
        [Test]
        public void Obj_AfterJsonTextChanged_Reparses()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            SetJsonText(data, "{\"Value\":50,\"Name\":\"First\"}");

            var first = data.Obj as TestData;
            Assert.That(first.Value, Is.EqualTo(50));
            Assert.That(first.Name, Is.EqualTo("First"));

            // 保存原始引用
            var originalRef = first;

            // Act — 修改 jsonText 为新 JSON
            SetJsonText(data, "{\"Value\":200,\"Name\":\"Second\"}");

            var second = data.Obj as TestData;

            // Assert — 字段已被新 JSON 覆盖
            Assert.That(second.Value, Is.EqualTo(200),
                "Obj should re-parse after jsonText was changed");
            Assert.That(second.Name, Is.EqualTo("Second"));
            Assert.That(second, Is.SameAs(originalRef),
                "DeserializeFromString updates existing object, so reference stays same");
        }

        #endregion

        #region ISerializationCallbackReceiver — Unity Serialization Callbacks

        /// <summary>
        /// OnBeforeSerialize 应填充 jsonText 为 Obj 的序列化结果。
        /// </summary>
        [Test]
        public void OnBeforeSerialize_PopulatesJsonText()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            var testObj = data.Obj as TestData;
            testObj.Value = 123;
            testObj.Name = "SerializeTest";

            var receiver = (ISerializationCallbackReceiver)data;

            // Act
            receiver.OnBeforeSerialize();

            // Assert
            var json = GetJsonText(data);
            Assert.That(json, Is.Not.Null.And.Not.Empty,
                "OnBeforeSerialize should populate jsonText");
            Assert.That(json, Does.Contain("123"),
                "jsonText should contain serialized Value");
            Assert.That(json, Does.Contain("SerializeTest"),
                "jsonText should contain serialized Name");
        }

        /// <summary>
        /// OnAfterDeserialize 应从 jsonText 填充 Obj。
        /// </summary>
        [Test]
        public void OnAfterDeserialize_RepopulatesObj()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            SetJsonText(data, "{\"Value\":77,\"Name\":\"AfterDeserialize\"}");

            // 先手动反序列化一次以初始化 obj
            var receiver = (ISerializationCallbackReceiver)data;
            receiver.OnAfterDeserialize();

            // Act
            var result = data.Obj as TestData;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(77));
            Assert.That(result.Name, Is.EqualTo("AfterDeserialize"));
        }

        /// <summary>
        /// 完整的 Unity 回调循环：序列化 → 修改源 → 反序列化，验证数据一致性。
        /// </summary>
        [Test]
        public void UnityCallbackRoundtrip_BeforeAndAfter()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            var testObj = data.Obj as TestData;
            testObj.Value = 42;
            testObj.Name = "Roundtrip";

            // Act — 模拟 Unity 序列化
            var receiver = (ISerializationCallbackReceiver)data;
            receiver.OnBeforeSerialize();

            // 记录序列化后的 jsonText
            var serializedJson = GetJsonText(data);
            Assert.That(serializedJson, Is.Not.Null.And.Not.Empty);

            // 修改 Obj 字段（模拟对象被 GC 回收或重新加载后的状态）
            testObj.Value = 0;
            testObj.Name = null;

            // Act — 模拟 Unity 反序列化（从保存的 jsonText 恢复）
            receiver.OnAfterDeserialize();

            // Assert — 数据应恢复为序列化前的状态
            Assert.That(testObj.Value, Is.EqualTo(42));
            Assert.That(testObj.Name, Is.EqualTo("Roundtrip"));
        }

        /// <summary>
        /// OnAfterDeserialize 在 jsonText 为空时，Obj 不应为 null。
        /// </summary>
        [Test]
        public void OnAfterDeserialize_EmptyJsonText_ObjNotNull()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            SetJsonText(data, "");

            // 需要先通过构造函数创建 obj，然后再清除引用
            // 绕过：先设置有效 jsonText 触发 obj 创建，再清空 jsonText 测试回调不会丢 obj
            SetJsonText(data, "{\"Value\":1,\"Name\":\"x\"}");
            var receiver = (ISerializationCallbackReceiver)data;
            receiver.OnAfterDeserialize();

            // 现在 obj 存在，清空 jsonText 再调 AfterDeserialize
            SetJsonText(data, "");
            receiver.OnAfterDeserialize();

            // Assert — 即使 jsonText 为空，obj 仍存在
            var result = data.Obj as TestData;
            Assert.That(result, Is.Not.Null);
        }

        #endregion

        #region Construction

        /// <summary>
        /// 使用有效类型构造 JsonData，验证类型元数据已设置。
        /// </summary>
        [Test]
        public void Constructor_ValidType_SetsTypeMetadata()
        {
            // Act
            var data = new JsonData(typeof(TestData));

            // Assert
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Obj, Is.Not.Null);
            Assert.That(data.Obj, Is.InstanceOf<TestData>());

            // 验证字段通过反射设置正确
            var objTypeName = typeof(JsonData).GetField("objTypeName",
                BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(data) as string;
            var objAssemblyName = typeof(JsonData).GetField("objAssemblyName",
                BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(data) as string;

            Assert.That(objTypeName, Is.EqualTo(typeof(TestData).FullName),
                "objTypeName should be set to the type's full name");
            Assert.That(objAssemblyName, Is.EqualTo(typeof(TestData).Assembly.FullName),
                "objAssemblyName should be set to the type's assembly full name");
        }

        /// <summary>
        /// 构造后 Obj 立即可用（非 null），且可序列化。
        /// </summary>
        [Test]
        public void Constructor_ObjAvailable_CanSerialize()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            var obj = data.Obj as TestData;
            obj.Value = 88;
            obj.Name = "ConstructorTest";

            // Act
            data.Serialize();

            // Assert
            var json = data.JsonText;
            Assert.That(json, Is.Not.Null.And.Not.Empty);
            Assert.That(json, Does.Contain("88"));
            Assert.That(json, Does.Contain("ConstructorTest"));
        }

        /// <summary>
        /// 使用 null 类型构造不会崩溃，Obj 为 null。
        /// </summary>
        [Test]
        public void Constructor_NullType_DoesNotCrash()
        {
            // Act
            JsonData data = null;
            Assert.DoesNotThrow(() =>
            {
                data = new JsonData(null);
            }, "Constructor with null type should not throw");

            // Assert — 构造成功但不创建对象
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Obj, Is.Null);
        }

        /// <summary>
        /// 构造后可以使用不同的 JsonObject 子类，不互相干扰。
        /// </summary>
        [Test]
        public void Constructor_DifferentTypes_Independent()
        {
            // Act
            var data1 = new JsonData(typeof(TestData));
            var obj1 = data1.Obj as TestData;
            obj1.Value = 100;
            obj1.Name = "Type1";

            var data2 = new JsonData(typeof(OtherData));
            var obj2 = data2.Obj as OtherData;
            obj2.Score = 3.14f;

            data1.Serialize();
            data2.Serialize();

            // Assert
            Assert.That(data1.JsonText, Does.Not.Contain("Score"),
                "JsonData instances should be independent");
            Assert.That(data2.JsonText, Does.Not.Contain("Name"));
        }

        #endregion

        #region Serialize / Deserialize Roundtrip

        /// <summary>
        /// 对 Obj 赋值后 Serialize，再通过 jsonText 重新获取 Obj，
        /// 验证数据完整保留。
        /// </summary>
        [Test]
        public void SerializeThenObjAccess_PreservesData()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            var obj = data.Obj as TestData;
            obj.Value = 256;
            obj.Name = "RoundtripData";

            // Act — 序列化
            data.Serialize();
            var jsonAfterSerialize = data.JsonText;

            // 直接修改 Obj 字段
            obj.Value = 0;
            obj.Name = null;

            // 通过修改 jsonText 触发反序列化（模拟从文件或网络加载）
            SetJsonText(data, jsonAfterSerialize);
            var restored = data.Obj as TestData;

            // Assert
            Assert.That(restored.Value, Is.EqualTo(256));
            Assert.That(restored.Name, Is.EqualTo("RoundtripData"));
        }

        #endregion

        #region Obj with Different jsonText States

        /// <summary>
        /// null jsonText 通过 Obj getter 不应崩溃。
        /// </summary>
        [Test]
        public void Obj_NullJsonText_DoesNotCrash()
        {
            // Arrange
            var data = new JsonData(typeof(TestData));
            Assert.That(data, Is.Not.Null);

            // 构造时 jsonText 为空字符串（默认），设为 null
            SetJsonText(data, null);

            // Act & Assert
            TestData result = null;
            Assert.DoesNotThrow(() =>
            {
                result = data.Obj as TestData;
            }, "Accessing Obj with null jsonText should not throw");

            Assert.That(result, Is.Not.Null,
                "Obj should still be available even with null jsonText");
        }

        #endregion
    }
}
