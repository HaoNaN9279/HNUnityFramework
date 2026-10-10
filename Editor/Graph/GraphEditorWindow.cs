using System;
using HN.Framework.Core.Level.Logic.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 图编辑器窗口基类。提供工具栏、加载/保存（.asset）、JSON 导入导出与校验等通用能力。
    /// 领域编辑器继承后设置显示名与分类过滤即可。
    /// </summary>
    public class GraphEditorWindow : EditorWindow
    {
        private GraphData _graph;
        private GraphEditorView _view;
        private Label _statusLabel;
        private string _currentAssetPath;

        /// <summary>窗口标题 / 默认图名称。</summary>
        protected virtual string GraphDisplayName => "Graph";

        /// <summary>领域分类过滤（节点类型路径首段）。为空时展示全部节点类型。</summary>
        protected virtual string CategoryFilter => null;

        /// <summary>
        /// 创建默认空图。
        /// </summary>
        /// <returns>新建的图数据。</returns>
        protected virtual GraphData CreateDefaultGraph()
        {
            return new GraphData(Guid.NewGuid().ToString("N"), GraphDisplayName);
        }

        /// <summary>
        /// 打开指定类型的图编辑器窗口。
        /// </summary>
        /// <typeparam name="T">窗口类型。</typeparam>
        /// <param name="title">窗口标题。</param>
        /// <returns>打开的窗口。</returns>
        protected static T Open<T>(string title) where T : GraphEditorWindow
        {
            var window = GetWindow<T>(title);
            window.Show();
            return window;
        }

        private void CreateGUI()
        {
            if (_graph == null)
            {
                _graph = CreateDefaultGraph();
            }

            BuildToolbar();

            _view = new GraphEditorView(_graph)
            {
                CategoryFilter = CategoryFilter
            };
            rootVisualElement.Add(_view);

            _statusLabel = new Label("Ready.");
            _statusLabel.style.marginLeft = 4f;
            _statusLabel.style.marginBottom = 2f;
            rootVisualElement.Add(_statusLabel);
        }

        private void BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.flexWrap = Wrap.Wrap;
            toolbar.style.paddingLeft = 4f;
            toolbar.style.paddingTop = 2f;

            toolbar.Add(MakeButton("New", OnNew));
            toolbar.Add(MakeButton("Open", OnOpen));
            toolbar.Add(MakeButton("Save", OnSave));
            toolbar.Add(MakeButton("Save As", OnSaveAs));
            toolbar.Add(MakeButton("Export JSON", OnExportJson));
            toolbar.Add(MakeButton("Import JSON", OnImportJson));
            toolbar.Add(MakeButton("Validate", OnValidate));
            toolbar.Add(MakeButton("Frame All", OnFrameAll));

            rootVisualElement.Add(toolbar);
        }

        private static Button MakeButton(string text, Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginRight = 4f;
            button.style.marginTop = 2f;
            button.style.marginBottom = 2f;
            return button;
        }

        private void OnNew()
        {
            _graph = CreateDefaultGraph();
            _currentAssetPath = null;
            _view.LoadGraph(_graph);
            SetStatus("New graph created.");
        }

        private void OnOpen()
        {
            var path = EditorUtility.OpenFilePanel("Open Graph", Application.dataPath, "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var projectPath = ToProjectPath(path);
            if (projectPath == null)
            {
                SetStatus("Select an asset inside the project.");
                return;
            }

            var asset = GraphSerializationUtility.LoadAsset(projectPath);
            if (asset == null)
            {
                SetStatus("Not a graph asset.");
                return;
            }

            _graph = asset.Load();
            _currentAssetPath = projectPath;
            _view.LoadGraph(_graph);
            SetStatus("Opened " + projectPath);
        }

        private void OnSave()
        {
            if (string.IsNullOrEmpty(_currentAssetPath))
            {
                OnSaveAs();
                return;
            }

            _view.SyncToData();
            GraphSerializationUtility.SaveAsset(_graph, _currentAssetPath);
            SetStatus("Saved " + _currentAssetPath);
        }

        private void OnSaveAs()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Graph", GraphDisplayName, "asset", "Save graph asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _view.SyncToData();
            GraphSerializationUtility.SaveAsset(_graph, path);
            _currentAssetPath = path;
            SetStatus("Saved " + path);
        }

        private void OnExportJson()
        {
            _view.SyncToData();
            var path = EditorUtility.SaveFilePanel("Export JSON", Application.dataPath, GraphDisplayName, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            GraphSerializationUtility.ExportJson(_graph, path);
            SetStatus("Exported " + path);
        }

        private void OnImportJson()
        {
            var path = EditorUtility.OpenFilePanel("Import JSON", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var graph = GraphSerializationUtility.ImportJson(path);
            if (graph == null)
            {
                SetStatus("Invalid JSON.");
                return;
            }

            _graph = graph;
            _currentAssetPath = null;
            _view.LoadGraph(_graph);
            SetStatus("Imported " + path);
        }

        private void OnValidate()
        {
            _view.SyncToData();
            if (GraphSerializationUtility.Validate(_graph, out var error))
            {
                SetStatus("Validation passed.");
            }
            else
            {
                SetStatus("Validation failed: " + error);
                Debug.LogWarning("[GraphEditor] " + error);
            }
        }

        private void OnFrameAll()
        {
            _view.FrameAll();
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
        }

        private static string ToProjectPath(string absolutePath)
        {
            var dataPath = Application.dataPath;
            if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(dataPath))
            {
                return null;
            }

            var normalized = absolutePath.Replace('\\', '/');
            var root = dataPath.Replace('\\', '/');
            if (!normalized.StartsWith(root, StringComparison.Ordinal))
            {
                return null;
            }

            return "Assets" + normalized.Substring(root.Length);
        }
    }
}
