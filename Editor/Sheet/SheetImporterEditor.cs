using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace HN.Framework.Editor
{
    /// <summary>
    /// Sheet 导入器的自定义 Inspector 编辑器。
    /// </summary>
    [CustomEditor(typeof(SheetImporter))]
    public class SheetImporterEditor : ScriptedImporterEditor
    {
        /// <summary>
        /// 启用编辑器时调用，初始化导入器和 Sheet 引用。
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            importer = serializedObject.targetObject as SheetImporter;
            sheet = AssetDatabase.LoadAssetAtPath<Sheet>(importer.assetPath);
        }

        /// <summary>
        /// 绘制自定义 Inspector GUI，提供打开 Sheet 编辑器和保存按钮。
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Sheet Editor"))
            {
                OnOpenSheetEditor();
            }

            if (GUILayout.Button("Save"))
            {
                Save();
            }
        }


        private void OnOpenSheetEditor()
        {
            if (sheet != null)
                SheetEditor.OpenWindow(sheet);
        }

        private void Save()
        {
            if (sheet != null)
                sheet.SaveAsset();
        }


        private SheetImporter importer;
        private Sheet sheet;
    }
}
