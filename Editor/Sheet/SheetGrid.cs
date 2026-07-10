#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// UI Toolkit 网格视图，用于编辑配置表。
    /// 提供冻结表头、列宽拖拽、单元格类型分发和滚动功能。
    /// </summary>
    public class SheetGrid : VisualElement
    {
        private TableModel? _model;
        private ScrollView _scrollView;
        private VisualElement _headerRow;
        private VisualElement _bodyContainer;
        private readonly List<float> _columnWidths = new();
        private float _defaultColumnWidth = 120f;

        // 列宽拖拽状态
        private int _resizingColumn = -1;
        private float _resizeStartX;
        private float _resizeStartWidth;

        /// <summary>当前显示的表模型</summary>
        public TableModel? Model => _model;

        public SheetGrid()
        {
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Column;

            BuildLayout();
        }

        /// <summary>设置要显示的表模型并刷新显示。</summary>
        public void SetModel(TableModel model)
        {
            _model = model;
            InitColumnWidths();
            RebuildGrid();
        }

        /// <summary>刷新当前显示。</summary>
        public void Refresh()
        {
            if (_model != null)
            {
                RebuildGrid();
            }
        }

        /// <summary>获取修改后的模型（从 UI 中收集数据）。</summary>
        public TableModel GetModel()
        {
            if (_model == null)
                return new TableModel();

            var rows = new List<RowData>();
            for (int i = 0; i < _bodyContainer.childCount; i++)
            {
                var rowElement = _bodyContainer[i];
                var cells = CollectRowCells(rowElement);
                rows.Add(new RowData(cells.ToArray()));
            }

            _model.Rows = rows.ToArray();
            return _model;
        }

        private void BuildLayout()
        {
            // 冻结表头
            _headerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 44,
                    minHeight = 44,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f),
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                },
            };
            Add(_headerRow);

            // 滚动区域
            _scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
            {
                style = { flexGrow = 1 },
            };
            Add(_scrollView);

            _bodyContainer = new VisualElement();
            _scrollView.Add(_bodyContainer);
        }

        private void InitColumnWidths()
        {
            if (_model == null) return;

            _columnWidths.Clear();
            for (int i = 0; i < _model.Columns.Length; i++)
            {
                _columnWidths.Add(_defaultColumnWidth);
            }
        }

        private void RebuildGrid()
        {
            if (_model == null) return;

            BuildHeader();
            BuildBody();
        }

        /// <summary>构建冻结表头行（字段名 + 类型标注）。</summary>
        private void BuildHeader()
        {
            _headerRow.Clear();
            if (_model == null) return;

            for (int i = 0; i < _model.Columns.Length; i++)
            {
                var colDef = _model.Columns[i];
                var width = i < _columnWidths.Count ? _columnWidths[i] : _defaultColumnWidth;
                var colIndex = i; // 闭包捕获

                var headerCell = new VisualElement
                {
                    style =
                    {
                        width = width,
                        flexDirection = FlexDirection.Column,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center,
                        paddingLeft = 4,
                        paddingRight = 4,
                        borderRightWidth = 1,
                        borderRightColor = new Color(0.35f, 0.35f, 0.35f, 1f),
                    },
                };

                // 字段名
                var nameLabel = new Label(colDef.Name)
                {
                    style =
                    {
                        fontSize = 11,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        overflow = Overflow.Hidden,
                    },
                };
                headerCell.Add(nameLabel);

                // 类型标注
                var typeLabel = new Label(colDef.Type)
                {
                    style =
                    {
                        fontSize = 9,
                        color = new Color(0.6f, 0.6f, 0.7f, 1f),
                        unityTextAlign = TextAnchor.MiddleCenter,
                        overflow = Overflow.Hidden,
                    },
                };
                headerCell.Add(typeLabel);

                _headerRow.Add(headerCell);

                // 列宽拖拽手柄
                if (i < _model.Columns.Length - 1)
                {
                    var resizeHandle = new VisualElement
                    {
                        style =
                        {
                            width = 4,
                            position = Position.Absolute,
                            right = 0,
                            top = 0,
                            bottom = 0,
                            backgroundColor = new Color(0, 0, 0, 0),
                            cursor = new StyleCursor(StyleKeyword.Auto),
                        },
                    };

                    // 使用 IMGUI 事件处理拖拽（更可靠）
                    IMGUIContainer imgui = null;
                    imgui = new IMGUIContainer(() =>
                    {
                        var evtRect = imgui.worldBound;
                        EditorGUIUtility.AddCursorRect(
                            new Rect(evtRect.xMax - 8, evtRect.y, 8, evtRect.height),
                            MouseCursor.ResizeHorizontal);

                        var resizeRect = new Rect(evtRect.xMax - 6, evtRect.y, 6, evtRect.height);
                        var currentEvent = Event.current;

                        if (currentEvent.type == EventType.MouseDown && resizeRect.Contains(currentEvent.mousePosition))
                        {
                            GUIUtility.hotControl = imgui.GetHashCode();
                            _resizingColumn = colIndex;
                            _resizeStartX = currentEvent.mousePosition.x;
                            _resizeStartWidth = _columnWidths[colIndex];
                            currentEvent.Use();
                        }

                        if (_resizingColumn == colIndex &&
                            currentEvent.type == EventType.MouseDrag &&
                            GUIUtility.hotControl == imgui.GetHashCode())
                        {
                            float delta = currentEvent.mousePosition.x - _resizeStartX;
                            float newWidth = Mathf.Max(40f, _resizeStartWidth + delta);
                            _columnWidths[colIndex] = newWidth;
                            RebuildGrid();
                            currentEvent.Use();
                        }

                        if (currentEvent.type == EventType.MouseUp && _resizingColumn == colIndex)
                        {
                            GUIUtility.hotControl = 0;
                            _resizingColumn = -1;
                            currentEvent.Use();
                        }
                    })
                    {
                        style =
                        {
                            position = Position.Absolute,
                            right = 0,
                            top = 0,
                            bottom = 0,
                            width = 12,
                            backgroundColor = new Color(0, 0, 0, 0),
                        },
                    };
                    headerCell.style.position = Position.Relative;
                    headerCell.Add(imgui);
                }
            }
        }

        /// <summary>构建数据行。</summary>
        private void BuildBody()
        {
            _bodyContainer.Clear();
            if (_model == null) return;

            for (int rowIdx = 0; rowIdx < _model.Rows.Length; rowIdx++)
            {
                var rowData = _model.Rows[rowIdx];
                var rowElement = BuildRow(rowIdx, rowData);
                _bodyContainer.Add(rowElement);
            }
        }

        private VisualElement BuildRow(int rowIndex, RowData rowData)
        {
            var rowElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    minHeight = 28,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 1f),
                },
                userData = rowIndex,
            };

            // 行号
            var rowNum = new Label((rowIndex + 1).ToString())
            {
                style =
                {
                    width = 36,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    color = new Color(0.5f, 0.5f, 0.5f, 1f),
                    justifyContent = Justify.Center,
                },
            };
            rowElement.Add(rowNum);

            if (_model == null) return rowElement;

            for (int colIdx = 0; colIdx < _model.Columns.Length; colIdx++)
            {
                var colDef = _model.Columns[colIdx];
                var cellData = colIdx < rowData.Cells.Length
                    ? rowData.Cells[colIdx]
                    : new CellData();

                var width = colIdx < _columnWidths.Count ? _columnWidths[colIdx] : _defaultColumnWidth;
                var cellElement = CreateCellElement(cellData, colDef, rowIndex, colIdx, width);
                rowElement.Add(cellElement);
            }

            return rowElement;
        }

        /// <summary>根据列类型分发创建不同控件。</summary>
        private VisualElement CreateCellElement(
            CellData cellData, ColumnDef colDef, int rowIndex, int colIndex, float width)
        {
            var cellContainer = new VisualElement
            {
                style =
                {
                    width = width,
                    paddingLeft = 2,
                    paddingRight = 2,
                    justifyContent = Justify.Center,
                    borderRightWidth = 1,
                    borderRightColor = new Color(0.15f, 0.15f, 0.15f, 1f),
                },
            };

            if (colDef.IsAssetRef)
            {
                var assetCell = new AssetRefCell(cellData);
                cellContainer.Add(assetCell);
            }
            else if (colDef.Type.Equals("BOOL", StringComparison.OrdinalIgnoreCase))
            {
                var toggle = new Toggle
                {
                    value = cellData.RawValue.Equals("true", StringComparison.OrdinalIgnoreCase)
                            || cellData.RawValue == "1",
                };
                toggle.RegisterValueChangedCallback(evt =>
                {
                    cellData.RawValue = evt.newValue ? "true" : "false";
                });
                cellContainer.Add(toggle);
            }
            else if (colDef.Type.Equals("INT", StringComparison.OrdinalIgnoreCase))
            {
                var intField = new IntegerField
                {
                    value = ParseInt(cellData.RawValue),
                    style = { flexGrow = 1 },
                };
                intField.RegisterValueChangedCallback(evt =>
                {
                    cellData.RawValue = evt.newValue.ToString();
                });
                cellContainer.Add(intField);
            }
            else if (colDef.Type.Equals("FLOAT", StringComparison.OrdinalIgnoreCase))
            {
                var floatField = new FloatField
                {
                    value = ParseFloat(cellData.RawValue),
                    style = { flexGrow = 1 },
                };
                floatField.RegisterValueChangedCallback(evt =>
                {
                    cellData.RawValue = evt.newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                });
                cellContainer.Add(floatField);
            }
            else
            {
                // 默认 STRING
                var textField = new TextField
                {
                    value = cellData.RawValue,
                    style = { flexGrow = 1 },
                    isDelayed = true,
                };
                textField.RegisterValueChangedCallback(evt =>
                {
                    cellData.RawValue = evt.newValue;
                });
                cellContainer.Add(textField);
            }

            return cellContainer;
        }

        private List<CellData> CollectRowCells(VisualElement rowElement)
        {
            var cells = new List<CellData>();
            if (_model == null) return cells;

            // 跳过行号元素 (child 0)
            int childStart = 1;
            for (int i = childStart; i < rowElement.childCount; i++)
            {
                var child = rowElement[i];
                var cellData = ExtractCellData(child);
                cells.Add(cellData);
            }

            return cells;
        }

        private CellData ExtractCellData(VisualElement cellContainer)
        {
            if (cellContainer.childCount == 0)
                return new CellData();

            var first = cellContainer[0];

            switch (first)
            {
                case AssetRefCell assetCell:
                    return assetCell.CellData;

                case Toggle toggle:
                    return new CellData(toggle.value ? "true" : "false");

                case IntegerField intField:
                    return new CellData(intField.value.ToString());

                case FloatField floatField:
                    return new CellData(floatField.value.ToString(System.Globalization.CultureInfo.InvariantCulture));

                case TextField textField:
                    return new CellData(textField.value);

                default:
                    return new CellData();
            }
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static float ParseFloat(string value)
        {
            return float.TryParse(value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0f;
        }
    }
}
