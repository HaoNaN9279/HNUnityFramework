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
    /// <see cref="ExcelSourceParser"/> 的单元测试。
    /// </summary>
    [TestFixture]
    public class ExcelParserTests
    {
        private string _tempFilePath = string.Empty;
        private ExcelSourceParser _parser = null!;

        [SetUp]
        public void SetUp()
        {
            _parser = new ExcelSourceParser();
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
        public void Parse_ValidFile_ReturnsCorrectColumnsAndRows()
        {
            // Arrange: 创建一个包含 3 列 2 行数据的 xlsx
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Test");
                // Row 1: headers
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = "Name";
                ws.Cell(1, 3).Value = "Value";
                // Row 2: types
                ws.Cell(2, 1).Value = "INT";
                ws.Cell(2, 2).Value = "STRING";
                ws.Cell(2, 3).Value = "FLOAT";
                // Row 3: data
                ws.Cell(3, 1).Value = "1";
                ws.Cell(3, 2).Value = "Alice";
                ws.Cell(3, 3).Value = "3.14";
                // Row 4: data
                ws.Cell(4, 1).Value = "2";
                ws.Cell(4, 2).Value = "Bob";
                ws.Cell(4, 3).Value = "2.71";
            });

            // Act
            var model = _parser.Parse(_tempFilePath);

            // Assert
            Assert.That(model.Columns.Length, Is.EqualTo(3));
            Assert.That(model.Columns[0].Name, Is.EqualTo("Id"));
            Assert.That(model.Columns[0].Type, Is.EqualTo("INT"));
            Assert.That(model.Columns[1].Name, Is.EqualTo("Name"));
            Assert.That(model.Columns[1].Type, Is.EqualTo("STRING"));
            Assert.That(model.Columns[2].Name, Is.EqualTo("Value"));
            Assert.That(model.Columns[2].Type, Is.EqualTo("FLOAT"));

            Assert.That(model.Rows.Length, Is.EqualTo(2));
            Assert.That(model.Rows[0].Cells[0].RawValue, Is.EqualTo("1"));
            Assert.That(model.Rows[0].Cells[1].RawValue, Is.EqualTo("Alice"));
            Assert.That(model.Rows[0].Cells[2].RawValue, Is.EqualTo("3.14"));
            Assert.That(model.Rows[1].Cells[0].RawValue, Is.EqualTo("2"));
            Assert.That(model.Rows[1].Cells[1].RawValue, Is.EqualTo("Bob"));
            Assert.That(model.Rows[1].Cells[2].RawValue, Is.EqualTo("2.71"));
        }

        [Test]
        public void Parse_EmptyFile_ReturnsEmptyModel()
        {
            // Arrange: 空 workbook（只有 headers 没有数据行）
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Empty");
                ws.Cell(1, 1).Value = "ColA";
                ws.Cell(1, 2).Value = "ColB";
                ws.Cell(2, 1).Value = "STRING";
                ws.Cell(2, 2).Value = "INT";
                // 无数据行
            });

            // Act
            var model = _parser.Parse(_tempFilePath);

            // Assert
            Assert.That(model.Columns.Length, Is.EqualTo(2));
            Assert.That(model.Rows.Length, Is.EqualTo(0));
        }

        [Test]
        public void Parse_FileWithAssetRefColumn_IsAssetRefTrue()
        {
            // Arrange: 包含 AssetRef 列
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Assets");
                ws.Cell(1, 1).Value = "Icon";
                ws.Cell(1, 2).Value = "Name";
                ws.Cell(2, 1).Value = "AssetRef(Sprite)";
                ws.Cell(2, 2).Value = "STRING";
                ws.Cell(3, 1).Value = "icon_hero";
                ws.Cell(3, 2).Value = "Hero";
            });

            // Act
            var model = _parser.Parse(_tempFilePath);

            // Assert
            Assert.That(model.Columns.Length, Is.EqualTo(2));
            Assert.That(model.Columns[0].IsAssetRef, Is.True);
            Assert.That(model.Columns[0].Type, Is.EqualTo("AssetRef(Sprite)"));
            Assert.That(model.Columns[1].IsAssetRef, Is.False);
        }

        [Test]
        public void Parse_FileWithEmptyDataRows_SkipsEmptyRows()
        {
            // Arrange: 中间有空行
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Data");
                ws.Cell(1, 1).Value = "ID";
                ws.Cell(2, 1).Value = "INT";
                ws.Cell(3, 1).Value = "1";
                // Row 4 is empty (should be skipped)
                // But since we only have 1 column, an empty cell means empty row
                // This test verifies the parser stops at empty rows
            });

            // Act
            var model = _parser.Parse(_tempFilePath);

            // Assert
            Assert.That(model.Rows.Length, Is.EqualTo(1)); // only row 3, row 4 is empty and stops
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
    public class ExcelParserTests
    {
        [Test]
        public void Placeholder_ClosedXML_NotAvailable()
        {
            Assert.Inconclusive("ClosedXML.dll 未加载。请在 Unity Plugin Inspector 中关闭其 Validate References 后将 #if false 改为 #if true。");
        }
    }
}
#endif
