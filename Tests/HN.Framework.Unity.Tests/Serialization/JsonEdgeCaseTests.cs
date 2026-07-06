using System;
using System.IO;
using System.Text;
using HN.Framework.Core.Driver.Common.Serialization;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests.Serialization
{
    /// <summary>
    /// JSON 序列化模块边界和错误处理测试。
    /// 覆盖：空 JSON、null 字符串、深度限制、无效 JSON、
    /// 文件缺失、Unicode 转义、枚举向后兼容、无效类型名、文件 I/O 往返。
    /// </summary>
    [TestFixture]
    public class JsonEdgeCaseTests
    {
        #region Helper Types

        /// <summary>
        /// 用于枚举向后兼容测试的枚举类型。
        /// </summary>
        public enum TestEnum
        {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2
        }

        /// <summary>
        /// 包含基本字段和枚举字段的测试数据类。
        /// </summary>
        [Serializable]
        public class SimpleData
        {
            public int IntValue;
            public string StringValue;
            public TestEnum EnumValue;
        }

        /// <summary>
        /// 用于深度嵌套序列化测试的递归类型。
        /// </summary>
        [Serializable]
        public class NestedNode
        {
            public NestedNode Child;
            public int Value;
        }

        #endregion

        #region File I/O Helpers

        private string _tempFilePath;

        /// <summary>
        /// 清理测试中创建的临时文件。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (_tempFilePath != null && File.Exists(_tempFilePath))
            {
                try
                {
                    File.Delete(_tempFilePath);
                }
                catch
                {
                    // 忽略清理错误
                }
                _tempFilePath = null;
            }
        }

        /// <summary>
        /// 创建一个空临时文件并返回路径。
        /// </summary>
        private string CreateTempFile(string content = null)
        {
            _tempFilePath = Path.GetTempFileName();
            if (content != null)
            {
                File.WriteAllText(_tempFilePath, content, Encoding.UTF8);
            }
            return _tempFilePath;
        }

        #endregion

        #region Empty JSON

        /// <summary>
        /// 测试：反序列化空 JSON "{}" 返回含默认值的对象，不崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_EmptyJson_ReturnsObjectWithDefaults()
        {
            var result = Json.DeserializeFromString<SimpleData>("{}");

            Assert.IsNotNull(result, "反序列化空 JSON 不应返回 null");
            Assert.AreEqual(0, result.IntValue, "int 字段应为默认值 0");
            Assert.IsNull(result.StringValue, "string 字段应为默认值 null");
            Assert.AreEqual(TestEnum.Value0, result.EnumValue, "enum 字段应为默认值 Value0");
        }

        /// <summary>
        /// 测试：空 JSON 反序列化覆盖已有对象时，不改变任何字段。
        /// </summary>
        [Test]
        public void DeserializeFromString_EmptyJsonOverwrite_DoesNotChangeExistingObject()
        {
            var obj = new SimpleData { IntValue = 42, StringValue = "test", EnumValue = TestEnum.Value2 };
            Json.DeserializeFromString(obj, "{}");

            Assert.AreEqual(42, obj.IntValue, "覆盖写入后 int 字段不应被改变");
            Assert.AreEqual("test", obj.StringValue, "覆盖写入后 string 字段不应被改变");
            Assert.AreEqual(TestEnum.Value2, obj.EnumValue, "覆盖写入后 enum 字段不应被改变");
        }

        #endregion

        #region Null Strings

        /// <summary>
        /// 测试：Serialize(null) 返回空字符串而非崩溃。
        /// </summary>
        [Test]
        public void Serialize_NullObject_ReturnsEmptyString()
        {
            var result = Json.Serialize(null);

            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// 测试：DeserializeFromString 传入 null 字符串时使用 "{}" 作为默认值。
        /// </summary>
        [Test]
        public void DeserializeFromString_NullJsonString_UsesEmptyBrace()
        {
            var obj = new SimpleData { IntValue = 99 };
            Json.DeserializeFromString(obj, null);

            // obj 不应被 null jsonString 影响（空 JSON 不会覆盖任何值）
            Assert.AreEqual(99, obj.IntValue, "null jsonString 不应修改已有字段值");
        }

        /// <summary>
        /// 测试：DeserializeFromString 传入空字符串时使用 "{}" 作为默认值。
        /// </summary>
        [Test]
        public void DeserializeFromString_EmptyJsonString_UsesEmptyBrace()
        {
            var obj = new SimpleData { IntValue = 99 };
            Json.DeserializeFromString(obj, "");

            Assert.AreEqual(99, obj.IntValue, "空 jsonString 不应修改已有字段值");
        }

        #endregion

        #region Depth Limit

        /// <summary>
        /// 测试：序列化超过 64 层的深度嵌套对象不会堆栈溢出，
        /// 超出深度限制的内层对象应序列化为 null。
        /// </summary>
        [Test]
        public void Serialize_DeepNestingDepth65_DoesNotOverflow()
        {
            // 构建 65 层嵌套结构：root(深度0) → .Child(深度1) → ... → .Child(深度64)
            var root = new NestedNode { Value = 0 };
            var current = root;
            for (int i = 1; i <= 64; i++)
            {
                current.Child = new NestedNode { Value = i };
                current = current.Child;
            }

            string json = null;
            Assert.DoesNotThrow(() => { json = Json.Serialize(root); },
                "序列化 65 层嵌套对象不应抛出异常");

            Assert.IsNotNull(json, "序列化结果不应为 null");
            Assert.IsTrue(json.Length > 0, "序列化结果不应为空");
        }

        /// <summary>
        /// 测试：反序列化超过 64 层的深度嵌套 JSON 不应堆栈溢出。
        /// </summary>
        [Test]
        public void DeserializeFromString_DeepNestingDepth65_DoesNotOverflow()
        {
            // 构造 65 层嵌套 JSON 字符串
            var sb = new StringBuilder();
            for (int i = 0; i < 64; i++)
            {
                sb.Append("{\"Child\":");
            }
            sb.Append("{\"Child\":null,\"Value\":64}");
            for (int i = 0; i < 64; i++)
            {
                sb.Append(",\"Value\":0}");
            }

            string deepJson = sb.ToString();

            NestedNode result = null;
            Assert.DoesNotThrow(() => { result = Json.DeserializeFromString<NestedNode>(deepJson); },
                "反序列化 65 层嵌套 JSON 不应抛出异常");

            Assert.IsNotNull(result, "反序列化结果不应为 null");
        }

        /// <summary>
        /// 测试：合法深度 10 层嵌套 JSON 能正确反序列化。
        /// 作为深度限制的基准对照测试。
        /// </summary>
        [Test]
        public void DeserializeFromString_ShallowNesting_WorksCorrectly()
        {
            string json = "{\"Child\":{\"Child\":{\"Value\":42}}}";
            var result = Json.DeserializeFromString<NestedNode>(json);

            Assert.IsNotNull(result, "10 层以内的嵌套应当正常反序列化");
            Assert.IsNotNull(result.Child, "第一层 Child 不应为 null");
            Assert.IsNotNull(result.Child.Child, "第二层 Child 不应为 null");
            Assert.AreEqual(42, result.Child.Child.Value, "最内层 Value 应正确反序列化");
        }

        #endregion

        #region Invalid JSON

        /// <summary>
        /// 测试：反序列化非 JSON 格式的纯文本不应崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_NotJsonText_ReturnsObjectWithoutCrashing()
        {
            SimpleData result = null;
            Assert.DoesNotThrow(() => { result = Json.DeserializeFromString<SimpleData>("this is not json at all"); },
                "反序列化非 JSON 文本不应抛出异常");

            Assert.IsNotNull(result, "反序列化非 JSON 文本应返回对象（使用默认值）");
        }

        /// <summary>
        /// 测试：反序列化截断的 JSON 字符串不应崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_TruncatedJson_ReturnsObjectWithoutCrashing()
        {
            SimpleData result = null;
            Assert.DoesNotThrow(() => { result = Json.DeserializeFromString<SimpleData>("{\"IntValue\":42"); },
                "反序列化截断的 JSON 不应抛出异常");

            Assert.IsNotNull(result, "反序列化截断 JSON 应返回对象（使用默认值）");
        }

        /// <summary>
        /// 测试：反序列化中缀错误的 JSON 字符串不应崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_MalformedMiddleJson_ReturnsObjectWithoutCrashing()
        {
            SimpleData result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.DeserializeFromString<SimpleData>("{\"IntValue\": 42, , \"StringValue\": \"hello\"}");
            }, "反序列化中间有语法错误的 JSON 不应抛出异常");

            Assert.IsNotNull(result, "反序列化错误 JSON 应返回对象");
        }

        /// <summary>
        /// 测试：反序列化字段值类型不匹配的 JSON（字符串赋值给 int）不应崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_WrongFieldType_DoesNotCrash()
        {
            SimpleData result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.DeserializeFromString<SimpleData>("{\"IntValue\": \"not a number\"}");
            }, "字段值类型不匹配不应抛出异常");

            Assert.IsNotNull(result, "类型不匹配时也应返回对象");
            // 转换失败时 IntValue 保持默认值 0
            Assert.AreEqual(0, result.IntValue, "转换失败的 int 字段应保持默认值");
        }

        /// <summary>
        /// 测试：反序列化包含未知字段的 JSON 不应影响已知字段。
        /// </summary>
        [Test]
        public void DeserializeFromString_UnknownFields_IgnoresThem()
        {
            string json = "{\"IntValue\": 42, \"NonExistentField\": \"ghost\", \"AnotherOne\": 123}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.IntValue, "已知字段应正确反序列化");
        }

        #endregion

        #region Missing File & File I/O Errors

        /// <summary>
        /// 测试：读取不存在的文件路径应返回空字符串，不抛出异常。
        /// </summary>
        [Test]
        public void ReadFromDisk_NonExistentFile_ReturnsEmptyString()
        {
            string nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid():N}.json");

            string result = null;
            Assert.DoesNotThrow(() => { result = Json.ReadFromDisk(nonExistentPath); },
                "读取不存在的文件不应抛出异常");

            Assert.AreEqual(string.Empty, result, "读取不存在的文件应返回空字符串");
        }

        /// <summary>
        /// 测试：WriteToDisk 传入 null 路径应返回 false。
        /// </summary>
        [Test]
        public void WriteToDisk_NullPath_ReturnsFalse()
        {
            bool result = Json.WriteToDisk(null, "test content");
            Assert.IsFalse(result, "null 路径应返回 false");
        }

        /// <summary>
        /// 测试：WriteToDisk 传入空路径应返回 false。
        /// </summary>
        [Test]
        public void WriteToDisk_EmptyPath_ReturnsFalse()
        {
            bool result = Json.WriteToDisk(string.Empty, "test content");
            Assert.IsFalse(result, "空路径应返回 false");
        }

        #endregion

        #region WriteToDisk / ReadFromDisk Roundtrip

        /// <summary>
        /// 测试：WriteToDisk 写入内容后，ReadFromDisk 能正确读回。
        /// </summary>
        [Test]
        public void WriteToDisk_ReadFromDisk_Roundtrip()
        {
            string path = CreateTempFile();
            string original = "{\"IntValue\": 42, \"StringValue\": \"hello world\"}";

            bool writeResult = Json.WriteToDisk(path, original);
            Assert.IsTrue(writeResult, "WriteToDisk 应返回 true");

            string readResult = Json.ReadFromDisk(path);
            Assert.AreEqual(original, readResult, "ReadFromDisk 应读回与写入相同的内容");
        }

        /// <summary>
        /// 测试：WriteToDisk 覆盖已有文件后，ReadFromDisk 能正确读回新内容。
        /// </summary>
        [Test]
        public void WriteToDisk_Overwrite_ReadFromDisk_ReturnsNewContent()
        {
            string path = CreateTempFile("old content");

            string newContent = "new content that is different";
            bool writeResult = Json.WriteToDisk(path, newContent);
            Assert.IsTrue(writeResult);

            string readResult = Json.ReadFromDisk(path);
            Assert.AreEqual(newContent, readResult, "覆盖写入后应读回新内容");
        }

        /// <summary>
        /// 测试：WriteToDisk 写入中文和特殊字符，ReadFromDisk 能正确读回。
        /// </summary>
        [Test]
        public void WriteToDisk_ReadFromDisk_UnicodeContent()
        {
            string path = CreateTempFile();
            string original = "{\"name\": \"你好世界\", \"emoji\": \"🚀\"}";

            Json.WriteToDisk(path, original);
            string readResult = Json.ReadFromDisk(path);

            Assert.AreEqual(original, readResult, "Unicode 内容应完整往返");
        }

        #endregion

        #region Unicode Escape \uXXXX

        /// <summary>
        /// 测试：Unicode 转义序列 \u0041 应解析为字符 'A'。
        /// 使用硬编码 JSON 字符串验证 \uXXXX 反序列化功能。
        /// </summary>
        [Test]
        public void DeserializeFromString_UnicodeEscape_RoundtripsCorrectly()
        {
            // \u0041 = 'A'
            string json = "{\"StringValue\": \"\\u0041\"}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual("A", result.StringValue,
                "\\u0041 应被解析为字符 'A'");
        }

        /// <summary>
        /// 测试：多个 Unicode 转义序列应正确解析。
        /// </summary>
        [Test]
        public void DeserializeFromString_MultipleUnicodeEscapes_RoundtripsCorrectly()
        {
            // \u0048\u0065\u006C\u006C\u006F = "Hello"
            string json = "{\"StringValue\": \"\\u0048\\u0065\\u006C\\u006C\\u006F\"}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual("Hello", result.StringValue,
                "多个 \\uXXXX 序列应被正确串联解析");
        }

        /// <summary>
        /// 测试：Unicode 转义与非转义字符混合应正确解析。
        /// </summary>
        [Test]
        public void DeserializeFromString_MixedUnicodeAndLiteral_RoundtripsCorrectly()
        {
            // "Hi A!" 其中 A 使用 \u0041
            string json = "{\"StringValue\": \"Hi \\u0041!\"}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual("Hi A!", result.StringValue,
                "Unicode 转义与普通字符混合应正确解析");
        }

        #endregion

        #region Enum Backward Compatibility

        /// <summary>
        /// 测试：旧格式的枚举字符串值 "Value1" 能正确反序列化为 TestEnum.Value1。
        /// 当枚举序列化格式从字符串改为整数后，仍需兼容旧的字符串格式。
        /// </summary>
        [Test]
        public void DeserializeFromString_EnumStringValue_ParsesCorrectly()
        {
            // 旧格式：枚举作为带引号的字符串序列化
            string json = "{\"EnumValue\": \"Value1\"}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual(TestEnum.Value1, result.EnumValue,
                "枚举字符串格式 \"Value1\" 应正确反序列化为 TestEnum.Value1");
        }

        /// <summary>
        /// 测试：枚举字符串格式 "Value2" 能正确反序列化为 TestEnum.Value2。
        /// </summary>
        [Test]
        public void DeserializeFromString_EnumStringValue2_ParsesCorrectly()
        {
            string json = "{\"EnumValue\": \"Value2\"}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual(TestEnum.Value2, result.EnumValue,
                "枚举字符串格式 \"Value2\" 应正确反序列化");
        }

        /// <summary>
        /// 测试：整数格式的枚举值 1 能正确反序列化（新格式前向兼容）。
        /// </summary>
        [Test]
        public void DeserializeFromString_EnumIntegerValue_ParsesCorrectly()
        {
            string json = "{\"EnumValue\": 1}";
            var result = Json.DeserializeFromString<SimpleData>(json);

            Assert.AreEqual(TestEnum.Value1, result.EnumValue,
                "枚举整数值 1 应正确反序列化为 TestEnum.Value1");
        }

        /// <summary>
        /// 测试：无效的枚举字符串名称在反序列化时不应崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_EnumInvalidName_DoesNotCrash()
        {
            SimpleData result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.DeserializeFromString<SimpleData>("{\"EnumValue\": \"NonExistentValue\"}");
            }, "无效的枚举名称不应抛出异常");

            Assert.IsNotNull(result);
            // 无效名称转换失败时保持默认值
            Assert.AreEqual(TestEnum.Value0, result.EnumValue,
                "无效枚举名称时应保持默认值 Value0");
        }

        #endregion

        #region Invalid Type Name

        /// <summary>
        /// 测试：DeserializeFromString 传入不存在的类型名应返回 null，不崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_InvalidTypeName_ReturnsNull()
        {
            object result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.DeserializeFromString("NonExistent.Type, NonExistent.Assembly", "{}");
            }, "无效类型名不应抛出异常");

            Assert.IsNull(result, "无效类型名应返回 null");
        }

        /// <summary>
        /// 测试：DeserializeFromString 传入 null 类型名应返回 null，不崩溃。
        /// </summary>
        [Test]
        public void DeserializeFromString_NullTypeName_ReturnsNull()
        {
            object result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.DeserializeFromString(null, "{}");
            }, "null 类型名不应抛出异常");

            Assert.IsNull(result, "null 类型名应返回 null");
        }

        #endregion

        #region Serialize Edge Cases

        /// <summary>
        /// 测试：序列化基本类型对象生成正确格式的 JSON。
        /// </summary>
        [Test]
        public void Serialize_ValidObject_ProducesValidJson()
        {
            var obj = new SimpleData { IntValue = 10, StringValue = "test" };
            string json = Json.Serialize(obj);

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"IntValue\""), "应包含 IntValue 键");
            Assert.IsTrue(json.Contains("10"), "应包含值 10");
            Assert.IsTrue(json.Contains("\"StringValue\""), "应包含 StringValue 键");
            Assert.IsTrue(json.Contains("\"test\""), "应包含值 test");
        }

        /// <summary>
        /// 测试：序列化包含特殊字符的字符串时正确转义。
        /// </summary>
        [Test]
        public void Serialize_StringWithSpecialChars_EscapesCorrectly()
        {
            var obj = new SimpleData { StringValue = "line1\nline2\t\"quoted\"" };
            string json = Json.Serialize(obj);

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\\n"), "换行符应被转义为 \\n");
            Assert.IsTrue(json.Contains("\\t"), "制表符应被转义为 \\t");
            Assert.IsTrue(json.Contains("\\\""), "双引号应被转义");
        }

        #endregion

        #region Deserialize<T>(string path) Edge Cases

        /// <summary>
        /// 测试：Deserialize 从有效 JSON 文件正确反序列化。
        /// </summary>
        [Test]
        public void Deserialize_ValidJsonFile_ReturnsPopulatedObject()
        {
            string path = CreateTempFile("{\"IntValue\": 42, \"StringValue\": \"from file\"}");
            var result = Json.Deserialize<SimpleData>(path);

            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.IntValue, "文件中的 IntValue 应被正确反序列化");
            Assert.AreEqual("from file", result.StringValue, "文件中的 StringValue 应被正确反序列化");
        }

        /// <summary>
        /// 测试：Deserialize 从不存在的文件应安全返回默认值。
        /// </summary>
        [Test]
        public void Deserialize_MissingFile_ReturnsDefault()
        {
            string nonExistentPath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json");

            SimpleData result = null;
            Assert.DoesNotThrow(() =>
            {
                result = Json.Deserialize<SimpleData>(nonExistentPath);
            }, "从缺失文件反序列化不应抛出异常");

            Assert.AreEqual(default(SimpleData), result, "缺失文件反序列化应返回 default");
        }

        #endregion

        #region Deserialize<T>(T obj, string path) Edge Cases

        /// <summary>
        /// 测试：Deserialize 覆盖模式从有效 JSON 文件正确更新对象。
        /// </summary>
        [Test]
        public void DeserializeOverwrite_ValidJsonFile_UpdatesObject()
        {
            string path = CreateTempFile("{\"IntValue\": 77, \"StringValue\": \"overwritten\"}");
            var obj = new SimpleData { IntValue = 0, StringValue = null };

            bool result = Json.Deserialize(obj, path);

            Assert.IsTrue(result, "有效文件反序列化应返回 true");
            Assert.AreEqual(77, obj.IntValue, "IntValue 应被覆盖为文件中的值");
            Assert.AreEqual("overwritten", obj.StringValue, "StringValue 应被覆盖为文件中的值");
        }

        /// <summary>
        /// 测试：Deserialize 覆盖模式传入 null 路径应返回 false。
        /// </summary>
        [Test]
        public void DeserializeOverwrite_NullPath_ReturnsFalse()
        {
            var obj = new SimpleData();
            bool result = Json.Deserialize(obj, (string)null);

            Assert.IsFalse(result, "null 路径应返回 false");
        }

        /// <summary>
        /// 测试：Deserialize 覆盖模式传入 null 对象应返回 false。
        /// </summary>
        [Test]
        public void DeserializeOverwrite_NullObject_ReturnsFalse()
        {
            bool result = Json.Deserialize<SimpleData>(null, "some/path.json");
            Assert.IsFalse(result, "null 对象应返回 false");
        }

        #endregion
    }
}
