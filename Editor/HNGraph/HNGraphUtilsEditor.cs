using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HN.Serialize;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Graph.Editor
{
    public class HNGraphUtilsEditor
    {
        public static VisualElement DrawField(HNGraphInspectableInfo attribute, JsonData jsonData, FieldInfo fieldInfo)
        {
            return attribute.Inspect(jsonData, fieldInfo);
        }

        public static VisualElement DrawFields(HNGraphNode nodeData, BindingFlags bindingFlags)
        {
            VisualElement root = new VisualElement();

            JsonData jsonData = nodeData?.NodeData;
            JsonObject jsonObject = jsonData.Obj;
            Type objType = jsonObject.GetType();
            foreach(var fieldInfo in objType.GetFields(bindingFlags))
            {
                foreach(HNGraphInspectableInfo attribute in fieldInfo.GetCustomAttributes(typeof(HNGraphInspectableInfo), false))
                {
                    var propertyField = DrawField(attribute, jsonData, fieldInfo);
                    root.Add(propertyField);
                }
            }

            return root;
        }
    }


}
