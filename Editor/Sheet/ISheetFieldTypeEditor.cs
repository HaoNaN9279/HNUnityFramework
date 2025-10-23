using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    public interface ISheetFieldTypeEditor
    {
        public VisualElement DrawField(string value);
    }
}
