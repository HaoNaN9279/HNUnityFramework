using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace HN.Framework.Editor
{
    [CustomPropertyDrawer(typeof(HNDictionary), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer 
    {
        private SerializedProperty listProperty;

        private SerializedProperty getListProperty(SerializedProperty property) =>
            listProperty ??= property.FindPropertyRelative("list");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            EditorGUI.PropertyField(position, getListProperty(property), label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
        {
            return EditorGUI.GetPropertyHeight(getListProperty(property), true);
        }
    }
}
