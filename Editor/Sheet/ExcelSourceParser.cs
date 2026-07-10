#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// 使用 ClosedXML 解析 .xlsx 文件为 TableModel。
    /// Row 1 = 字段名, Row 2 = 类型标注, Row 3+ = 数据。
    /// </summary>
    public class ExcelSourceParser
    {
        /// <summary>
        /// 解析 Excel 文件为 TableModel。
        /// </summary>
        /// <param name="excelPath">.xlsx 文件路径</param>
        /// <returns>解析后的 TableModel</returns>
        public TableModel Parse(string excelPath)
        {
            using var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheet(1);

            var tableName = Path.GetFileNameWithoutExtension(excelPath);
            var columns = SchemaProvider.ParseSchemaFromWorksheet(worksheet);
            var rows = ParseDataRows(worksheet, columns.Length);

            return new TableModel(tableName, columns, rows, excelPath);
        }

        /// <summary>
        /// 从第 3 行开始解析数据行，跳过全空行。
        /// </summary>
        /// <param name="worksheet">Excel 工作表</param>
        /// <param name="columnCount">列数</param>
        /// <returns>解析后的行数据数组</returns>
        private static RowData[] ParseDataRows(IXLWorksheet worksheet, int columnCount)
        {
            var rows = new List<RowData>();
            int rowIndex = 3;

            while (true)
            {
                var excelRow = worksheet.Row(rowIndex);
                var cells = new List<CellData>();
                bool hasAnyValue = false;

                for (int col = 1; col <= columnCount; col++)
                {
                    var cell = excelRow.Cell(col);
                    var rawValue = SchemaProvider.GetStringValue(cell);

                    if (!string.IsNullOrEmpty(rawValue))
                        hasAnyValue = true;

                    cells.Add(new CellData(rawValue));
                }

                // 如果整行全空，停止读取
                if (!hasAnyValue)
                    break;

                rows.Add(new RowData(cells.ToArray()));
                rowIndex++;
            }

            return rows.ToArray();
        }
    }
}
