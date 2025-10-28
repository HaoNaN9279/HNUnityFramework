using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;
using UnityEditor.UIElements;

namespace HN.Framework.Editor
{
    public class SheetEditor : EditorWindow
    {
        public static void OpenWindow(Sheet sheet)
        {
            SheetEditor w = GetWindow<SheetEditor>();
            w.objField.value = sheet;
            w.titleContent = new GUIContent($"[SheetEditor]");
        }


        void CreateGUI()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if(styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            root = rootVisualElement;

            objField = new ObjectField("Sheet");
            objField.RegisterValueChangedCallback((e) =>
            {
                Clear();
                if (e.newValue as Sheet != null)
                {
                    Draw(e.newValue as Sheet);
                }
            });

            sheetRoot = new VisualElement();
            sheetRoot.name = "sheetRoot";

            root.Add(objField);
            root.Add(sheetRoot);
        }

        private void RepaintSheet()
        {
            ClearSheet();
            DrawSheet(sheet);
            Repaint();
        }

        private void Clear()
        {
            if (sheetRoot != null)
            {
                sheetRoot.Clear();
                titleContent = new GUIContent($"[SheetEditor]");
                sheet = null;
                ClearSheet();
            }
        }

        private void ClearSheet()
        {
            serializedObject = null;
            typesProperty = null;
            headersProperty = null;
            elementsProperty = null;
            sheetFieldTypeDrawer = null;
            objField = null;
            scrollView = null;
            selectedColumnIdField = null;
            selectedRowIdField = null;
            columnSeperators.Clear();
            columnIdFields.Clear();
            rowIdFields.Clear();
            sheetRoot.Clear();
        }

        private void Draw(Sheet sheet)
        {
            this.sheet = sheet;
            titleContent = new GUIContent($"{sheet.name}[SheetEditor]");
            DrawSheet(sheet);
        }
        
        private void DrawSheet(Sheet sheet)
        {
            serializedObject = new SerializedObject(sheet);
            typesProperty = serializedObject.FindProperty("types");
            headersProperty = serializedObject.FindProperty("headers");
            elementsProperty = serializedObject.FindProperty("elements");
            sheetFieldTypeDrawer = new SheetFieldTypeDrawer(elementsProperty);

            var menuContainer = DrawMenuContainer();
            menuContainer.name = "menuContainer";
            sheetRoot.Add(menuContainer);

            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.name = "scrollView";
            scrollView.contentContainer.name = "scrollViewContentContainer";

            var leftColumn = DrawLeftColumn();
            scrollView.Add(leftColumn);

            for (int i = 0; i < sheet.columnCount; i++)
            {
                // columnTitle需要在scrollView垂直滚动时保持垂直位置不变，所以position被标记为absolute，
                // 所以无法跟随parent flex grow，这里传出来被Seperator手动修改其width。
                var sheetColumn = DrawSheetColumn(i, out VisualElement columnTitle);
                scrollView.Add(sheetColumn);
                var columnSeperator = DrawColumnSeperator(new[] { sheetColumn, columnTitle });
                columnSeperators.Add(columnSeperator);
                scrollView.Add(columnSeperator);
            }

            sheetRoot.Add(scrollView);

            leftColumn.BringToFront();
        }

        private VisualElement DrawMenuContainer()
        {
            var menuContainer = new VisualElement();
            
            var addColumnBeforeButton = new UnityEngine.UIElements.Button(AddColumnBefore);
            addColumnBeforeButton.name = "addColumnBeforeButton";
            addColumnBeforeButton.tooltip = "当前选择列前添加列";
            menuContainer.Add(addColumnBeforeButton);
            var addColumnAfterButton = new UnityEngine.UIElements.Button(AddColumnAfter);
            addColumnAfterButton.name = "addColumnAfterButton";
            addColumnAfterButton.tooltip = "当前选择列后添加列";
            menuContainer.Add(addColumnAfterButton);
            var deleteColumnButton = new UnityEngine.UIElements.Button(DeleteColumn);
            deleteColumnButton.name = "deleteColumnButton";
            deleteColumnButton.tooltip = "删除当前选择列";
            menuContainer.Add(deleteColumnButton);
            
            var addRowBeforeButton = new UnityEngine.UIElements.Button(AddRowBefore);
            addRowBeforeButton.name = "addRowBeforeButton";
            addRowBeforeButton.tooltip = "当前选择行前添加行";
            menuContainer.Add(addRowBeforeButton);
            var addRowAfterButton = new UnityEngine.UIElements.Button(AddRowAfter);
            addRowAfterButton.name = "addRowAfterButton";
            addRowAfterButton.tooltip = "当前选择行后添加行";
            menuContainer.Add(addRowAfterButton);
            var deleteRowButton = new UnityEngine.UIElements.Button(DeleteRow);
            deleteRowButton.name = "deleteRowButton";
            deleteRowButton.tooltip = "删除当前选择行";
            menuContainer.Add(deleteRowButton);
            
            return menuContainer;
        }

