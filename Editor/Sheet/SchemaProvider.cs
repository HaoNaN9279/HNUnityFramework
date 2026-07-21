#nullable enable

using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// 从 Excel 文件中解析列 Schema（字段名 + 类型标注）。
    /// Row 1 = 字段名，Row 2 = 类型标注。
    /// </summary>
    public static class SchemaProvider
    {
        /// <summary>
        /// 解析 Excel 文件的 Schema（前两行）。
        /// </summary>
        /// <param name="excelPath">.xlsx 文件路径</param>
        /// <returns>列定义数组</returns>
        public static ColumnDef[] ParseExcelSchema(string excelPath)
        {
            using var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheet(1);
            return ParseSchemaFromWorksheet(worksheet);
        }

        /// <summary>
        /// 从 Worksheet 中解析 Schema。
        /// </summary>
        /// <param name="worksheet">已打开的 Excel 工作表</param>
        /// <returns>列定义数组</returns>
        public static ColumnDef[] ParseSchemaFromWorksheet(IXLWorksheet worksheet)
        {
            var row1 = worksheet.Row(1);
            var row2 = worksheet.Row(2);
            return ParseSchemaFromRows(row1, row2);
        }

        /// <summary>
        /// 从两个 IXLRow 中解析 Schema。
        /// Row 1 = 字段名，Row 2 = 类型标注。
        /// 遇到第一个空字段名时停止。
        /// </summary>
        /// <param name="row1">字段名行</param>
        /// <param name="row2">类型标注行</param>
        /// <returns>列定义数组</returns>
        public static ColumnDef[] ParseSchemaFromRows(IXLRow row1, IXLRow row2)
        {
            var columns = new List<ColumnDef>();
            int colIndex = 1;

            while (true)
            {
                var headerCell = row1.Cell(colIndex);
                var headerValue = GetStringValue(headerCell);

                // 遇到第一个空字段名时停止
                if (string.IsNullOrEmpty(headerValue))
                    break;

                var typeCell = row2.Cell(colIndex);
                var typeValue = GetStringValue(typeCell);

                columns.Add(new ColumnDef(headerValue.Trim(), typeValue.Trim()));
                colIndex++;
            }

            return columns.ToArray();
        }

        /// <summary>
        /// 安全获取单元格的字符串值（空单元格返回空串）。
        /// </summary>
        internal static string GetStringValue(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return string.Empty;

            // 尝试获取字符串值，如果是数字则转为字符串
            if (cell.DataType == XLDataType.Text)
                return cell.GetString();

            if (cell.DataType == XLDataType.Number)
                return cell.GetDouble().ToString();

            return cell.GetString();
        }
    }
}
