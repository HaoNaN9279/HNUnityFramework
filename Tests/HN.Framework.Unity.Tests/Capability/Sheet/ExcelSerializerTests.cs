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
    /// <see cref="ExcelSerializer"/> 的单元测试。
    /// </summary>
    [TestFixture]
    public class ExcelSerializerTests
    {
        private string _tempFilePath = string.Empty;
        private ExcelSourceParser _parser = null!;
        private ExcelSerializer _serializer = null!;

        [SetUp]
        public void SetUp()
        {
            _parser = new ExcelSourceParser();
            _serializer = new ExcelSerializer();
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
        public void Serialize_ThenParse_DataPreserved()
        {
            // Arrange: 创建初始数据并序列化
            var columns = new[]
            {
                new ColumnDef("ID", "INT"),
                new ColumnDef("Name", "STRING"),
            };
            var rows = new[]
            {
                new RowData(new[] { new CellData("1"), new CellData("Alice") }),
                new RowData(new[] { new CellData("2"), new CellData("Bob") }),
            };
            var model = new TableModel("Roundtrip", columns, rows, _tempFilePath);

            // Act: 序列化
            _serializer.Serialize(_tempFilePath, model);

            // Assert: 重新解析验证
            var parsed = _parser.Parse(_tempFilePath);
            Assert.That(parsed.Columns.Length, Is.EqualTo(2));
            Assert.That(parsed.Columns[0].Name, Is.EqualTo("ID"));
            Assert.That(parsed.Columns[0].Type, Is.EqualTo("INT"));
            Assert.That(parsed.Columns[1].Name, Is.EqualTo("Name"));
            Assert.That(parsed.Columns[1].Type, Is.EqualTo("STRING"));
            Assert.That(parsed.Rows.Length, Is.EqualTo(2));
            Assert.That(parsed.Rows[0].Cells[0].RawValue, Is.EqualTo("1"));
            Assert.That(parsed.Rows[0].Cells[1].RawValue, Is.EqualTo("Alice"));
            Assert.That(parsed.Rows[1].Cells[0].RawValue, Is.EqualTo("2"));
            Assert.That(parsed.Rows[1].Cells[1].RawValue, Is.EqualTo("Bob"));
        }

        [Test]
        public void ModifyCellValue_Serialize_ThenReParse_ValueChanged()
        {
            // Arrange: 创建初始文件
            CreateTestWorkbook(_tempFilePath, wb =>
            {
                var ws = wb.AddWorksheet("Modify");
                ws.Cell(1, 1).Value = "Count";
                ws.Cell(2, 1).Value = "INT";
                ws.Cell(3, 1).Value = "10";
            });

            // Act: 解析 → 修改 → 序列化
            var model = _parser.Parse(_tempFilePath);
            model.Rows[0].Cells[0].RawValue = "99";
            _serializer.Serialize(_tempFilePath, model);

            // Assert: 重新解析验证值已变更
            var parsed = _parser.Parse(_tempFilePath);
            Assert.That(parsed.Rows.Length, Is.EqualTo(1));
            Assert.That(parsed.Rows[0].Cells[0].RawValue, Is.EqualTo("99"));
        }

        [Test]
        public void Serialize_NewFile_CreatesFile()
        {
            // Arrange
            var columns = new[] { new ColumnDef("Key", "STRING") };
            var rows = new[] { new RowData(new[] { new CellData("hello") }) };
            var model = new TableModel("New", columns, rows, _tempFilePath);

            // Act
            _serializer.Serialize(_tempFilePath, model);

            // Assert
            Assert.That(File.Exists(_tempFilePath), Is.True);
            var parsed = _parser.Parse(_tempFilePath);
            Assert.That(parsed.Rows[0].Cells[0].RawValue, Is.EqualTo("hello"));
        }

        [Test]
        public void Roundtrip_MultipleColumns_AllPreserved()
        {
            // Arrange
            var columns = new[]
            {
                new ColumnDef("A", "INT"),
                new ColumnDef("B", "STRING"),
                new ColumnDef("C", "FLOAT"),
                new ColumnDef("D", "BOOL"),
            };
            var rows = new[]
            {
                new RowData(new[]
                {
                    new CellData("1"), new CellData("test"), new CellData("3.14"), new CellData("true"),
                }),
            };
            var model = new TableModel("Multi", columns, rows, _tempFilePath);

            // Act
            _serializer.Serialize(_tempFilePath, model);
            var parsed = _parser.Parse(_tempFilePath);

            // Assert
            Assert.That(parsed.Columns.Length, Is.EqualTo(4));
            Assert.That(parsed.Rows[0].Cells[0].RawValue, Is.EqualTo("1"));
            Assert.That(parsed.Rows[0].Cells[1].RawValue, Is.EqualTo("test"));
            Assert.That(parsed.Rows[0].Cells[2].RawValue, Is.EqualTo("3.14"));
            Assert.That(parsed.Rows[0].Cells[3].RawValue, Is.EqualTo("true"));
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
    public class ExcelSerializerTests
    {
        [Test]
        public void Placeholder_ClosedXML_NotAvailable()
        {
            Assert.Inconclusive("ClosedXML.dll 未加载。请在 Unity Plugin Inspector 中关闭其 Validate References 后将 #if false 改为 #if true。");
        }
    }
}
#endif
