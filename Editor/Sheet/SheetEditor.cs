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
            }
        }

        private void DrawSheet(Sheet sheet)
        {
            this.sheet = sheet;
            titleContent = new GUIContent($"{sheet.name}[SheetEditor]");
            serializedObject = new SerializedObject(sheet);
            sheetFieldTypeDrawer = new SheetFieldTypeDrawer();

            var scrollView = new ScrollView();
            scrollView.name = "scrollView";

            var titlesField = DrawTitles();

            var dataField = DrawData();

            scrollView.Add(titlesField);
            scrollView.Add(dataField);
            dataField.PlaceBehind(titlesField);
            sheetRoot.Add(scrollView);

            scrollView.verticalScroller.valueChanged += (value) =>
            {
                titlesField.style.top = value;
            };
        }

        private VisualElement DrawTitles()
        {
            var titlesRoot = new VisualElement();
            titlesRoot.name = "titlesRoot";
            var box = new VisualElement();
            var idHeaderField = new VisualElement();
            idHeaderField.name = "idHeaderField";
            var typesField = DrawTypes();
            var headersField = DrawHeaders();
            var descriptionsField = DrawDescriptions();
            box.Add(typesField);
            box.Add(headersField);
            box.Add(descriptionsField);
            titlesRoot.Add(idHeaderField);
            titlesRoot.Add(box);
            return titlesRoot;
        }

        private VisualElement DrawData()
        {
            var dataRoot = new VisualElement();
            dataRoot.name = "dataRoot";
            for (int i = 0; i < sheet.elements.Count / sheet.typeCount; i++)
            {
                var dataLineField = DrawDataLine(i);
                dataRoot.Add(dataLineField);
                if (i == sheet.elements.Count / sheet.typeCount - 1)
                {
                    var boxVerticalSpace = DrawBoxVerticalSpace();
                    dataRoot.Add(boxVerticalSpace);
                }
            }
            return dataRoot;
        }

        private VisualElement DrawTypes()
        {
            var typesRoot = new VisualElement();
            typesRoot.name = "typesRoot";
            for (int i = 0; i < sheet.typeCount; i++)
            {
                var typeField = DrawTypeField(sheet.types[i]);
                typesRoot.Add(typeField);
                if (i != sheet.typeCount - 1)
                {
                    var boxHorizontalSpace = DrawBoxHorizontalSpace();
                    typesRoot.Add(boxHorizontalSpace);
                }
            }
            return typesRoot;
        }

        private VisualElement DrawHeaders()
        {
            var headersRoot = new VisualElement();
            headersRoot.name = "headersRoot";
            for (int i = 0; i < sheet.typeCount; i++)
            {
                var headerField = DrawHeaderField(sheet.headers[i]);
                headersRoot.Add(headerField);
                if (i != sheet.typeCount - 1)
                {
                    var boxHorizontalSpace = DrawBoxHorizontalSpace();
                    headersRoot.Add(boxHorizontalSpace);
                }
            }
            return headersRoot;
        }

        private VisualElement DrawDescriptions()
        {
            var descriptionsRoot = new VisualElement();
            descriptionsRoot.name = "descriptionsRoot";
            for (int i = 0; i < sheet.typeCount; i++)
            {
                var descriptionField = DrawDescriptionField(sheet.descriptions[i]);
                descriptionsRoot.Add(descriptionField);
                if (i != sheet.typeCount - 1)
                {
                    var boxHorizontalSpace = DrawBoxHorizontalSpace();
                    descriptionsRoot.Add(boxHorizontalSpace);
                }
            }
            return descriptionsRoot;
        }
        
        private VisualElement DrawDataLine(int lineId)
        {
            var dataLineRoot = new VisualElement();
            dataLineRoot.name = "dataLineRoot";
            var idField = new Label();
            idField.name = "idField";
            idField.text = lineId.ToString();
            dataLineRoot.Add(idField);
            for (int i = 0; i < sheet.typeCount; i++)
            {
                var dataFieldRoot = new VisualElement();
                dataFieldRoot.name = "dataFieldRoot";
                int dataId = lineId * sheet.typeCount + i;
                var dataField = sheetFieldTypeDrawer.DrawField(sheet.types[i], sheet.elements[dataId]);
                dataField.name = "dataField";
                dataFieldRoot.Add(dataField);
                dataLineRoot.Add(dataFieldRoot);
                if (i != sheet.typeCount - 1)
                {
                    var boxHorizontalSpace = DrawBoxHorizontalSpace();
                    dataLineRoot.Add(boxHorizontalSpace);
                }
            }
            return dataLineRoot;
        }

        private VisualElement DrawTypeField(string typeName)
        {
            var typeRoot = new VisualElement();
            typeRoot.name = "typeRoot";
            var typeList = sheetFieldTypeDrawer.DrawerDict.Keys.ToList();
            var typeField = new DropdownField(typeList, 0);
            typeField.name = "typeField";
            typeField.value = typeName;
            typeRoot.Add(typeField);
            return typeRoot;
        }

        private VisualElement DrawHeaderField(string header)
        {
            var headerRoot = new VisualElement();
            headerRoot.name = "headerRoot";
            var headerField = new TextField();
            headerField.name = "headerField";
            headerField.value = header;
            headerRoot.Add(headerField);
            return headerRoot;
        }
        
        private VisualElement DrawDescriptionField(string description)
        {
            var descriptionRoot = new VisualElement();
            descriptionRoot.name = "descriptionRoot";
            var descriptionField = new TextField();
            descriptionField.name = "descriptionField";
            descriptionField.value = description;
            descriptionRoot.Add(descriptionField);
            return descriptionRoot;
        }

        private VisualElement DrawBoxHorizontalSpace()
        {
            var horizontalSpace = new VisualElement();
            horizontalSpace.name = "horizontalSpace";
            return horizontalSpace;
        }
        
        private VisualElement DrawBoxVerticalSpace()
        {
            var verticalSpace = new VisualElement();
            verticalSpace.name = "verticalSpace";
            return verticalSpace;
        }



        private Sheet sheet;
        private SerializedObject serializedObject;
        private SheetFieldTypeDrawer sheetFieldTypeDrawer;
        private VisualElement root;
        private ObjectField objField;
        private VisualElement sheetRoot;
        private List<int> boxesWidth = new List<int>();

        private const string typesPropertyName = "types";
        private const string typeCountPropertyName = "typeCount";
        private const string headersPropertyName = "headers";
        private const string elementsPropertyName = "elements";

        private const int defaultBoxWidth = 100;
        private const int defaultBoxHeight = 20;
        private const int defaultBoxHorizontalSpace = 2;
        private const int defaultBoxVerticalSpace = 1;

        private const string styleSheetPath = "Assets/HNUnityFramework/Editor/Sheet/SheetEditor.uss";
    }
}