        private VisualElement DrawLeftColumn()
        {
            var leftColumn = new VisualElement();
            leftColumn.name = "leftColumn";

            var topLeftCorner = new VisualElement();
            topLeftCorner.name = "topLeftCorner";
            scrollView.verticalScroller.valueChanged += (value) =>
            {
                topLeftCorner.style.top = value;
            };

            var leftDataColumn = new VisualElement();
            leftDataColumn.name = "leftDataColumn";

            leftColumn.Add(topLeftCorner);
            leftColumn.Add(leftDataColumn);

            for (int i = 0; i < sheet.rowCount; i++)
            {
                var dataLeft = DrawDataLeft(i);
                leftDataColumn.Add(dataLeft);
            }

            topLeftCorner.BringToFront();
            return leftColumn;
        }

        private VisualElement DrawSheetColumn(int columnId, out VisualElement columnTitle)
        {
            var sheetColumn = new VisualElement();
            sheetColumn.name = "sheetColumn";
            if (columnId == 0)
                sheetColumn.style.marginLeft = 32;

            columnTitle = DrawColumnTitle(columnId);
            sheetColumn.Add(columnTitle);

            for (int i = 0; i < sheet.rowCount; i++)
            {
                var dataField = DrawDataField(columnId, i);
                sheetColumn.Add(dataField);
            }

            columnTitle.BringToFront();
            return sheetColumn;
        }

        private VisualElement DrawColumnSeperator(VisualElement[] targets)
        {
            var columnSeperator = new ColumnSeperator(targets);
            columnSeperator.name = "columnSeperator";
            return columnSeperator;
        }

        private VisualElement DrawDataLeft(int rowId)
        {
            var rowIdField = new Label();
            rowIdField.name = "rowIdField";
            rowIdField.text = rowId.ToString();
            rowIdField.RegisterCallback<MouseDownEvent>((e) =>
            {
                if(e.button == 0)
                    SelectRow(e.target as Label, rowId);
            });
            scrollView.horizontalScroller.valueChanged += (value) =>
            {
                rowIdField.style.left = value;
            };
            if(rowId == 0)
                rowIdField.style.marginTop = titleHeight;
            if (rowId % 2 == 0)
                rowIdField.AddToClassList("id-row-even");
            else
                rowIdField.AddToClassList("id-row-odd");
            rowIdFields.Add(rowIdField);
            return rowIdField;
        }

        private VisualElement DrawColumnTitle(int columnId)
        {
            var columnTitle = new VisualElement();
            columnTitle.name = "columnTitle";
            scrollView.verticalScroller.valueChanged += (value) =>
            {
                columnTitle.style.top = value;
            };

            var columnIdField = new Label();
            columnIdField.name = "columnIdField";
            columnIdField.text = GetColumnId(columnId);
            columnIdField.RegisterCallback<MouseDownEvent>((e) =>
            {
                if(e.button == 0)
                    SelectColumn(e.target as Label, columnId);
            });
            columnIdFields.Add(columnIdField);
            columnTitle.Add(columnIdField);

            var typeField = DrawTypeField(columnId);
            columnTitle.Add(typeField);

            var headerField = DrawHeaderField(columnId);
            columnTitle.Add(headerField);

            return columnTitle;
        }

        private VisualElement DrawDataField(int columnId, int rowId)
        {
            var dataRoot = new VisualElement();
            dataRoot.name = "dataRoot";
            if(rowId == 0)
                dataRoot.style.marginTop = titleHeight;
            string typeName = sheet.types[columnId];
            int elementId = rowId * sheet.columnCount + columnId;
            string value = sheet.elements[elementId];
            var dataField = sheetFieldTypeDrawer.DrawField(typeName, elementId, value);
            if (rowId % 2 == 0)
                dataRoot.AddToClassList("data-row-even");
            else
                dataRoot.AddToClassList("data-row-odd");
            dataField.RegisterCallback<ClickEvent>((e) =>
            {
                SelectRow(rowIdFields[rowId], rowId);
                SelectColumn(columnIdFields[columnId], columnId);
            });
            dataRoot.Add(dataField);
            return dataRoot;
        }

        private VisualElement DrawTypeField(int columnId)
        {
            var typeField = new DropdownField(sheetFieldTypeDrawer.TypeNameList, 0);
            typeField.name = "typeField";
            typeField.value = sheet.types[columnId];
            typeField.RegisterValueChangedCallback((e) =>
            {
                typesProperty.GetArrayElementAtIndex(columnId).stringValue = e.newValue.ToString();
                serializedObject.ApplyModifiedProperties();
            });
            return typeField;
        }

        private VisualElement DrawHeaderField(int columnId)
        {
            var headerField = new TextField();
            headerField.name = "headerField";
            headerField.value = sheet.headers[columnId];
            headerField.RegisterValueChangedCallback((e) =>
            {
                headersProperty.GetArrayElementAtIndex(columnId).stringValue = e.newValue.ToString();
                serializedObject.ApplyModifiedProperties();
            });
            return headerField;
        }

