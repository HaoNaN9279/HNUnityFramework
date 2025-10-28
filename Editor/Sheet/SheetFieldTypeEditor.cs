using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public interface ISheetFieldTypeEditor
    {
        public VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value);
    }


    public abstract class SheetFieldTypeEditor : ISheetFieldTypeEditor
    {
        public abstract VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value);


        public static TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
        public static TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
        public static TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
        
    }


    [SheetFieldType("INT")]
    public class SheetFieldTypeIntDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value)
        {
            var intField = new IntegerField();
            if (int.TryParse(value, out int result))
            {
                intField.value = result;
            }
            else
            {
                intField.value = defaultValue;
            }
            intField.tooltip = value;
            intField.RegisterValueChangedCallback((e) =>
            {
                elementsProperty.GetArrayElementAtIndex(elementId).stringValue = e.newValue.ToString();
                elementsProperty.serializedObject.ApplyModifiedProperties();
            });
            return intField;
        }


        private const int defaultValue = 0;
    }


    [SheetFieldType("FLOAT")]
    public class SheetFieldTypeFloatDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value)
        {
            var floatField = new FloatField();
            if (float.TryParse(value, out float result))
            {
                floatField.value = result;
            }
            else
            {
                floatField.value = defaultValue;
            }
            floatField.tooltip = value;
            floatField.RegisterValueChangedCallback((e) =>
            {
                elementsProperty.GetArrayElementAtIndex(elementId).stringValue = e.newValue.ToString();
                elementsProperty.serializedObject.ApplyModifiedProperties();
            });
            return floatField;
            }


        private const float defaultValue = 0.0f;
    }


    [SheetFieldType("STRING")]
    public class SheetFieldTypeStringDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value)
        {
            var stringField = new TextField();
            stringField.value = value;
            stringField.tooltip = value;
            stringField.RegisterValueChangedCallback((e) =>
            {
                elementsProperty.GetArrayElementAtIndex(elementId).stringValue = e.newValue.ToString();
                elementsProperty.serializedObject.ApplyModifiedProperties();
            });
            return stringField;
        }
    }
}
