#nullable enable

using System;
using UnityEngine;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// 配置表模型，包含表名、列定义和行数据。
    /// </summary>
    public class TableModel
    {
        /// <summary>表名</summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>列定义数组</summary>
        public ColumnDef[] Columns { get; set; } = Array.Empty<ColumnDef>();

        /// <summary>行数据数组</summary>
        public RowData[] Rows { get; set; } = Array.Empty<RowData>();

        /// <summary>Excel 文件路径</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// 创建一个空表模型。
        /// </summary>
        public TableModel()
        {
        }

        /// <summary>
        /// 创建带数据的表模型。
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">列定义</param>
        /// <param name="rows">行数据</param>
        /// <param name="sourcePath">Excel 文件路径</param>
        public TableModel(string tableName, ColumnDef[] columns, RowData[] rows, string sourcePath)
        {
            TableName = tableName;
            Columns = columns;
            Rows = rows;
            SourcePath = sourcePath;
        }
    }

    /// <summary>
    /// 列定义，包含字段名、类型、注释和是否为资源引用列。
    /// </summary>
    public class ColumnDef
    {
        /// <summary>字段名称（来自 Excel 第 1 行）</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>类型标注（来自 Excel 第 2 行，如 INT, STRING, FLOAT, AssetRef(Sprite)）</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>字段注释</summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>是否为资源引用列（Type 以 "AssetRef" 开头）</summary>
        public bool IsAssetRef => Type.StartsWith("AssetRef", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 创建列定义。
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="type">类型标注</param>
        /// <param name="comment">注释</param>
        public ColumnDef(string name, string type, string comment = "")
        {
            Name = name;
            Type = type;
            Comment = comment;
        }
    }

    /// <summary>
    /// 行数据，包含单元格数组。
    /// </summary>
    public class RowData
    {
        /// <summary>单元格数据数组</summary>
        public CellData[] Cells { get; set; } = Array.Empty<CellData>();

        /// <summary>
        /// 创建行数据。
        /// </summary>
        public RowData()
        {
        }

        /// <summary>
        /// 创建带数据的行。
        /// </summary>
        /// <param name="cells">单元格数据</param>
        public RowData(CellData[] cells)
        {
            Cells = cells;
        }
    }

    /// <summary>
    /// 单元格数据，包含原始值、解析后的资源引用和显示标签。
    /// </summary>
    public class CellData
    {
        /// <summary>单元格原始值（字符串）</summary>
        public string RawValue { get; set; } = string.Empty;

        /// <summary>解析后的 Unity 资源引用</summary>
        public UnityEngine.Object? ResolvedAsset { get; set; }

        /// <summary>显示标签</summary>
        public string DisplayLabel { get; set; } = string.Empty;

        /// <summary>
        /// 创建单元格数据。
        /// </summary>
        public CellData()
        {
        }

        /// <summary>
        /// 创建带值的单元格数据。
        /// </summary>
        /// <param name="rawValue">原始值</param>
        /// <param name="displayLabel">显示标签</param>
        public CellData(string rawValue, string? displayLabel = null)
        {
            RawValue = rawValue;
            DisplayLabel = displayLabel ?? rawValue;
        }
    }
}
