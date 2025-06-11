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
        public static VisualElement DrawProperty(HNGraphInspectableInfo attribute, JsonData jsonData, PropertyInfo propertyInfo)
        {
            return attribute.Inspect(jsonData, propertyInfo);
        }

        public static VisualElement DrawProperties(HNGraphNode nodeData, BindingFlags bindingFlags)
        {
            VisualElement root = new VisualElement();

            JsonData jsonData = nodeData?.NodeData;
            JsonObject jsonObject = jsonData.Obj;
            Type objType = jsonObject.GetType();
            foreach(var propertyInfo in objType.GetProperties(bindingFlags))
            {
                foreach(HNGraphInspectableInfo attribute in propertyInfo.GetCustomAttributes(typeof(HNGraphInspectableInfo), false))
                {
                    var propertyField = DrawProperty(attribute, jsonData, propertyInfo);
                    root.Add(propertyField);
                }
            }

            return root;
        }
    }


}
