using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;
using UnityEditor.UIElements;
using System.Linq;

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
                ClearSheet();
                if (e.newValue as Sheet != null)
                {
                    DrawSheet(e.newValue as Sheet);
                }
            });

            sheetRoot = new VisualElement();
            sheetRoot.name = "sheetRoot";

            root.Add(objField);
            root.Add(sheetRoot);
        }

        private void ClearSheet()
        {
            if(sheetRoot != null)
            {
                sheetRoot.Clear();
                titleContent = new GUIContent($"[SheetEditor]");
                columnSeperators.Clear();
            }
        }

        private void DrawSheet(Sheet sheet)
        {
            this.sheet = sheet;
            titleContent = new GUIContent($"{sheet.name}[SheetEditor]");
            serializedObject = new SerializedObject(sheet);
            sheetFieldTypeDrawer = new SheetFieldTypeDrawer();

            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.name = "scrollView";
            scrollView.contentContainer.name = "scrollViewContentContainer";

            var leftColumn = DrawLeftColumn();
            scrollView.Add(leftColumn);

            for (int i = 0; i < sheet.typeCount; i++)
            {
                var sheetColumn = DrawSheetColumn(i, out VisualElement columnHeader);
                scrollView.Add(sheetColumn);
                var columnSeperator = DrawColumnSeperator(new[] { sheetColumn, columnHeader });
                columnSeperators.Add(columnSeperator);
                scrollView.Add(columnSeperator);
            }

            sheetRoot.Add(scrollView);

            leftColumn.BringToFront();
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

            for (int i = 0; i < sheet.lineCount; i++)
            {
                var dataLeft = DrawDataLeft(i);
                leftDataColumn.Add(dataLeft);
            }

            topLeftCorner.BringToFront();
            return leftColumn;
        }

        private VisualElement DrawSheetColumn(int columnId, out VisualElement columnHeader)
        {
            var sheetColumn = new VisualElement();
            sheetColumn.name = "sheetColumn";
            if (columnId == 0)
                sheetColumn.style.marginLeft = 32;

            columnHeader = DrawColumnHeader(columnId);
            sheetColumn.Add(columnHeader);

            for (int i = 0; i < sheet.lineCount; i++)
            {
                var dataField = DrawDataField(columnId, i);
                sheetColumn.Add(dataField);
            }

            columnHeader.BringToFront();
            return sheetColumn;
        }

        private VisualElement DrawColumnSeperator(VisualElement[] targets)
        {
            var columnSeperator = new ColumnSeperator(targets);
            columnSeperator.name = "columnSeperator";
            return columnSeperator;
        }

        private VisualElement DrawDataLeft(int lineId)
        {
            var idField = new Label();
            idField.name = "idField";
            idField.text = lineId.ToString();
            scrollView.horizontalScroller.valueChanged += (value) =>
            {
                idField.style.left = value;
            };
            if(lineId == 0)
                idField.style.marginTop = 42;
            if (lineId % 2 == 0)
                idField.AddToClassList("id-row-even");
            else
                idField.AddToClassList("id-row-odd");
            return idField;
        }

        private VisualElement DrawColumnHeader(int columnId)
        {
            var columnHeader = new VisualElement();
            columnHeader.name = "columnHeader";
            scrollView.verticalScroller.valueChanged += (value) =>
            {
                columnHeader.style.top = value;
            };

            var titleFieldContainer = DrawTitleFieldContainer(columnId);

            var reorderButton = new VisualElement();
            reorderButton.name = "reorderButton";

            columnHeader.Add(titleFieldContainer);
            columnHeader.Add(reorderButton);
            return columnHeader;
        }

        private VisualElement DrawDataField(int columnId, int lineId)
        {
            var dataRoot = new VisualElement();
            dataRoot.name = "dataRoot";
            if(lineId == 0)
                dataRoot.style.marginTop = 42;
            string typeName = sheet.types[columnId];
            string value = sheet.elements[lineId * sheet.typeCount + columnId];
            var dataField = sheetFieldTypeDrawer.DrawField(typeName, value);
            if (lineId % 2 == 0)
                dataRoot.AddToClassList("data-row-even");
            else
                dataRoot.AddToClassList("data-row-odd");
            dataRoot.Add(dataField);
            return dataRoot;
        }

        private VisualElement DrawTitleFieldContainer(int columnId)
        {
            var titleFieldContainer = new VisualElement();
            titleFieldContainer.name = "headerFieldContainer";

            var typeField = DrawTypeField(columnId);
            titleFieldContainer.Add(typeField);

            var headerField = DrawHeaderField(columnId);
            titleFieldContainer.Add(headerField);

            return titleFieldContainer;
        }

        private VisualElement DrawTypeField(int columnId)
        {
            var typeField = new DropdownField(sheetFieldTypeDrawer.TypeNameList, 0);
            typeField.name = "typeField";
            typeField.value = sheet.types[columnId];
            return typeField;
        }
        
        private VisualElement DrawHeaderField(int columnId)
        {
            var headerField = new TextField();
            headerField.name = "headerField";
            headerField.value = sheet.headers[columnId];
            return headerField;
        }        


        private Sheet sheet;
        private SerializedObject serializedObject;
        private SheetFieldTypeDrawer sheetFieldTypeDrawer;
        private VisualElement root;
        private ObjectField objField;
        private VisualElement sheetRoot;
        private ScrollView scrollView;
        private List<VisualElement> boxesSeperator = new List<VisualElement>();
        private List<VisualElement> columnSeperators = new List<VisualElement>();

        private const string typesPropertyName = "types";
        private const string typeCountPropertyName = "typeCount";
        private const string headersPropertyName = "headers";
        private const string elementsPropertyName = "elements";

        private const int defaultBoxWidth = 100;
        private const int defaultBoxHeight = 20;
        private const int defaultBoxHorizontalSpace = 2;
        private const int defaultBoxVerticalSpace = 1;

        private const string styleSheetPath = "Assets/HNUnityFramework/Editor/Sheet/SheetEditor.uss";
    
    
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
