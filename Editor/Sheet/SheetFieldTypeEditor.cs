using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public abstract class SheetFieldTypeEditor : ISheetFieldTypeEditor
    {
        public abstract VisualElement DrawField(string value);


        public static TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
        public static TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
        public static TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
        
    }


    [SheetFieldType("INT")]
    public class SheetFieldTypeIntDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(string value)
        {
            var intField = new IntegerField();
            var valueObj = intConverter.ConvertFromString(value);
            intField.value = valueObj == null ? defaultValue : (int)valueObj;
            return intField;
        }


        private const int defaultValue = 0;
    }


    [SheetFieldType("FLOAT")]
    public class SheetFieldTypeFloatDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(string value)
        {
            var floatField = new FloatField();
            var valueObj = floatConverter.ConvertFromString(value);
            floatField.value = valueObj == null ? defaultValue : (float)valueObj;
            return floatField;
        }


        private const float defaultValue = 0.0f;
    }


    [SheetFieldType("STRING")]
    public class SheetFieldTypeStringDrawer : SheetFieldTypeEditor
    {
        public override VisualElement DrawField(string value)
        {
            var stringField = new TextField();
            stringField.value = value;
            return stringField;
        }
    }
}
