using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace HN.Framework.Editor
{
    public class SheetEditor : EditorWindow
    {
        /// <summary>
        /// Opens the Sheet editor window.
        /// </summary>
        /// <param name="sheet">The Sheet asset to edit in the window.</param>
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
            summariesProperty = null;
            elementsProperty = null;
            sheetFieldTypeDrawer = null;
            objField = null;
            scrollView = null;
            logField = null;
            selectedColumnIdField = null;
            selectedRowIdField = null;
            columnSeparators.Clear();
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
            summariesProperty = serializedObject.FindProperty("summaries");
            elementsProperty = serializedObject.FindProperty("elements");
            sheetFieldTypeDrawer = new SheetFieldTypeDrawer(elementsProperty);

            var menuContainer = DrawMenuContainer();
            menuContainer.name = "menuContainer";
            sheetRoot.Add(menuContainer);

            var sheetInfoField = DrawSheetInfoField();
            sheetRoot.Add(sheetInfoField);

            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.name = "scrollView";
            scrollView.contentContainer.name = "scrollViewContentContainer";

            var leftColumn = DrawLeftColumn();
            scrollView.Add(leftColumn);

            for (int i = 0; i < sheet.ColumnCount; i++)
            {
                // columnTitle需要在scrollView垂直滚动时保持垂直位置不变，所以position被标记为absolute，
                // 所以无法跟随parent flex grow，这里传出来被Separator手动修改其width。
                var sheetColumn = DrawSheetColumn(i, out VisualElement columnTitle);
                scrollView.Add(sheetColumn);
                var columnSeparator = DrawColumnSeparator(new[] { sheetColumn, columnTitle });
                columnSeparators.Add(columnSeparator);
                scrollView.Add(columnSeparator);
            }

            sheetRoot.Add(scrollView);

            logField = new Label();
            logField.name = "logField";
            logField.text = "";
            sheetRoot.Add(logField);

            leftColumn.BringToFront();
        }

        private VisualElement DrawMenuContainer()
        {
            var menuContainer = new VisualElement();

            var saveButton = new UnityEngine.UIElements.Button(sheet.SaveAsset);
            saveButton.name = "saveButton";
            saveButton.tooltip = "Save";
            menuContainer.Add(saveButton);

            var exportButton = new UnityEngine.UIElements.Button(sheet.Export);
            exportButton.name = "exportButton";
            exportButton.tooltip = "Export";
            menuContainer.Add(exportButton);

            var buttonSeparator0 = new Label();
            buttonSeparator0.name = "buttonSeparator";
            buttonSeparator0.text = "|";
            menuContainer.Add(buttonSeparator0);

            var addColumnBeforeButton = new UnityEngine.UIElements.Button(AddColumnBefore);
            addColumnBeforeButton.name = "addColumnBeforeButton";
            addColumnBeforeButton.tooltip = "Add column before selected column";
            menuContainer.Add(addColumnBeforeButton);
            var addColumnAfterButton = new UnityEngine.UIElements.Button(AddColumnAfter);
            addColumnAfterButton.name = "addColumnAfterButton";
            addColumnAfterButton.tooltip = "Add column after selected column";
            menuContainer.Add(addColumnAfterButton);
            var deleteColumnButton = new UnityEngine.UIElements.Button(DeleteColumn);
            deleteColumnButton.name = "deleteColumnButton";
            deleteColumnButton.tooltip = "Delete selected column";
            menuContainer.Add(deleteColumnButton);

            var buttonSeparator1 = new Label();
            buttonSeparator1.name = "buttonSeparator";
            buttonSeparator1.text = "|";
            menuContainer.Add(buttonSeparator1);

            var addRowBeforeButton = new UnityEngine.UIElements.Button(AddRowBefore);
            addRowBeforeButton.name = "addRowBeforeButton";
            addRowBeforeButton.tooltip = "Add row before selected row";
            menuContainer.Add(addRowBeforeButton);
            var addRowAfterButton = new UnityEngine.UIElements.Button(AddRowAfter);
            addRowAfterButton.name = "addRowAfterButton";
            addRowAfterButton.tooltip = "Add row after selected row";
            menuContainer.Add(addRowAfterButton);
            var deleteRowButton = new UnityEngine.UIElements.Button(DeleteRow);
            deleteRowButton.name = "deleteRowButton";
            deleteRowButton.tooltip = "Delete selected row";
            menuContainer.Add(deleteRowButton);

            return menuContainer;
        }
        
        private VisualElement DrawSheetInfoField()
        {
            var sheetInfoField = new VisualElement();
            sheetInfoField.name = "sheetInfoField";

            var sheetNameField = new TextField();
            sheetNameField.name = "sheetNameField";
            sheetNameField.value = sheet.SheetName;

            return null;
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

            for (int i = 0; i < sheet.RowCount; i++)
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
                sheetColumn.style.marginLeft = rowIdFieldWidth;

            columnTitle = DrawColumnTitle(columnId);
            sheetColumn.Add(columnTitle);

            for (int i = 0; i < sheet.RowCount; i++)
            {
                var dataField = DrawDataField(columnId, i);
                sheetColumn.Add(dataField);
            }

            columnTitle.BringToFront();
            return sheetColumn;
        }

        private VisualElement DrawColumnSeparator(VisualElement[] targets)
        {
            var columnSeparator = new ColumnSeparator(targets);
            columnSeparator.name = "columnSeparator";
            return columnSeparator;
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

            var summaryField = DrawSummaryField(columnId);
            columnTitle.Add(summaryField);

            return columnTitle;
        }

        private VisualElement DrawDataField(int columnId, int rowId)
        {
            var dataRoot = new VisualElement();
            dataRoot.name = "dataRoot";
            if(rowId == 0)
                dataRoot.style.marginTop = titleHeight;
            string typeName = sheet.Types[columnId];
            int elementId = rowId * sheet.ColumnCount + columnId;
            string value = sheet.Elements[elementId];
            var dataField = sheetFieldTypeDrawer.DrawField(typeName, elementId, value);
            if (rowId % 2 == 0)
                dataRoot.AddToClassList("data-row-even");
            else
                dataRoot.AddToClassList("data-row-odd");
            dataField.RegisterCallback<ClickEvent>((e) =>
            {
                SelectRow(rowIdFields[rowId], rowId);
                SelectColumn(columnIdFields[columnId], columnId);
                logField.text = $"{GetColumnId(columnId)}{rowId}:{value}";
            });
            dataRoot.Add(dataField);
            return dataRoot;
        }

        private VisualElement DrawTypeField(int columnId)
        {
            var typeField = new DropdownField(sheetFieldTypeDrawer.TypeNameList, 0);
            typeField.name = "typeField";
            typeField.value = sheet.Types[columnId];
            typeField.RegisterValueChangedCallback((e) =>
            {
                typesProperty.GetArrayElementAtIndex(columnId).stringValue = e.newValue.ToString();
                if (serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(serializedObject.targetObject);
            });
            return typeField;
        }

        private VisualElement DrawHeaderField(int columnId)
        {
            var headerField = new TextField();
            headerField.name = "headerField";
            headerField.value = sheet.Headers[columnId];
            headerField.RegisterValueChangedCallback((e) =>
            {
                headersProperty.GetArrayElementAtIndex(columnId).stringValue = e.newValue.ToString();
                if (serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(serializedObject.targetObject);
            });
            return headerField;
        }

        private VisualElement DrawSummaryField(int columnId)
        {
            var summaryField = new TextField();
            summaryField.name = "summaryField";
            summaryField.value = sheet.Summaries[columnId];
            summaryField.RegisterValueChangedCallback((e) =>
            {
                summariesProperty.GetArrayElementAtIndex(columnId).stringValue = e.newValue.ToString();
                if (serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(serializedObject.targetObject);
            });
            return summaryField;
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
            if(EditorUtility.DisplayDialog("Delete", $"Confirm sheet {sheet.name} deleting column {GetColumnId(selectedColumnId)} ?", "Confirm", "Cancel"))
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
            if(EditorUtility.DisplayDialog("Delete", $"Confirm sheet {sheet.name} deleting row {selectedRowId} ?", "Confirm", "Cancel"))
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
        private SerializedProperty summariesProperty;
        private SerializedProperty elementsProperty;
        private SheetFieldTypeDrawer sheetFieldTypeDrawer;
        private ObjectField objField;
        private VisualElement sheetRoot;
        private ScrollView scrollView;
        private Label logField;
        private List<VisualElement> columnSeparators = new List<VisualElement>();
        private List<Label> columnIdFields = new List<Label>();
        private List<Label> rowIdFields = new List<Label>();
        private int selectedColumnId;
        private Label selectedColumnIdField;
        private int selectedRowId;
        private Label selectedRowIdField;

        private const int rowIdFieldWidth = 32;
        private const int titleHeight = 80;
        private Color selectedColor = new Color(0.188f, 0.365f, 0.604f);
        private Color unselectedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        private const string styleSheetPath = "Assets/HNUnityFramework/Editor/Sheet/Resource/SheetEditor.uss";
    
    

        public class ColumnSeparator : VisualElement
        {
            public ColumnSeparator(VisualElement[] targets)
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
