using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common.Serialization;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests
{
    /// <summary>
    /// EditMode 单元测试，覆盖 Json 序列化往返（serialize → deserialize → equality）：
    /// 基本类型、枚举、数组、List、Dictionary、嵌套对象、null 处理。
    /// 当前大部分测试预期会 FAIL（待 Json.cs 修复后通过）。
    /// </summary>
    [TestFixture]
    public class JsonRoundtripTests
    {
        #region Helper Types

        /// <summary>
        /// 用于枚举往返测试的枚举类型。
        /// </summary>
        public enum TestEnum
        {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2,
            Value100 = 100
        }

        /// <summary>
        /// 嵌套对象的内部数据类型。
        /// </summary>
        [Serializable]
        public class InnerData
        {
            public int Value;
            public string Name;
        }

        /// <summary>
        /// 嵌套对象的外部数据类型，包含内部对象字段。
        /// </summary>
        [Serializable]
        public class OuterData
        {
            public int Id;
            public string Label;
            public InnerData Inner;
        }

        /// <summary>
        /// 用于测试 null 字段往返的数据类型。
        /// </summary>
        [Serializable]
        public class NullableFieldsData
        {
            public string StringField;
            public int? NullableIntField;
            public InnerData ObjectField;
        }

        /// <summary>
        /// 用于测试基本类型字段往返的简单数据类型。
        /// </summary>
        [Serializable]
        public class SimpleData
        {
            public int IntValue;
            public float FloatValue;
            public double DoubleValue;
            public bool BoolValue;
            public string StringValue;
        }

        #endregion

        // ====================================================================
        // A. Primitive Types — Serialize / Deserialize Roundtrip
        // ====================================================================

        #region Primitive Types

        [Test]
        public void SerializeDeserialize_Int_Roundtrip()
        {
            int original = 42;
            string json = Json.Serialize(original);
            int result = Json.DeserializeFromString<int>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Float_Roundtrip()
        {
            float original = 3.14f;
            string json = Json.Serialize(original);
            float result = Json.DeserializeFromString<float>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Double_Roundtrip()
        {
            double original = 2.718281828;
            string json = Json.Serialize(original);
            double result = Json.DeserializeFromString<double>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_String_Roundtrip()
        {
            string original = "hello world";
            string json = Json.Serialize(original);
            string result = Json.DeserializeFromString<string>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Bool_Roundtrip()
        {
            bool original = true;
            string json = Json.Serialize(original);
            bool result = Json.DeserializeFromString<bool>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Decimal_Roundtrip()
        {
            decimal original = 123.456m;
            string json = Json.Serialize(original);
            decimal result = Json.DeserializeFromString<decimal>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Long_Roundtrip()
        {
            long original = 9223372036854775807;
            string json = Json.Serialize(original);
            long result = Json.DeserializeFromString<long>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Short_Roundtrip()
        {
            short original = 32767;
            string json = Json.Serialize(original);
            short result = Json.DeserializeFromString<short>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Byte_Roundtrip()
        {
            byte original = 255;
            string json = Json.Serialize(original);
            byte result = Json.DeserializeFromString<byte>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_UInt_Roundtrip()
        {
            uint original = 4294967295;
            string json = Json.Serialize(original);
            uint result = Json.DeserializeFromString<uint>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_ULong_Roundtrip()
        {
            ulong original = 18446744073709551615;
            string json = Json.Serialize(original);
            ulong result = Json.DeserializeFromString<ulong>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_UShort_Roundtrip()
        {
            ushort original = 65535;
            string json = Json.Serialize(original);
            ushort result = Json.DeserializeFromString<ushort>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_SByte_Roundtrip()
        {
            sbyte original = 127;
            string json = Json.Serialize(original);
            sbyte result = Json.DeserializeFromString<sbyte>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_Char_Roundtrip()
        {
            char original = 'A';
            string json = Json.Serialize(original);
            char result = Json.DeserializeFromString<char>(json);
            Assert.AreEqual(original, result);
        }

        #endregion

        // ====================================================================
        // B. Enum — Integer Format Serialization
        // ====================================================================

        #region Enum

        [Test]
        public void SerializeDeserialize_Enum_Roundtrip()
        {
            TestEnum original = TestEnum.Value100;
            string json = Json.Serialize(original);
            TestEnum result = Json.DeserializeFromString<TestEnum>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeEnum_OutputsIntegerNotStringName()
        {
            TestEnum value = TestEnum.Value1;
            string json = Json.Serialize(value);

            // 验证输出是整数 "1"，而非带引号的字符串 "\"Value1\""
            Assert.That(json, Does.Not.Contain("Value1"),
                "枚举应序列化为整数值，而非名称字符串。");
            Assert.That(json, Does.Not.Contain("\"1\""),
                "整数不应带 JSON 引号。");

            // 验证去除空白后是纯数字
            string trimmed = json.Trim();
            int parsedInt;
            Assert.IsTrue(int.TryParse(trimmed, out parsedInt),
                $"序列化结果应为纯整数，实际为: '{trimmed}'");
            Assert.AreEqual(1, parsedInt);
        }

        [Test]
        public void DeserializeEnum_FromInteger_ReturnsCorrectValue()
        {
            string json = "2";
            TestEnum result = Json.DeserializeFromString<TestEnum>(json);
            Assert.AreEqual(TestEnum.Value2, result);
        }

        #endregion

        // ====================================================================
        // C. Arrays — Serialize / Deserialize Roundtrip
        // ====================================================================

        #region Arrays

        [Test]
        public void SerializeDeserialize_IntArray_Roundtrip()
        {
            int[] original = { 1, 2, 3, 4, 5 };
            string json = Json.Serialize(original);
            int[] result = Json.DeserializeFromString<int[]>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_StringArray_Roundtrip()
        {
            string[] original = { "alpha", "beta", "gamma" };
            string json = Json.Serialize(original);
            string[] result = Json.DeserializeFromString<string[]>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_FloatArray_Roundtrip()
        {
            float[] original = { 1.1f, 2.2f, 3.3f };
            string json = Json.Serialize(original);
            float[] result = Json.DeserializeFromString<float[]>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_EmptyArray_Roundtrip()
        {
            int[] original = Array.Empty<int>();
            string json = Json.Serialize(original);
            int[] result = Json.DeserializeFromString<int[]>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        #endregion

        // ====================================================================
        // D. List&lt;T&gt; — Serialize / Deserialize Roundtrip
        // ====================================================================

        #region List

        [Test]
        public void SerializeDeserialize_IntList_Roundtrip()
        {
            var original = new List<int> { 10, 20, 30, 40 };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<List<int>>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_StringList_Roundtrip()
        {
            var original = new List<string> { "one", "two", "three" };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<List<string>>(json);
            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeDeserialize_EmptyList_Roundtrip()
        {
            var original = new List<int>();
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<List<int>>(json);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        // ====================================================================
        // E. Dictionary&lt;K, V&gt; — Serialize / Deserialize Roundtrip
        // ====================================================================

        #region Dictionary

        [Test]
        public void SerializeDeserialize_DictionaryStringInt_Roundtrip()
        {
            var original = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<Dictionary<string, int>>(json);

            Assert.AreEqual(original.Count, result.Count);
            foreach (var kvp in original)
            {
                Assert.IsTrue(result.ContainsKey(kvp.Key),
                    $"字典应包含键 '{kvp.Key}'");
                Assert.AreEqual(kvp.Value, result[kvp.Key],
                    $"键 '{kvp.Key}' 的值不匹配");
            }
        }

        [Test]
        public void SerializeDeserialize_DictionaryIntString_Roundtrip()
        {
            var original = new Dictionary<int, string>
            {
                { 1, "alpha" },
                { 2, "beta" },
                { 3, "gamma" }
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<Dictionary<int, string>>(json);

            Assert.AreEqual(original.Count, result.Count);
            foreach (var kvp in original)
            {
                Assert.IsTrue(result.ContainsKey(kvp.Key),
                    $"字典应包含键 '{kvp.Key}'");
                Assert.AreEqual(kvp.Value, result[kvp.Key],
                    $"键 '{kvp.Key}' 的值不匹配");
            }
        }

        [Test]
        public void SerializeDeserialize_DictionaryWithNestedValue_Roundtrip()
        {
            var original = new Dictionary<string, InnerData>
            {
                { "a", new InnerData { Value = 1, Name = "first" } },
                { "b", new InnerData { Value = 2, Name = "second" } }
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<Dictionary<string, InnerData>>(json);

            Assert.AreEqual(original.Count, result.Count);
            foreach (var kvp in original)
            {
                Assert.IsTrue(result.ContainsKey(kvp.Key));
                Assert.AreEqual(kvp.Value.Value, result[kvp.Key].Value);
                Assert.AreEqual(kvp.Value.Name, result[kvp.Key].Name);
            }
        }

        [Test]
        public void SerializeDeserialize_EmptyDictionary_Roundtrip()
        {
            var original = new Dictionary<string, int>();
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<Dictionary<string, int>>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        // ====================================================================
        // F. Nested Objects — Recursive Serialization
        // ====================================================================

        #region Nested Objects

        [Test]
        public void SerializeDeserialize_NestedObject_Roundtrip()
        {
            var original = new OuterData
            {
                Id = 1,
                Label = "root",
                Inner = new InnerData
                {
                    Value = 42,
                    Name = "nested"
                }
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<OuterData>(json);

            Assert.AreEqual(original.Id, result.Id);
            Assert.AreEqual(original.Label, result.Label);
            Assert.IsNotNull(result.Inner, "嵌套对象不应为 null");
            Assert.AreEqual(original.Inner.Value, result.Inner.Value);
            Assert.AreEqual(original.Inner.Name, result.Inner.Name);
        }

        [Test]
        public void SerializeDeserialize_NestedObject_WithNullInner_Roundtrip()
        {
            var original = new OuterData
            {
                Id = 1,
                Label = "root",
                Inner = null
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<OuterData>(json);

            Assert.AreEqual(original.Id, result.Id);
            Assert.AreEqual(original.Label, result.Label);
            Assert.IsNull(result.Inner, "嵌套 null 对象应保持为 null");
        }

        [Test]
        public void SerializeDeserialize_DeeplyNestedObject_Roundtrip()
        {
            var original = new OuterData
            {
                Id = 1,
                Label = "level1",
                Inner = new InnerData
                {
                    Value = 10,
                    Name = "level2"
                }
            };

            // 将 OuterData 作为 InnerData 的嵌套：
            // OuterData { ... } → 验证两层嵌套的往返
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<OuterData>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(original.Id, result.Id);
            Assert.AreEqual(original.Inner.Value, result.Inner.Value);
            Assert.AreEqual(original.Inner.Name, result.Inner.Name);
        }

        #endregion

        // ====================================================================
        // G. Simple Object with All Primitive Fields
        // ====================================================================

        #region Simple Object

        [Test]
        public void SerializeDeserialize_SimpleObject_Roundtrip()
        {
            var original = new SimpleData
            {
                IntValue = 42,
                FloatValue = 3.14f,
                DoubleValue = 2.718,
                BoolValue = true,
                StringValue = "test"
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual(original.IntValue, result.IntValue);
            Assert.AreEqual(original.FloatValue, result.FloatValue);
            Assert.AreEqual(original.DoubleValue, result.DoubleValue);
            Assert.AreEqual(original.BoolValue, result.BoolValue);
            Assert.AreEqual(original.StringValue, result.StringValue);
        }

        #endregion

        // ====================================================================
        // H. Null Handling
        // ====================================================================

        #region Null Handling

        [Test]
        public void SerializeNull_OutputsNullToken()
        {
            // 序列化 null 对象的字段值时，应输出 "null"
            var data = new OuterData { Id = 0, Label = null, Inner = null };
            string json = Json.Serialize(data);

            Assert.That(json, Does.Contain("\"Inner\": null"),
                "null 引用字段应序列化为 JSON null 字面量。");
            Assert.That(json, Does.Contain("\"Label\": null"),
                "null 字符串字段应序列化为 JSON null 字面量。");
        }

        [Test]
        public void DeserializeNull_ReturnsNullForReferenceFields()
        {
            // 构造包含 null 字段的 JSON
            string json = "{\n  \"Id\": 42,\n  \"Label\": null,\n  \"Inner\": null\n}";
            var result = Json.DeserializeFromString<OuterData>(json);

            Assert.AreEqual(42, result.Id);
            Assert.IsNull(result.Label, "反序列化的 null JSON 值应以 null 呈现");
            Assert.IsNull(result.Inner, "反序列化的 null JSON 对象应以 null 呈现");
        }

        [Test]
        public void SerializeDeserialize_NullFields_Roundtrip()
        {
            var original = new NullableFieldsData
            {
                StringField = null,
                NullableIntField = null,
                ObjectField = null
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<NullableFieldsData>(json);

            Assert.IsNull(result.StringField);
            Assert.IsNull(result.NullableIntField);
            Assert.IsNull(result.ObjectField);
        }

        [Test]
        public void SerializeDeserialize_NullablesWithValues_Roundtrip()
        {
            var original = new NullableFieldsData
            {
                StringField = "not null",
                NullableIntField = 123,
                ObjectField = new InnerData { Value = 7, Name = "inner" }
            };
            string json = Json.Serialize(original);
            var result = Json.DeserializeFromString<NullableFieldsData>(json);

            Assert.AreEqual(original.StringField, result.StringField);
            Assert.AreEqual(original.NullableIntField, result.NullableIntField);
            Assert.IsNotNull(result.ObjectField);
            Assert.AreEqual(original.ObjectField.Value, result.ObjectField.Value);
            Assert.AreEqual(original.ObjectField.Name, result.ObjectField.Name);
        }

        #endregion

        // ====================================================================
        // I. Boolean Roundtrip — True/False
        // ====================================================================

        #region Boolean Edge Cases

        [Test]
        public void SerializeDeserialize_BoolFalse_Roundtrip()
        {
            bool original = false;
            string json = Json.Serialize(original);
            bool result = Json.DeserializeFromString<bool>(json);
            Assert.AreEqual(original, result);
        }

        #endregion
    }
}
