#nullable enable

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// E7 配置表编辑器窗口。
    /// 提供打开、查看、编辑、保存 Excel (.xlsx) 配置表的功能。
    /// </summary>
    public class SheetEditorWindow : EditorWindow
    {
        private SheetGrid _sheetGrid = null!;
        private Label _statusLabel = null!;
        private VisualElement? _lubanHelpBox;
        private VisualElement? _lubanConfigRow;
        private string? _currentFilePath;
        private TableModel? _currentModel;

        private ExcelSourceParser _parser = null!;
        private ExcelSerializer _serializer = null!;

        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Sheet/Sheet Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<SheetEditorWindow>("E7 Sheet Editor");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        private void CreateGUI()
        {
            _parser = new ExcelSourceParser();
            _serializer = new ExcelSerializer();

            // 根容器
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            // 帮助提示：Luban 未配置时显示
            _lubanHelpBox = CreateLubanHelpBox();
            root.Add(_lubanHelpBox);

            // 工具栏
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 32,
                    minHeight = 32,
                    backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f),
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4,
                    alignItems = Align.Center,
                },
            };

            var btnOpen = new Button(() => OnOpen())
            {
                text = "打开",
                style = { marginRight = 4, height = 24 },
            };
            toolbar.Add(btnOpen);

            var btnSave = new Button(() => OnSave())
            {
                text = "保存",
                style = { marginRight = 4, height = 24 },
            };
            toolbar.Add(btnSave);

            var btnRefresh = new Button(() => OnRefresh())
            {
                text = "刷新",
                style = { marginRight = 4, height = 24 },
            };
            toolbar.Add(btnRefresh);

            var btnAddRow = new Button(() => OnAddRow())
            {
                text = "添加行",
                style = { marginRight = 4, height = 24 },
            };
            toolbar.Add(btnAddRow);

            // Luban CLI 路径配置
            _lubanConfigRow = CreateLubanConfigRow();
            toolbar.Add(_lubanConfigRow);

            // 状态栏
            _statusLabel = new Label("就绪")
            {
                style =
                {
                    marginLeft = StyleKeyword.Auto,
                    fontSize = 11,
                    color = new Color(0.6f, 0.6f, 0.6f, 1f),
                    unityTextAlign = TextAnchor.MiddleRight,
                },
            };
            toolbar.Add(_statusLabel);

            root.Add(toolbar);

            // 网格视图
            _sheetGrid = new SheetGrid();
            root.Add(_sheetGrid);
        }

        /// <summary>打开 Excel 文件。</summary>
        private void OnOpen()
        {
            var path = EditorUtility.OpenFilePanel("打开配置表", "Assets/", "xlsx");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                _currentFilePath = path;
                _currentModel = _parser.Parse(path);
                _sheetGrid.SetModel(_currentModel);
                _statusLabel.text = $"已打开: {Path.GetFileName(path)}";
                SetStatus("打开成功", StatusType.Info);
            }
            catch (System.Exception ex)
            {
                SetStatus($"打开失败: {ex.Message}", StatusType.Error);
            }
        }

        /// <summary>保存到 Excel 文件。</summary>
        private void OnSave()
        {
            if (_currentModel == null)
            {
                SetStatus("无数据可保存", StatusType.Warning);
                return;
            }

            // 从 UI 收集最新数据
            _currentModel = _sheetGrid.GetModel();

            try
            {
                var savePath = _currentFilePath;
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = EditorUtility.SaveFilePanel("保存配置表", "Assets/", _currentModel.TableName, "xlsx");
                    if (string.IsNullOrEmpty(savePath))
                        return;
                    _currentFilePath = savePath;
                }

                _serializer.Serialize(savePath, _currentModel);
                _currentModel.SourcePath = savePath;
                AssetDatabase.Refresh();
                SetStatus($"已保存: {Path.GetFileName(savePath)}", StatusType.Info);
            }
            catch (System.Exception ex)
            {
                SetStatus($"保存失败: {ex.Message}", StatusType.Error);
            }
        }

        /// <summary>重新加载当前文件。</summary>
        private void OnRefresh()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SetStatus("无已打开的文件", StatusType.Warning);
                return;
            }

            try
            {
                _currentModel = _parser.Parse(_currentFilePath);
                _sheetGrid.SetModel(_currentModel);
                SetStatus("刷新成功", StatusType.Info);
            }
            catch (System.Exception ex)
            {
                SetStatus($"刷新失败: {ex.Message}", StatusType.Error);
            }
        }

        /// <summary>添加新数据行。</summary>
        private void OnAddRow()
        {
            if (_currentModel == null)
            {
                SetStatus("请先打开一个配置表", StatusType.Warning);
                return;
            }

            int colCount = _currentModel.Columns.Length;
            var newCells = new CellData[colCount];
            for (int i = 0; i < colCount; i++)
            {
                newCells[i] = new CellData();
            }

            var newRow = new RowData(newCells);
            var updatedRows = new RowData[_currentModel.Rows.Length + 1];
            _currentModel.Rows.CopyTo(updatedRows, 0);
            updatedRows[updatedRows.Length - 1] = newRow;
            _currentModel.Rows = updatedRows;

            _sheetGrid.Refresh();
            SetStatus($"已添加第 {updatedRows.Length} 行", StatusType.Info);
        }

        private void SetStatus(string message, StatusType type)
        {
            _statusLabel.text = message;
            switch (type)
            {
                case StatusType.Error:
                    _statusLabel.style.color = new Color(1f, 0.4f, 0.4f, 1f);
                    break;
                case StatusType.Warning:
                    _statusLabel.style.color = new Color(1f, 0.8f, 0.3f, 1f);
                    break;
                default:
                    _statusLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    break;
            }
        }

        /// <summary>创建 Luban 未配置时的帮助提示栏。</summary>
        private VisualElement CreateLubanHelpBox()
        {
            var box = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    backgroundColor = new Color(0.25f, 0.22f, 0.1f, 1f),
                    borderBottomColor = new Color(0.5f, 0.4f, 0.1f, 1f),
                    borderBottomWidth = 1f,
                    paddingLeft = 12,
                    paddingRight = 12,
                    paddingTop = 6,
                    paddingBottom = 6,
                    alignItems = Align.Center,
                    display = LubanPathConfig.IsValid() ? DisplayStyle.None : DisplayStyle.Flex,
                },
            };

            var icon = new Label("\u2139") // ℹ
            {
                style =
                {
                    fontSize = 14,
                    color = new Color(0.9f, 0.7f, 0.2f, 1f),
                    marginRight = 8,
                },
            };
            box.Add(icon);

            var text = new Label("Luban CLI 未配置。配置表编辑器的基础功能（打开/编辑/保存 Excel）不受影响。"
                + "如需使用 Luban 代码生成功能，请部署 Luban 后在下方工具栏中设置其路径。"
                + "参见: https://www.datable.cn/")
            {
                style =
                {
                    fontSize = 11,
                    color = new Color(0.8f, 0.7f, 0.4f, 1f),
                    whiteSpace = WhiteSpace.Normal,
                },
            };
            box.Add(text);

            return box;
        }

        /// <summary>创建 Luban 路径配置行（工具栏内）。</summary>
        private VisualElement CreateLubanConfigRow()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = StyleKeyword.Auto,
                },
            };

            var lbl = new Label("Luban CLI:")
            {
                style =
                {
                    fontSize = 11,
                    color = new Color(0.6f, 0.6f, 0.6f, 1f),
                    marginRight = 4,
                },
            };
            row.Add(lbl);

            var pathField = new TextField
            {
                value = LubanPathConfig.GetPath(),
                style =
                {
                    width = 200,
                    height = 20,
                    marginRight = 4,
                    fontSize = 11,
                },
            };
            pathField.RegisterValueChangedCallback(evt => OnLubanPathChanged(evt.newValue));
            row.Add(pathField);

            var btnBrowse = new Button(() => OnBrowseLubanPath(pathField))
            {
                text = "...",
                style =
                {
                    width = 24,
                    height = 20,
                    marginRight = 4,
                    fontSize = 11,
                },
            };
            row.Add(btnBrowse);

            return row;
        }

        /// <summary>打开文件选择器以浏览 Luban CLI 路径。</summary>
        private void OnBrowseLubanPath(TextField pathField)
        {
            var selected = EditorUtility.OpenFilePanel("选择 Luban CLI", string.Empty, "");
            if (!string.IsNullOrEmpty(selected))
            {
                pathField.value = selected;
                OnLubanPathChanged(selected);
            }
        }

        /// <summary>Luban 路径变更时持久化存储并更新帮助提示栏可见性。</summary>
        private void OnLubanPathChanged(string newPath)
        {
            LubanPathConfig.SetPath(newPath);
            if (_lubanHelpBox != null)
            {
                _lubanHelpBox.style.display = LubanPathConfig.IsValid()
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        private enum StatusType { Info, Warning, Error }
    }
}
