using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public class SheetFieldTypeDrawer
    {
        public SheetFieldTypeDrawer(SerializedProperty elementsProperty)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => typeof(ISheetFieldTypeEditor).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => new { Type = t, Attr = t.GetCustomAttribute<SheetFieldTypeAttribute>() })
                        .Where(x => x.Attr != null);
            foreach (var item in types)
            {
                drawerDict[item.Attr.typeName] = item.Type;
            }
            typeNameList = drawerDict.Keys.ToList();
            this.elementsProperty = elementsProperty;
        }

        public VisualElement DrawField(string typeName, int elementId, string value)
        {
            if (drawerDict.TryGetValue(typeName, out var type))
            {
                var editor = (ISheetFieldTypeEditor)Activator.CreateInstance(type);
                return editor.DrawField(elementsProperty, elementId, value);
            }
            return null;
        }


        public IReadOnlyDictionary<string, Type> DrawerDict => drawerDict;
        public List<string> TypeNameList => typeNameList;
        
        private Dictionary<string, Type> drawerDict = new();
        private List<string> typeNameList;
        private SerializedProperty elementsProperty;
    }
}
