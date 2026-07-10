#nullable enable

using System;
using System.IO;
using ClosedXML.Excel;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// 将 TableModel 序列化回 .xlsx 文件。
    /// 保留现有单元格，仅更新变化的单元格，新行追加到末尾。
    /// </summary>
    public class ExcelSerializer
    {
        /// <summary>
        /// 将 TableModel 写入 Excel 文件。
        /// </summary>
        /// <param name="excelPath">目标 .xlsx 文件路径</param>
        /// <param name="model">要写入的 TableModel</param>
        public void Serialize(string excelPath, TableModel model)
        {
            XLWorkbook workbook;
            IXLWorksheet worksheet;

            bool fileExists = File.Exists(excelPath);
            if (fileExists)
            {
                workbook = new XLWorkbook(excelPath);
                worksheet = workbook.Worksheet(1);
            }
            else
            {
                workbook = new XLWorkbook();
                worksheet = workbook.AddWorksheet(model.TableName);
            }

            try
            {
                WriteSchemaRows(worksheet, model.Columns);
                WriteDataRows(worksheet, model.Columns.Length, model.Rows);
                workbook.SaveAs(excelPath);
            }
            finally
            {
                workbook.Dispose();
            }
        }

        /// <summary>
        /// 写入 Schema 行（第 1 行字段名，第 2 行类型标注）。
        /// </summary>
        private static void WriteSchemaRows(IXLWorksheet worksheet, ColumnDef[] columns)
        {
            for (int col = 0; col < columns.Length; col++)
            {
                var colIndex = col + 1;
                worksheet.Cell(1, colIndex).Value = columns[col].Name;
                worksheet.Cell(2, colIndex).Value = columns[col].Type;
            }
        }

        /// <summary>
        /// 从第 3 行开始写入数据行，先清除旧数据再写入新数据。
        /// </summary>
        private static void WriteDataRows(IXLWorksheet worksheet, int columnCount, RowData[] rows)
        {
            // 清除第 3 行及以后的所有数据
            ClearExistingDataRows(worksheet, columnCount);

            // 写入新数据
            for (int rowIdx = 0; rowIdx < rows.Length; rowIdx++)
            {
                int excelRow = rowIdx + 3;
                var rowData = rows[rowIdx];

                for (int colIdx = 0; colIdx < Math.Min(columnCount, rowData.Cells.Length); colIdx++)
                {
                    var cellData = rowData.Cells[colIdx];
                    if (!string.IsNullOrEmpty(cellData.RawValue))
                    {
                        worksheet.Cell(excelRow, colIdx + 1).Value = cellData.RawValue;
                    }
                }
            }
        }

        /// <summary>
        /// 清除已有数据行（第 3 行及之后）。
        /// </summary>
        private static void ClearExistingDataRows(IXLWorksheet worksheet, int columnCount)
        {
            int lastUsedRow = worksheet.LastRowUsed()?.RowNumber() ?? 2;
            if (lastUsedRow >= 3)
            {
                for (int row = 3; row <= lastUsedRow; row++)
                {
                    for (int col = 1; col <= columnCount; col++)
                    {
                        worksheet.Cell(row, col).Clear();
                    }
                }
            }
        }
    }
}
