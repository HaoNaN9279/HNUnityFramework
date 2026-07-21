#nullable enable

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// 资源引用单元格 — 显示 64×64 缩略图，支持拖拽放置、点击定位和右键菜单。
    /// </summary>
    public class AssetRefCell : VisualElement
    {
        private Image _thumbnail;
        private Label _label;
        private CellData _cellData = new();

        private const float ThumbnailSize = 64f;

        /// <summary>关联的单元格数据</summary>
        public CellData CellData => _cellData;

        /// <summary>
        /// 创建资源引用单元格。
        /// </summary>
        /// <param name="cellData">单元格数据</param>
        public AssetRefCell(CellData? cellData = null)
        {
            _cellData = cellData ?? new CellData();

            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.Center;
            style.width = ThumbnailSize + 16;
            style.height = ThumbnailSize + 32;
            style.paddingTop = 4;
            style.paddingBottom = 4;

            // 缩略图
            _thumbnail = new Image
            {
                style =
                {
                    width = ThumbnailSize,
                    height = ThumbnailSize,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                },
                scaleMode = ScaleMode.ScaleToFit,
            };
            Add(_thumbnail);

            // 标签
            _label = new Label(_cellData.DisplayLabel)
            {
                style =
                {
                    fontSize = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    width = ThumbnailSize + 8,
                },
            };
            Add(_label);

            // 交互
            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<ContextClickEvent>(OnContextClick);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);

            RefreshDisplay();
        }

        /// <summary>刷新缩略图和标签显示。</summary>
        public void RefreshDisplay()
        {
            if (_cellData.ResolvedAsset != null)
            {
                var preview = AssetPreview.GetAssetPreview(_cellData.ResolvedAsset);
                if (preview == null)
                    preview = AssetPreview.GetMiniThumbnail(_cellData.ResolvedAsset);

                _thumbnail.image = preview;
                _label.text = _cellData.DisplayLabel;
            }
            else
            {
                _thumbnail.image = null;
                _label.text = "(空)";
            }
        }

        /// <summary>更新单元格数据。</summary>
        public void SetCellData(CellData cellData)
        {
            _cellData = cellData;
            RefreshDisplay();
        }

        private void OnClick(ClickEvent evt)
        {
            if (_cellData.ResolvedAsset != null)
            {
                EditorGUIUtility.PingObject(_cellData.ResolvedAsset);
            }
        }

        private void OnContextClick(ContextClickEvent evt)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("清除"), false, () =>
            {
                _cellData.ResolvedAsset = null;
                _cellData.RawValue = string.Empty;
                _cellData.DisplayLabel = string.Empty;
                RefreshDisplay();
            });

            if (_cellData.ResolvedAsset != null)
            {
                menu.AddItem(new GUIContent("浏览"), false, () =>
                {
                    Selection.activeObject = _cellData.ResolvedAsset;
                    EditorGUIUtility.PingObject(_cellData.ResolvedAsset);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("浏览"));
            }

            menu.ShowAsContext();
            evt.StopPropagation();
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            var draggedObject = DragAndDrop.objectReferences;
            if (draggedObject.Length > 0 && draggedObject[0] != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            var draggedObject = DragAndDrop.objectReferences;
            if (draggedObject.Length > 0 && draggedObject[0] != null)
            {
                var asset = draggedObject[0];
                var assetPath = AssetDatabase.GetAssetPath(asset);

                // 检查 Addressables Label
                if (!HasAddressableLabel(asset))
                {
                    EditorUtility.DisplayDialog(
                        "警告",
                        $"资源 \"{asset.name}\" 未分配 Addressables Label。\n请在 Addressables Group 中为该资源配置 Label。",
                        "确定");
                }

                _cellData.ResolvedAsset = asset;
                _cellData.RawValue = assetPath;
                _cellData.DisplayLabel = asset.name;
                RefreshDisplay();
            }

            DragAndDrop.AcceptDrag();
            evt.StopPropagation();
        }

        /// <summary>
        /// 检查资源是否已分配 Addressables Label。
        /// </summary>
        private static bool HasAddressableLabel(UnityEngine.Object asset)
        {
            try
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(assetPath))
                    return false;

                var guid = AssetDatabase.AssetPathToGUID(assetPath);

                // 使用反射检查 Addressables 设置，避免硬依赖
                var settingsType = System.Type.GetType(
                    "UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
                if (settingsType == null)
                    return false;

                var defaultSettingsMethod = settingsType.GetMethod(
                    "get_DefaultSettings",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (defaultSettingsMethod == null)
                    return false;

                var settings = defaultSettingsMethod.Invoke(null, null);
                if (settings == null)
                    return false;

                var findAssetEntryMethod = settingsType.GetMethod("FindAssetEntry", new[] { typeof(string) });
                if (findAssetEntryMethod == null)
                    return false;

                var entry = findAssetEntryMethod.Invoke(settings, new object[] { guid });
                return entry != null;
            }
            catch
            {
                // Addressables 未安装或反射失败 → 跳过检查
                return true;
            }
        }
    }
}
