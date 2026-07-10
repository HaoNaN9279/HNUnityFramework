// NOTE: 此测试依赖 ClosedXML.dll，该 DLL 因引用校验问题暂时无法加载。
// 待用户在 Unity Plugin Inspector 中关闭 ClosedXML.dll + DocumentFormat.OpenXml.dll 的
// "Validate References" 后，将 #if false 改为 #if true 即可启用全部测试。
#if false
#nullable enable

using System;
using System.IO;
using ClosedXML.Excel;
using HN.Framework.Editor.Sheet;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests.Capability.Sheet
{
    /// <summary>
    /// <see cref="SchemaProvider"/> 的单元测试。
    /// </summary>
    [TestFixture]
    public class SchemaProviderTests
    {
        private string _tempFilePath = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        [Test]
        public void ParseExcelSchema_ValidHeadersAndTypes_ReturnsCorrectColumnDefs()
        {
            // Arrange
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Schema");
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = "Name";
                ws.Cell(1, 3).Value = "Health";
                ws.Cell(1, 4).Value = "IsBoss";
                ws.Cell(2, 1).Value = "INT";
                ws.Cell(2, 2).Value = "STRING";
                ws.Cell(2, 3).Value = "FLOAT";
                ws.Cell(2, 4).Value = "BOOL";
            });

            // Act
            var columns = SchemaProvider.ParseExcelSchema(_tempFilePath);

            // Assert
            Assert.That(columns.Length, Is.EqualTo(4));
            Assert.That(columns[0].Name, Is.EqualTo("Id"));
            Assert.That(columns[0].Type, Is.EqualTo("INT"));
            Assert.That(columns[1].Name, Is.EqualTo("Name"));
            Assert.That(columns[1].Type, Is.EqualTo("STRING"));
            Assert.That(columns[2].Name, Is.EqualTo("Health"));
            Assert.That(columns[2].Type, Is.EqualTo("FLOAT"));
            Assert.That(columns[3].Name, Is.EqualTo("IsBoss"));
            Assert.That(columns[3].Type, Is.EqualTo("BOOL"));
        }

        [Test]
        public void ParseExcelSchema_AssetRefType_IsAssetRefTrue()
        {
            // Arrange
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Assets");
                ws.Cell(1, 1).Value = "Icon";
                ws.Cell(1, 2).Value = "Prefab";
                ws.Cell(1, 3).Value = "Sound";
                ws.Cell(2, 1).Value = "AssetRef(Sprite)";
                ws.Cell(2, 2).Value = "AssetRef(GameObject)";
                ws.Cell(2, 3).Value = "AssetRef(AudioClip)";
            });

            // Act
            var columns = SchemaProvider.ParseExcelSchema(_tempFilePath);

            // Assert
            Assert.That(columns.Length, Is.EqualTo(3));
            Assert.That(columns[0].IsAssetRef, Is.True);
            Assert.That(columns[0].Type, Is.EqualTo("AssetRef(Sprite)"));
            Assert.That(columns[1].IsAssetRef, Is.True);
            Assert.That(columns[1].Type, Is.EqualTo("AssetRef(GameObject)"));
            Assert.That(columns[2].IsAssetRef, Is.True);
            Assert.That(columns[2].Type, Is.EqualTo("AssetRef(AudioClip)"));
        }

        [Test]
        public void ParseExcelSchema_EmptyColumns_TruncatesAtFirstEmpty()
        {
            // Arrange: 3 个有效列，但第 4 个 header 为空
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Partial");
                ws.Cell(1, 1).Value = "A";
                ws.Cell(1, 2).Value = "B";
                ws.Cell(1, 3).Value = "C";
                // Column 4: empty header
                ws.Cell(1, 5).Value = "E"; // 跳过空列后的列也会被忽略
                ws.Cell(2, 1).Value = "INT";
                ws.Cell(2, 2).Value = "STRING";
                ws.Cell(2, 3).Value = "INT";
                ws.Cell(2, 5).Value = "INT";
            });

            // Act
            var columns = SchemaProvider.ParseExcelSchema(_tempFilePath);

            // Assert: 只解析到第 3 列（第 4 列为空，停止）
            Assert.That(columns.Length, Is.EqualTo(3));
            Assert.That(columns[0].Name, Is.EqualTo("A"));
            Assert.That(columns[1].Name, Is.EqualTo("B"));
            Assert.That(columns[2].Name, Is.EqualTo("C"));
        }

        [Test]
        public void ParseExcelSchema_NoTypeRow_ReturnsColumnsWithEmptyType()
        {
            // Arrange: 没有第 2 行类型标注
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("NoTypes");
                ws.Cell(1, 1).Value = "X";
                ws.Cell(1, 2).Value = "Y";
            });

            // Act
            var columns = SchemaProvider.ParseExcelSchema(_tempFilePath);

            // Assert
            Assert.That(columns.Length, Is.EqualTo(2));
            Assert.That(columns[0].Name, Is.EqualTo("X"));
            Assert.That(columns[0].Type, Is.EqualTo(string.Empty));
            Assert.That(columns[1].Name, Is.EqualTo("Y"));
            Assert.That(columns[1].Type, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ParseExcelSchema_TrimsWhitespace()
        {
            // Arrange
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Trim");
                ws.Cell(1, 1).Value = "  Name  ";
                ws.Cell(2, 1).Value = "  STRING  ";
            });

            // Act
            var columns = SchemaProvider.ParseExcelSchema(_tempFilePath);

            // Assert
            Assert.That(columns[0].Name, Is.EqualTo("Name"));
            Assert.That(columns[0].Type, Is.EqualTo("STRING"));
        }

        /// <summary>
        /// 辅助方法：创建临时 xlsx 文件并调用配置回调。
        /// </summary>
        private static void CreateTestWorkbook(string filePath, Action<XLWorkbook> configure)
        {
            using var workbook = new XLWorkbook();
            configure(workbook);
            workbook.SaveAs(filePath);
        }
    }
}
#else
using NUnit.Framework;

namespace HN.Framework.Unity.Tests.Capability.Sheet
{
    [TestFixture]
    public class SchemaProviderTests
    {
        [Test]
        public void Placeholder_ClosedXML_NotAvailable()
        {
            Assert.Inconclusive("ClosedXML.dll 未加载。请在 Unity Plugin Inspector 中关闭其 Validate References 后将 #if false 改为 #if true。");
        }
    }
}
#endif
