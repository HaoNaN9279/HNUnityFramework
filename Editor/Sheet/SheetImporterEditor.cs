using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace HN.Framework.Editor
{
    [CustomEditor(typeof(SheetImporter))]
    public class SheetImporterEditor : ScriptedImporterEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Sheet Editor"))
            {
                OnOpenSheetEditor();
            }
        }
        

        private void OnOpenSheetEditor()
        {
            var importer = serializedObject.targetObject as SheetImporter;
            var sheet = AssetDatabase.LoadAssetAtPath<Sheet>(importer.assetPath);
            if(sheet != null)
                SheetEditor.OpenWindow(sheet);
        }
    }
}
