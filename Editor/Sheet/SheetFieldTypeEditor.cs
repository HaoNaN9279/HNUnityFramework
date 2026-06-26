using System.ComponentModel;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    /// <summary>
    /// Sheet 字段类型编辑器的接口。
    /// </summary>
    public interface ISheetFieldTypeEditor
    {
        /// <summary>
        /// 绘制字段编辑器 UI。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 VisualElement。</returns>
        public VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value);
    }


    /// <summary>
    /// Sheet 字段类型编辑器的抽象基类，提供基础类型转换器。
    /// </summary>
    public abstract class SheetFieldTypeEditor : ISheetFieldTypeEditor
    {
        /// <summary>
        /// 绘制字段编辑器 UI。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 VisualElement。</returns>
        public abstract VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value);


        public static TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
        public static TypeConverter floatConverter = TypeDescriptor.GetConverter(typeof(float));
        public static TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
        
    }


    /// <summary>
    /// INT 类型的 Sheet 字段编辑器绘制器。
    /// </summary>
    [SheetFieldType("INT")]
    public class SheetFieldTypeIntDrawer : SheetFieldTypeEditor
    {
        /// <summary>
        /// 绘制整数字段编辑器 UI。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 IntegerField。</returns>
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
                if (elementsProperty.serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(elementsProperty.serializedObject.targetObject);
            });
            return intField;
        }


        private const int defaultValue = 0;
    }


    /// <summary>
    /// FLOAT 类型的 Sheet 字段编辑器绘制器。
    /// </summary>
    [SheetFieldType("FLOAT")]
    public class SheetFieldTypeFloatDrawer : SheetFieldTypeEditor
    {
        /// <summary>
        /// 绘制浮点数字段编辑器 UI。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 FloatField。</returns>
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
                if (elementsProperty.serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(elementsProperty.serializedObject.targetObject);
            });
            return floatField;
            }


        private const float defaultValue = 0.0f;
    }


    /// <summary>
    /// STRING 类型的 Sheet 字段编辑器绘制器。
    /// </summary>
    [SheetFieldType("STRING")]
    public class SheetFieldTypeStringDrawer : SheetFieldTypeEditor
    {
        /// <summary>
        /// 绘制文本字段编辑器 UI。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 TextField。</returns>
        public override VisualElement DrawField(SerializedProperty elementsProperty, int elementId, string value)
        {
            var stringField = new TextField();
            stringField.value = value;
            stringField.tooltip = value;
            stringField.RegisterValueChangedCallback((e) =>
            {
                elementsProperty.GetArrayElementAtIndex(elementId).stringValue = e.newValue.ToString();
                if (elementsProperty.serializedObject.ApplyModifiedProperties())
                    EditorUtility.SetDirty(elementsProperty.serializedObject.targetObject);
            });
            return stringField;
        }
    }
}