        private string GetColumnId(int columnId)
        {
            string[] letters = new string[] {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L","M",
                                             "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
            int x = columnId;
            string id = "";
            do
            {
                int w = x % 26;
                id = id.Insert(0, letters[w]);
                x = x / 26;
            } while (x != 0);
            return id;
        }

        private void ChangeBorderColor(VisualElement target, Color color)
        {
            if (target == null)
                return;

            target.style.borderLeftColor = new StyleColor(color);
            target.style.borderTopColor = new StyleColor(color);
            target.style.borderRightColor = new StyleColor(color);
            target.style.borderBottomColor = new StyleColor(color);
        }

        private void SelectRow(Label target, int rowId)
        {
            if (target != selectedRowIdField)
            {
                ChangeBorderColor(selectedRowIdField, unselectedColor);
                selectedRowIdField = target;
                selectedRowId = rowId;
            }
            ChangeBorderColor(selectedRowIdField, selectedColor);
        }

        private void SelectColumn(Label target, int columnId)
        {
            if (target != selectedColumnIdField)
            {
                ChangeBorderColor(selectedColumnIdField, unselectedColor);
                selectedColumnIdField = target;
                selectedColumnId = columnId;
            }
            ChangeBorderColor(selectedColumnIdField, selectedColor);
        }

        private void AddColumnBefore()
        {
            sheet.AddColumn(selectedColumnId);
            RepaintSheet();
        }

        private void AddColumnAfter()
        {
            sheet.AddColumn(selectedColumnId + 1);
            RepaintSheet();
        }

        private void DeleteColumn()
        {
            if(EditorUtility.DisplayDialog("删除", $"表 {sheet.name} 确认删除列 {GetColumnId(selectedColumnId)} ？", "确认", "取消"))
            {
                sheet.DeleteColumn(selectedColumnId);
                RepaintSheet();
            }
        }

        private void AddRowBefore()
        {
            sheet.AddRow(selectedRowId);
            RepaintSheet();
        }

        private void AddRowAfter()
        {
            sheet.AddRow(selectedRowId + 1);
            RepaintSheet();
        }
        
        private void DeleteRow()
        {
            if(EditorUtility.DisplayDialog("删除", $"表 {sheet.name} 确认删除行 {selectedRowId} ？", "确认", "取消"))
            {
                sheet.DeleteRow(selectedRowId);
                RepaintSheet();
            }
        }


        private VisualElement root;
        private Sheet sheet;
        private SerializedObject serializedObject;
        private SerializedProperty typesProperty;
        private SerializedProperty headersProperty;
        private SerializedProperty elementsProperty;
        private SheetFieldTypeDrawer sheetFieldTypeDrawer;
        private ObjectField objField;
        private VisualElement sheetRoot;
        private ScrollView scrollView;
        private List<VisualElement> columnSeperators = new List<VisualElement>();
        private List<Label> columnIdFields = new List<Label>();
        private List<Label> rowIdFields = new List<Label>();
        private int selectedColumnId;
        private Label selectedColumnIdField;
        private int selectedRowId;
        private Label selectedRowIdField;

        private const int titleHeight = 60;
        private Color selectedColor = new Color(0.188f, 0.365f, 0.604f);
        private Color unselectedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        private const string styleSheetPath = "Assets/HNUnityFramework/Editor/Sheet/Resource/SheetEditor.uss";
    
    

        public class ColumnSeperator : VisualElement
        {
            public ColumnSeperator(VisualElement[] targets)
            {
                targetElements = targets;
                startWidths = new float[targets.Length];

                RegisterCallback<MouseDownEvent>(OnMouseDown);
                RegisterCallback<MouseUpEvent>(OnMouseUp);
                RegisterCallback<MouseMoveEvent>(OnMouseMove);
            }


            private void OnMouseDown(MouseDownEvent evt)
            {
                if (evt.button == 0)
                {
                    isResizing = true;
                    for(int i = 0; i < targetElements.Length; i++)
                    {
                        startWidths[i] = targetElements[i].resolvedStyle.width;
                    }
                    startMousePosition = evt.mousePosition;

                    this.CaptureMouse();
                    evt.StopPropagation();
                }
            }

            private void OnMouseUp(MouseUpEvent evt)
            {
                if (isResizing && evt.button == 0)
                {
                    isResizing = false;
                    this.ReleaseMouse();
                    evt.StopPropagation();
                }
            }
            
            private void OnMouseMove(MouseMoveEvent evt)
            {
                if (!isResizing)
                    return;

                float deltaX = evt.mousePosition.x - startMousePosition.x;
                for(int i = 0; i < targetElements.Length; i++)
                {
                    float newWidth = Mathf.Max(startWidths[i] + deltaX, 50f);
                    targetElements[i].style.width = newWidth;
                }

                evt.StopPropagation();
            }


            private bool isResizing;
            private VisualElement[] targetElements;
            private float[] startWidths;
            private Vector2 startMousePosition;
        }
    }
}
