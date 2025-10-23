using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public class SheetFieldTypeDrawer
    {
        public SheetFieldTypeDrawer()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => typeof(ISheetFieldTypeEditor).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => new { Type = t, Attr = t.GetCustomAttribute<SheetFieldTypeAttribute>() })
                        .Where(x => x.Attr != null);
            foreach (var item in types)
            {
                drawerDict[item.Attr.typeName] = item.Type;
            }
        }
        
        public VisualElement DrawField(string typeName, string value)
        {
            if (drawerDict.TryGetValue(typeName, out var type))
            {
                var editor = (ISheetFieldTypeEditor)Activator.CreateInstance(type);
                return editor.DrawField(value);
            }
            return null;
        }


        public IReadOnlyDictionary<string, Type> DrawerDict => drawerDict;
        private Dictionary<string, Type> drawerDict = new();

    }
}
