using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Text.RegularExpressions;

namespace HN.Framework.Editor
{
    [CustomEditor(typeof(AddressablesAssetsGroupPresets))]
    public class AddressablesAssetsGroupPresetsEditor : UnityEditor.Editor
    {
        void OnEnable()
        {
            m_SerializedObject = new SerializedObject(target);
            m_GroupPresetsProperty = m_SerializedObject.FindProperty(GroupPresetsPropertyName);
            m_List = new ReorderableList(m_SerializedObject, m_GroupPresetsProperty, true, false, true, true);
            m_List.drawElementCallback = OnDrawElementCallback;
        }

        public override void OnInspectorGUI()
        {
            m_List.DoLayoutList();
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_GroupPresetsProperty.GetArrayElementAtIndex(index);
            var GroupNameProperty = element.FindPropertyRelative(GroupNamePropertyName);
            var GroupNamePropertyRect = new Rect(rect.x + 2, rect.y, rect.width * 0.4f, rect.height);
            EditorGUI.PropertyField(GroupNamePropertyRect, GroupNameProperty, GUIContent.none);

            var PathKeywordsProperty = element.FindPropertyRelative(PathKeywordsPropertyName);
            var PathKeywordsPropertyRect = new Rect(GroupNamePropertyRect.x + GroupNamePropertyRect.width + 2, rect.y, rect.width * 0.6f - 2, rect.height);
            EditorGUI.PropertyField(PathKeywordsPropertyRect, PathKeywordsProperty, GUIContent.none);

            m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }


        private SerializedObject m_SerializedObject;
        private SerializedProperty m_GroupPresetsProperty;
        private ReorderableList m_List;

        private const string GroupPresetsPropertyName = "m_GroupPresets";
        private const string GroupNamePropertyName = "m_GroupName";
        private const string PathKeywordsPropertyName = "m_PathKeywords";
    }
}
